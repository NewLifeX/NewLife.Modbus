using System.Runtime.CompilerServices;
using NewLife.Data;
using NewLife.Log;
using NewLife.Serialization;

[assembly: InternalsVisibleTo("XUnitTest, PublicKey=00240000048000001401000006020000002400005253413100080000010001000d41eb3bdab5c2150958b46c95632b7e4dcb0af77ed8637bd8543875bc2443d01273143bb46655a48a92efa76251adc63ccca6d0e9cef2e0ce93e32b5043bea179a6c710981be4a71703a03e10960643f7df091f499cf60183ef0e4e4e2eebf26e25cea0eebf87c8a6d7f8130c283fc3f747cb90623f0aaa619825e3fcd82f267a0f4bfd26c9f2a6b5a62a6b180b4f6d1d091fce6bd60a9aa9aa5b815b833b44e0f2e58b28a354cb20f52f31bb3b3a7c54f515426537e41f9c20c07e51f9cab8abc311daac19a41bd473a51c7386f014edf1863901a5c29addc89da2f2659c9c1e95affd6997396b9680e317c493e974a813186da277ff9c1d1b30e33cb5a2f6")]

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
    /// <param name="code">功能码</param>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="value">数据值</param>
    /// <returns>返回响应消息的负载部分</returns>
    public virtual Packet SendCommand(FunctionCodes code, Byte host, UInt16 address, UInt16 value)
    {
        var msg = CreateMessage();
        msg.Host = host;
        msg.Code = code;

        msg.SetRequest(address, value);

        var rs = SendCommand(msg);

        return rs?.Payload;
    }

    /// <summary>发送命令，并接收返回</summary>
    /// <param name="code">功能码</param>
    /// <param name="host">主机。一般是1</param>
    /// <param name="data">数据</param>
    /// <returns>返回响应消息的负载部分</returns>
    public virtual Packet SendCommand(FunctionCodes code, Byte host, Packet data)
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
    internal protected abstract ModbusMessage SendCommand(ModbusMessage message);
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
        try
        {
            var rs = SendCommand(FunctionCodes.ReadCoil, host, address, count);
            if (rs == null) return null;

            var len = -1;
            if (ValidResponse)
            {
                len = rs[0];
                if (rs.Total < 1 + len) return null;
            }

            return rs.Slice(1, len);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>读离散量输入，0x02</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">输入数量。一般要求8的倍数</param>
    /// <returns>输入状态字节数组</returns>
    public Packet ReadDiscrete(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadDiscrete", $"host={host} address={address}/0x{address:X4} count={count}");
        try
        {
            var rs = SendCommand(FunctionCodes.ReadDiscrete, host, address, count);
            if (rs == null) return null;

            var len = -1;
            if (ValidResponse)
            {
                len = rs[0];
                if (rs.Total < 1 + len) return null;
            }

            return rs.Slice(1, len);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>读取保持寄存器，0x03</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">寄存器数量。每个寄存器2个字节</param>
    /// <returns>寄存器值数组</returns>
    public Packet ReadRegister(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadRegister", $"host={host} address={address}/0x{address:X4} count={count}");
        try
        {
            var rs = SendCommand(FunctionCodes.ReadRegister, host, address, count);
            if (rs == null) return null;

            var len = -1;
            if (ValidResponse)
            {
                len = rs[0];
                if (rs.Total < 1 + len) return null;
            }

            return rs.Slice(1, len);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>读取输入寄存器，0x04</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="count">输入寄存器数量。每个寄存器2个字节</param>
    /// <returns>输入寄存器值数组</returns>
    public Packet ReadInput(Byte host, UInt16 address, UInt16 count)
    {
        using var span = Tracer?.NewSpan("modbus:ReadInput", $"host={host} address={address}/0x{address:X4} count={count}");
        try
        {
            var rs = SendCommand(FunctionCodes.ReadInput, host, address, count);
            if (rs == null) return null;

            var len = -1;
            if (ValidResponse)
            {
                len = rs[0];
                if (rs.Total < 1 + len) return null;
            }

            return rs.Slice(1, len);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
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
        try
        {
            var rs = SendCommand(FunctionCodes.WriteCoil, host, address, value);
            if (rs == null || rs.Total < 4) return -1;

            // 去掉2字节地址
            return rs.ReadBytes(2, 2).ToUInt16(0, false);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>写入保持寄存器，0x06</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="value">数值</param>
    /// <returns>寄存器值</returns>
    public Int32 WriteRegister(Byte host, UInt16 address, UInt16 value)
    {
        using var span = Tracer?.NewSpan("modbus:WriteRegister", $"host={host} address={address}/0x{address:X4} value=0x{value:X4}");
        try
        {
            var rs = SendCommand(FunctionCodes.WriteRegister, host, address, value);
            if (rs == null || rs.Total < 4) return -1;

            // 去掉2字节地址
            return rs.ReadBytes(2, 2).ToUInt16(0, false);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>写多个线圈，0x0F</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="values">值。一般是 0xFF00/0x0000</param>
    /// <returns>数量</returns>
    public Int32 WriteCoils(Byte host, UInt16 address, UInt16[] values)
    {
        using var span = Tracer?.NewSpan("modbus:WriteCoils", $"host={host} address={address}/0x{address:X4} values={values.Join("-", e => e.ToString("X4"))}");
        try
        {
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

            var rs = SendCommand(FunctionCodes.WriteCoils, host, pk);
            if (rs == null || rs.Total < 4) return -1;

            // 去掉2字节地址
            return rs.ReadBytes(2, 2).ToUInt16(0, false);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>写多个保持寄存器，0x10</summary>
    /// <param name="host">主机。一般是1</param>
    /// <param name="address">地址。例如0x0002</param>
    /// <param name="values">数值</param>
    /// <returns>寄存器数量</returns>
    public Int32 WriteRegisters(Byte host, UInt16 address, UInt16[] values)
    {
        using var span = Tracer?.NewSpan("modbus:WriteRegisters", $"host={host} address={address}/0x{address:X4} values={values.Join("-", e => e.ToString("X4"))}");
        try
        {
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

            var rs = SendCommand(FunctionCodes.WriteRegisters, host, pk);
            if (rs == null || rs.Total < 4) return -1;

            // 去掉2字节地址
            return rs.ReadBytes(2, 2).ToUInt16(0, false);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
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