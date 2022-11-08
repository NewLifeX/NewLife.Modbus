using System.Diagnostics;
using NewLife.IoT.Protocols;
using NewLife.IoT.ThingModels;
using NewLife.IoT.ThingSpecification;
using NewLife.Reflection;
using NewLife.Serialization;

namespace NewLife.IoT.Drivers;

/// <summary>
/// Modbus协议封装
/// </summary>
public abstract class ModbusDriver : DriverBase
{
    #region 属性
    /// <summary>
    /// Modbus通道
    /// </summary>
    public Modbus Modbus { get; set; }

    private Int32 _nodes;
    #endregion

    #region 构造
    /// <summary>
    /// 销毁时，关闭连接
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Modbus.TryDispose();
        Modbus = null;
    }
    #endregion

    #region 元数据
    #endregion

    #region 方法
    /// <summary>
    /// 创建Modbus通道
    /// </summary>
    /// <param name="device">逻辑设备</param>
    /// <param name="node">设备节点</param>
    /// <param name="parameters">参数</param>
    /// <returns></returns>
    internal protected abstract Modbus CreateModbus(IDevice device, ModbusNode node, IDictionary<String, Object> parameters);

    /// <summary>
    /// 打开通道。一个ModbusTcp设备可能分为多个通道读取，需要共用Tcp连接，以不同节点区分
    /// </summary>
    /// <param name="device">通道</param>
    /// <param name="parameters">参数</param>
    /// <returns></returns>
    public override INode Open(IDevice device, IDictionary<String, Object> parameters)
    {
        var p = JsonHelper.Convert<ModbusParameter>(parameters);

        var node = new ModbusNode
        {
            Host = p.Host,
            ReadCode = p.ReadCode,
            WriteCode = p.WriteCode,

            Driver = this,
            Device = device,
            Parameter = p,
        };

        // 实例化一次Tcp连接
        if (Modbus == null)
        {
            lock (this)
            {
                if (Modbus == null)
                {
                    var modbus = CreateModbus(device, node, parameters);
                    if (p.Timeout > 0) modbus.Timeout = p.Timeout;

                    // 外部已指定通道时，打开连接
                    if (device != null) modbus.Open();

                    Modbus = modbus;
                    //node.Modbus = modbus;
                }
            }
        }

        Interlocked.Increment(ref _nodes);

        return node;
    }

    /// <summary>
    /// 关闭设备驱动
    /// </summary>
    /// <param name="node"></param>
    public override void Close(INode node)
    {
        if (Interlocked.Decrement(ref _nodes) <= 0)
        {
            Modbus.TryDispose();
            Modbus = null;
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="node">节点对象，可存储站号等信息，仅驱动自己识别</param>
    /// <param name="points">点位集合</param>
    /// <returns></returns>
    public override IDictionary<String, Object> Read(INode node, IPoint[] points)
    {
        if (points == null || points.Length == 0) return null;

        var n = node as ModbusNode;
        var p = node.Parameter as ModbusParameter;

        // 组合多个片段，减少读取次数
        //var merge = p != null && (p.ReadCode == FunctionCodes.ReadRegister || p.ReadCode == FunctionCodes.ReadInput);
        var list = BuildSegments(points, p);

        // 加锁，避免冲突
        lock (Modbus)
        {
            // 分段整体读取
            for (var i = 0; i < list.Count; i++)
            {
                var seg = list[i];

                //var code = seg.ReadCode > 0 ? seg.ReadCode : n.ReadCode;
                if (seg.ReadCode == 0) seg.ReadCode = n.ReadCode;

                // 读取线圈时，个数向8对齐
                if (seg.ReadCode == FunctionCodes.ReadCoil)
                {
                    var y = seg.Count % 8;
                    seg.Count += 8 - y;
                }

                // 其中一项读取报错时，直接跳过，不要影响其它批次
                try
                {
                    seg.Data = Modbus.Read(seg.ReadCode, n.Host, (UInt16)seg.Address, (UInt16)seg.Count)?.ReadBytes();
                }
                catch (Exception ex)
                {
                    Log?.Error(ex.ToString());
                }

                // 读取时延迟一点时间
                if (i < list.Count - 1 && p.Delay > 0) Thread.Sleep(p.Delay);
            }
        }

        // 分割数据
        return Dispatch(points, list);
    }

    internal IList<Segment> BuildSegments(IList<IPoint> points, ModbusParameter p)
    {
        // 组合多个片段，减少读取次数
        var list = new List<Segment>();
        foreach (var point in points)
        {
            if (ModbusAddress.TryParse(point.Address, out var maddr))
            {
                list.Add(new Segment
                {
                    ReadCode = maddr.GetReadCode(),
                    Address = maddr.Address,
                    Count = GetCount(point)
                });
            }
        }
        list = list.OrderBy(e => e.ReadCode).ThenBy(e => e.Address).ThenByDescending(e => e.Count).ToList();

        //// 只有读寄存器合并，其它指令合并可能有问题，将来再优化
        //if (!merge) return list;
        //if (list.Any(e => e.ReadCode != FunctionCodes.ReadRegister && e.ReadCode != FunctionCodes.ReadInput)) return list;

        var k = 1;
        var rs = new List<Segment>();
        var prv = list[0];
        rs.Add(prv);
        for (var i = 1; i < list.Count; i++)
        {
            var cur = list[i];

            // 前一段末尾碰到了当前段开始，可以合并
            var flag = prv.Address + prv.Count >= cur.Address;
            // 如果是读取线圈，间隔小于8都可以合并
            if (!flag && cur.ReadCode == FunctionCodes.ReadCoil)
            {
                flag = prv.Address + prv.Count + 8 > cur.Address;
            }

            // 前一段末尾碰到了当前段开始，可以合并
            if (flag && prv.ReadCode == cur.ReadCode)
            {
                if (p.BatchSize <= 0 || k < p.BatchSize)
                {
                    // 要注意，可能前后重叠，也可能前面区域比后面还大
                    var size = cur.Address + cur.Count - prv.Address;
                    if (size > prv.Count) prv.Count = size;

                    // 连续合并数累加
                    k++;
                }
                else
                {
                    rs.Add(cur);

                    prv = cur;
                    k = 1;
                }
            }
            else
            {
                rs.Add(cur);

                prv = cur;
                k = 1;
            }
        }

        return rs;
    }

    internal IDictionary<String, Object> Dispatch(IPoint[] points, IList<Segment> segments)
    {
        var dic = new Dictionary<String, Object>();
        if (segments == null || segments.Count == 0) return dic;

        foreach (var point in points)
        {
            if (ModbusAddress.TryParse(point.Address, out var maddr))
            {
                var count = GetCount(point);

                // 找到片段
                var seg = segments.FirstOrDefault(e => e.Address <= maddr.Address && maddr.Address + count <= e.Address + e.Count);
                if (seg != null && seg.Data != null)
                {
                    var code = seg.ReadCode;
                    if (code is FunctionCodes.ReadRegister or FunctionCodes.ReadInput)
                    {
                        // 校验数据完整性
                        var offset = (maddr.Address - seg.Address) * 2;
                        var size = count * 2;
                        if (seg.Data.Length >= offset + size)
                            dic[point.Name] = seg.Data.ReadBytes(offset, size);
                    }
                    else if (code is FunctionCodes.ReadCoil or FunctionCodes.ReadDiscrete)
                    {
                        // 计算偏移，每8位一个字节，地址低3位是该字节内的偏移量
                        var offset = maddr.Address - seg.Address;
                        var idx = offset >> 3;
                        offset &= 0x07;
                        if (seg.Data.Length >= idx)
                            dic[point.Name] = (seg.Data[idx] >> offset) & 0x01;
                    }
                    else
                        throw new NotSupportedException($"无法拆分{code}");
                }
            }
        }
        return dic;
    }

    [DebuggerDisplay("{ReadCode}({Address}, {Count})")]
    internal class Segment
    {
        public FunctionCodes ReadCode { get; set; }
        public Int32 Address { get; set; }
        public Int32 Count { get; set; }
        public Byte[] Data { get; set; }
    }

    /// <summary>
    /// 从点位中计算寄存器个数
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public virtual Int32 GetCount(IPoint point)
    {
        // 字节数转寄存器数，要除以2
        var count = point.GetLength() / 2;
        return count > 0 ? count : 1;
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="node">节点对象，可存储站号等信息，仅驱动自己识别</param>
    /// <param name="point">点位</param>
    /// <param name="value">数值</param>
    public override Object Write(INode node, IPoint point, Object value)
    {
        if (value == null) return null;
        if (!ModbusAddress.TryParse(point.Address, out var maddr)) return null;

        var n = node as ModbusNode;
        UInt16[] vs;
        if (value is Byte[] buf)
        {
            vs = new UInt16[(Int32)Math.Ceiling(buf.Length / 2d)];
            for (var i = 0; i < vs.Length; i++)
            {
                vs[i] = buf.ToUInt16(i * 2, false);
            }
        }
        else
        {
            vs = ConvertToRegister(value, point, n.Device?.Specification);

            if (vs == null) throw new NotSupportedException($"点位[{point.Name}]不支持数据[{value}]");
        }

        // 加锁，避免冲突
        lock (Modbus)
        {
            var code = maddr.GetWriteCode();
            if (code == 0) code = n.WriteCode;
            return Modbus.Write(code, n.Host, maddr.Address, vs);
        }
    }

    /// <summary>原始数据转寄存器数组</summary>
    /// <param name="data"></param>
    /// <param name="point"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    private UInt16[] ConvertToRegister(Object data, IPoint point, ThingSpec spec)
    {
        // 找到物属性定义
        var pi = spec?.Properties?.FirstOrDefault(e => e.Id.EqualIgnoreCase(point.Name));
        var type = pi?.DataType?.Type;
        if (type.IsNullOrEmpty()) type = point.Type;
        //if (type.IsNullOrEmpty()) return null;

        var type2 = TypeHelper.GetNetType(point);
        //var value = data.ChangeType(type2);
        switch (type2.GetTypeCode())
        {
            case TypeCode.Boolean:
                return data.ToBoolean() ? new[] { (UInt16)0xFF00 } : new[] { (UInt16)0x00 };
            //case TypeCode.Byte:
            //    break;
            //case TypeCode.Char:
            //    break;
            //case TypeCode.DateTime:
            //    break;
            //case TypeCode.DBNull:
            //    break;
            //case TypeCode.Empty:
            //    break;
            case TypeCode.Int16:
            case TypeCode.UInt16:
                return new[] { (UInt16)data.ToInt() };
            case TypeCode.Int32:
            case TypeCode.UInt32:
                {
                    var n = data.ToInt();
                    return new[] { (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
                }
            case TypeCode.Int64:
            case TypeCode.UInt64:
                {
                    var n = data.ToLong();
                    return new[] { (UInt16)(n >> 48), (UInt16)(n >> 32), (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
                }
            //case TypeCode.Object:
            //    break;
            //case TypeCode.SByte:
            //    break;
            case TypeCode.Single:
                {
                    var d = (Single)data.ToDouble();
                    //var n = BitConverter.SingleToInt32Bits(d);
                    var n = (UInt32)d;
                    return new[] { (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
                }
            case TypeCode.Double:
                {
                    var d = (Double)data.ToDouble();
                    //var n = BitConverter.DoubleToInt64Bits(d);
                    var n = (UInt64)d;
                    return new[] { (UInt16)(n >> 48), (UInt16)(n >> 32), (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
                }
            case TypeCode.Decimal:
                {
                    var d = (Decimal)data.ToDecimal();
                    var n = (UInt64)d;
                    return new[] { (UInt16)(n >> 48), (UInt16)(n >> 32), (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
                }
            //case TypeCode.String:
            //    break;
            default:
                return null;
        }

        //switch (type.ToLower())
        //{
        //    case "short":
        //    case "int16":
        //    case "ushort":
        //    case "uint16":
        //        {
        //            var n = data.ToInt();
        //            return new[] { (UInt16)n };
        //        }
        //    case "int":
        //    case "int32":
        //    case "uint":
        //    case "uint32":
        //        {
        //            var n = data.ToInt();
        //            return new[] { (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
        //        }
        //    case "long":
        //    case "int64":
        //    case "ulong":
        //    case "uint64":
        //        {
        //            var n = data.ToLong();
        //            return new[] { (UInt16)(n >> 48), (UInt16)(n >> 32), (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
        //        }
        //    case "float":
        //    case "single":
        //        {
        //            var d = (Single)data.ToDouble();
        //            //var n = BitConverter.SingleToInt32Bits(d);
        //            var n = (UInt32)d;
        //            return new[] { (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
        //        }
        //    case "double":
        //    case "decimal":
        //        {
        //            var d = data.ToDouble();
        //            //var n = BitConverter.DoubleToInt64Bits(d);
        //            var n = (UInt64)d;
        //            return new[] { (UInt16)(n >> 48), (UInt16)(n >> 32), (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) };
        //        }
        //    case "bool":
        //    case "boolean":
        //        {
        //            return data.ToBoolean() ? new[] { (UInt16)0xFF00 } : new[] { (UInt16)0x00 };
        //        }
        //    default:
        //        return null;
        //}
        //return type.ToLower() switch
        //{
        //    "short" or "int16" or "ushort" or "uint16" => new[] { (UInt16)n },
        //    "int" or "int32" or "uint" or "uint32" => new[] { (UInt16)(n >> 16), (UInt16)(n & 0xFFFF) },
        //    "long" or "int64" or "uint64" => { },
        //    "float" or "single" => BitConverter.SingleToInt32Bits((Single)data.ToDouble()).GetBytes(false),
        //    "double" or "decimal" => BitConverter.DoubleToInt64Bits(data.ToDouble()).GetBytes(false),
        //    "bool" or "boolean" => data.ToBoolean() ? new[] { (UInt16)0xFF00 } : new[] { (UInt16)0x00 },
        //    _ => null,
        //};
    }

    /// <summary>
    /// 控制设备，特殊功能使用
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parameters"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void Control(INode node, IDictionary<String, Object> parameters) => throw new NotImplementedException();
    #endregion
}