using System.Runtime.Serialization;
using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>Modbus消息</summary>
public class ModbusMessage : IAccessor
{
    #region 属性
    /// <summary>是否响应</summary>
    [IgnoreDataMember]
    public Boolean Reply { get; set; }

    /// <summary>站号</summary>
    public Byte Host { get; set; }

    /// <summary>操作码</summary>
    public FunctionCodes Code { get; set; }

    /// <summary>错误码</summary>
    public ErrorCodes ErrorCode { get; set; }

    ///// <summary>地址。请求数据，地址与负载；响应数据没有地址只有负载</summary>
    //public UInt16 Address { get; set; }

    /// <summary>负载数据</summary>
    [IgnoreDataMember]
    public Packet Payload { get; set; }
    #endregion

    #region 构造
    /// <summary>已重载。友好字符串</summary>
    /// <returns></returns>
    public override String ToString()
    {
        var pk = Payload;

        if (!Reply && pk != null && Code <= FunctionCodes.WriteRegisters)
            return $"{Code} (0x{GetAddress():X4}, {pk?.Slice(2).ToHex()})";

        return $"{Code} {pk?.ToHex()}";
    }
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public virtual Boolean Read(Stream stream, Object context)
    {
        var binary = context as Binary ?? new Binary { Stream = stream, IsLittleEndian = false };

        Host = binary.ReadByte();

        var b = binary.ReadByte();
        Code = (FunctionCodes)(b & 0x7F);

        // 异常码
        if ((b & 0x80) == 0x80)
        {
            ErrorCode = (ErrorCodes)binary.ReadByte();
            return true;
        }

        Payload = stream.ReadBytes();

        return true;
    }

    /// <summary>解析消息</summary>
    /// <param name="data">数据包</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public static ModbusMessage Read(Packet data, Boolean reply = false)
    {
        var msg = new ModbusMessage { Reply = reply };
        if (msg.Read(data.GetStream(), null)) return msg;

        return null;
    }

    /// <summary>写入消息到数据流</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public virtual Boolean Write(Stream stream, Object context)
    {
        var binary = context as Binary ?? new Binary { Stream = stream, IsLittleEndian = false };

        binary.Write(Host);

        var b = (Byte)Code;
        if (ErrorCode > 0) b |= 0x80;
        binary.Write(b);

        // 异常码
        if (ErrorCode > 0)
        {
            binary.Write((Byte)ErrorCode);
            return true;
        }

        var pk = Payload;
        if (pk != null) binary.Write(pk.Data, pk.Offset, pk.Count);
        //Payload?.CopyTo(binary.Stream);

        return true;
    }

    /// <summary>消息转数据包</summary>
    /// <returns></returns>
    public Packet ToPacket()
    {
        var ms = new MemoryStream();
        Write(ms, null);

        ms.Position = 0;
        return new Packet(ms);
    }

    /// <summary>创建响应</summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual ModbusMessage CreateReply()
    {
        if (Reply) throw new InvalidOperationException();

        var msg = new ModbusMessage
        {
            Reply = true,
            Host = Host,
            Code = Code,
        };

        return msg;
    }

    /// <summary>获取地址。取负载开始2字节作为地址，基础读写指令都有</summary>
    /// <returns></returns>
    public UInt16 GetAddress() => Payload?.ReadBytes(0, 2).ToUInt16(0, false) ?? 0;

    /// <summary>获取请求地址和数值</summary>
    /// <returns></returns>
    public (UInt16 address, UInt16 count) GetRequest()
    {
        var pk = Payload;
        var address = pk.ReadBytes(0, 2).ToUInt16(0, false);
        var count = pk.ReadBytes(2, 2).ToUInt16(0, false);

        return (address, count);
    }

    /// <summary>设置请求地址和数值，填充负载数据</summary>
    /// <param name="address">地址</param>
    /// <param name="count">寄存器个数</param>
    public void SetRequest(UInt16 address, UInt16 count)
    {
        var buf = new Byte[4];
        buf.Write(address, 0, false);
        buf.Write(count, 2, false);

        Payload = buf;
    }

    /// <summary>设置请求地址和数据，填充负载数据</summary>
    /// <param name="address"></param>
    /// <param name="data"></param>
    public void SetRequest(UInt16 address, Packet data)
    {
        Payload = new Packet(address.GetBytes(false));
        Payload.Append(data);
    }
    #endregion
}