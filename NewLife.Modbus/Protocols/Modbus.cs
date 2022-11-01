using NewLife.Data;
using NewLife.Log;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>Modbus协议核心</summary>
public abstract class Modbus : DisposeBase
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>网络超时。发起请求后等待响应的超时时间，默认3000ms</summary>
    public Int32 Timeout { get; set; } = 3000;

    /// <summary>缓冲区大小。默认256</summary>
    public Int32 BufferSize { get; set; } = 256;

    /// <summary>校验响应数据长度。默认true</summary>
    public Boolean ValidResponse { get; set; } = true;

    /// <summary>性能追踪器</summary>
    public ITracer Tracer { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public Modbus() => Name = GetType().Name;
    #endregion

    #region 核心方法
    /// <summary>初始化。传入配置</summary>
    /// <param name="parameters"></param>
    public virtual void Init(IDictionary<String, Object> parameters) { }

    /// <summary>打开</summary>
    public virtual void Open() { }

    /// <summary>创建消息</summary>
    /// <returns></returns>
    protected virtual ModbusMessage CreateMessage() => new();

    /// <summary>发送命令，并接收返回</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="code">功能码</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="value">数据值</param>
    /// <returns>返回响应消息的负载部分</returns>
    public virtual Packet SendCommand(Byte host, FunctionCodes code, UInt16 address, UInt16 value)
    {
        var msg = CreateMessage();
        msg.Host = host;
        msg.Code = code;

        msg.SetRequest(address, value);

        var rs = SendCommand(msg);

        return rs?.Payload;
    }

    /// <summary>发送命令，并接收返回</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="code">功能码</param>
    /// <param name="data">数据</param>
    /// <returns>返回响应消息的负载部分</returns>
    public virtual Packet SendCommand(Byte host, FunctionCodes code, Packet data)
    {
        var msg = CreateMessage();
        msg.Host = host;
        msg.Code = code;
        msg.Payload = data;

        var rs = SendCommand(msg);

        return rs?.Payload;
    }
    /// <summary>发送消息并接收返回</summary>
    /// <param name="message">Modbus消息</param>
    /// <returns></returns>
    protected abstract ModbusMessage SendCommand(ModbusMessage message);
    #endregion

    #region 读取
    /// <summary>按功能码读取。用于IoT标准库</summary>
    /// <param name="code">功能码</param>
    /// <param name="host">主机</param>
    /// <param name="address">逻辑地址</param>
    /// <param name="count">个数。寄存器个数或线圈个数</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual Packet Read(FunctionCodes code, Byte host, UInt16 address, UInt16 count)
    {
        switch (code)
        {
            case FunctionCodes.ReadCoil: return ReadCoil(host, address, count);
            case FunctionCodes.ReadDiscrete: return ReadDiscrete(host, address, count);
            case FunctionCodes.ReadRegister: return ReadRegister(host, address, count);
            case FunctionCodes.ReadInput: return ReadInput(host, address, count);
            default:
                break;
        }

        throw new NotSupportedException($"ModbusRead不支持[{code}]");
    }

    /// <summary>读取线圈，0x01</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">线圈数量。一般要求8的倍数</param>
    /// <returns>线圈状态字节数组</returns>
    public Packet ReadCoil(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadCoil", $"host={host} address={address}/0x{address:X4} count={count}");

        var rs = SendCommand(host, FunctionCodes.ReadCoil, address, count);
        if (rs == null) return null;

        if (ValidResponse)
        {
            var len = rs[0];
            if (rs.Total < 1 + len) return null;
        }

        return rs.Slice(1);
    }

    /// <summary>读离散量输入，0x02</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">输入数量。一般要求8的倍数</param>
    /// <returns>输入状态字节数组</returns>
    public Packet ReadDiscrete(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadDiscrete", $"host={host} address={address}/0x{address:X4} count={count}");

        var rs = SendCommand(host, FunctionCodes.ReadDiscrete, address, count);
        if (rs == null) return null;

        if (ValidResponse)
        {
            var len = rs[0];
            if (rs.Total < 1 + len) return null;
        }

        return rs.Slice(1);
    }

    /// <summary>读取保持寄存器，0x03</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">寄存器数量。每个寄存器2个字节</param>
    /// <returns>寄存器值数组</returns>
    public Packet ReadRegister(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadRegister", $"host={host} address={address}/0x{address:X4} count={count}");

        var rs = SendCommand(host, FunctionCodes.ReadRegister, address, count);
        if (rs == null) return null;

        if (ValidResponse)
        {
            var len = rs[0];
            if (rs.Total < 1 + len) return null;
        }

        return rs.Slice(1);
    }

    /// <summary>读取输入寄存器，0x04</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">输入寄存器数量。每个寄存器2个字节</param>
    /// <returns>输入寄存器值数组</returns>
    public Packet ReadInput(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadInput", $"host={host} address={address}/0x{address:X4} count={count}");

        var rs = SendCommand(host, FunctionCodes.ReadInput, address, count);
        if (rs == null) return null;

        if (ValidResponse)
        {
            var len = rs[0];
            if (rs.Total < 1 + len) return null;
        }

        return rs.Slice(1);
    }
    #endregion

    #region 写入
    /// <summary>按功能码写入。用于IoT标准库</summary>
    /// <param name="code">功能码</param>
    /// <param name="host">主机</param>
    /// <param name="address">逻辑地址</param>
    /// <param name="values">待写入数值</param>
    /// <returns></returns>
    public virtual Object Write(FunctionCodes code, Byte host, UInt16 address, UInt16[] values)
    {
        switch (code)
        {
            case FunctionCodes.WriteCoil: return WriteCoil(host, address, values[0]);
            case FunctionCodes.WriteRegister: return WriteRegister(host, address, values[0]);
            case FunctionCodes.WriteCoils: return WriteCoils(host, address, values);
            case FunctionCodes.WriteRegisters: return WriteRegisters(host, address, values);
            default:
                break;
        }

        throw new NotSupportedException($"ModbusWrite不支持[{code}]");
    }

    /// <summary>写入单线圈，0x05</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="value">输出值。一般是 0xFF00/0x0000</param>
    /// <returns>输出值</returns>
    public Int32 WriteCoil(Byte host, UInt16 address, UInt16 value)
    {
        using var span = Tracer?.NewSpan("modbus:WriteCoil", $"host={host} address={address}/0x{address:X4} value=0x{value:X4}");

        var rs = SendCommand(host, FunctionCodes.WriteCoil, address, value);
        if (rs == null || rs.Total < 2) return -1;

        // 去掉2字节地址
        return rs.ReadBytes(2, 2).ToUInt16(0, false);
    }

    /// <summary>写入保持寄存器，0x06</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="value">数值</param>
    /// <returns>寄存器值</returns>
    public Int32 WriteRegister(Byte host, UInt16 address, UInt16 value)
    {
        using var span = Tracer?.NewSpan("modbus:WriteRegister", $"host={host} address={address}/0x{address:X4} value=0x{value:X4}");

        var rs = SendCommand(host, FunctionCodes.WriteRegister, address, value);
        if (rs == null || rs.Total < 2) return -1;

        // 去掉2字节地址
        return rs.ReadBytes(2, 2).ToUInt16(0, false);
    }

    /// <summary>写多个线圈，0x0F</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="values">值。一般是 0xFF00/0x0000</param>
    /// <returns>数量</returns>
    public Int32 WriteCoils(Byte host, UInt16 address, UInt16[] values)
    {
        using var span = Tracer?.NewSpan("modbus:WriteCoils", $"host={host} address={address}/0x{address:X4} values={values.Join("-", e => e.ToString("X4"))}");

        // 多个UInt16数值，合并成为负载数据
        var binary = new Binary { IsLittleEndian = false };
        binary.Write(address);
        binary.Write((UInt16)values.Length);

        // 字节数
        binary.Write((Byte)Math.Ceiling(values.Length / 8.0));

        // 数值转位
        var b = 0;
        var k = 0;
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] != 0)
                b |= 1 << k;

            if (k++ >= 7)
            {
                binary.Write((Byte)b);
                b = 0;
                k = 0;
            }
        }
        if (k > 0) binary.Write((Byte)b);

        // 直接使用内存流缓冲区，避免拷贝
        binary.Stream.Position = 0;
        var pk = new Packet(binary.Stream);

        var rs = SendCommand(host, FunctionCodes.WriteCoils, pk);
        if (rs == null || rs.Total < 4) return -1;

        // 去掉2字节地址
        return rs.ReadBytes(2, 2).ToUInt16(0, false);
    }

    /// <summary>写多个保持寄存器，0x10</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="values">数值</param>
    /// <returns>寄存器数量</returns>
    public Int32 WriteRegisters(Byte host, UInt16 address, UInt16[] values)
    {
        using var span = Tracer?.NewSpan("modbus:WriteRegisters", $"host={host} address={address}/0x{address:X4} values={values.Join("-", e => e.ToString("X4"))}");

        // 多个UInt16数值，合并成为负载数据
        var binary = new Binary { IsLittleEndian = false };
        binary.Write(address);
        binary.Write((UInt16)values.Length);

        binary.Write((Byte)(values.Length * 2));
        foreach (var item in values)
        {
            binary.Write(item);
        }

        // 直接使用内存流缓冲区，避免拷贝
        binary.Stream.Position = 0;
        var pk = new Packet(binary.Stream);

        var rs = SendCommand(host, FunctionCodes.WriteRegisters, pk);
        if (rs == null || rs.Total < 4) return -1;

        // 去掉2字节地址
        return rs.ReadBytes(2, 2).ToUInt16(0, false);
    }
    #endregion

    #region 日志
    /// <summary>日志</summary>
    public ILog Log { get; set; }

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}