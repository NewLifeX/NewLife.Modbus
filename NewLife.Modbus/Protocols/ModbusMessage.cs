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

    /// <summary>地址。请求数据，地址与负载；响应数据没有地址只有负载</summary>
    public UInt16 Address { get; set; }

    /// <summary>负载数据。请求数据，地址与负载；响应数据没有地址只有负载</summary>
    [IgnoreDataMember]
    public Packet Payload { get; set; }
    #endregion

    #region 构造
    /// <summary>已重载。友好字符串</summary>
    /// <returns></returns>
    public override String ToString()
    {
        if (!Reply) return $"{Code} (0x{Address:X4}, {Payload?.ToHex()})";

        return $"{Code} {Payload?.ToHex()}";
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

        var remain = (Int32)(stream.Length - stream.Position);
        if (remain <= 0) return false;

        if (!Reply)
        {
            // 请求数据，地址和负载
            Address = binary.Read<UInt16>();
            Payload = binary.ReadBytes(remain - 2);
        }
        else if (remain >= 1)
        {
            // 响应数据，长度和负载
            var len = binary.ReadByte();
            if (len <= remain - 1) Payload = binary.ReadBytes(len);
        }

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
        if (!Reply)
        {
            // 请求数据，地址和负载
            binary.Write(Address);
            if (pk != null) binary.Write(pk.Data, pk.Offset, pk.Count);
        }
        else
        {
            var len2 = (pk?.Total ?? 0);
            binary.Write((Byte)len2);
            if (pk != null) binary.Write(pk.Data, pk.Offset, pk.Count);
        }

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
    #endregion
}