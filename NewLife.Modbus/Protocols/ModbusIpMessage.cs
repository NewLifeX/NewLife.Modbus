using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusTcp、ModbusUdp消息</summary>
public class ModbusIpMessage : ModbusMessage
{
    #region 属性
    /// <summary>事务元标识符。主要用于在主站设备在接收到响应时能知道是哪个请求的响应</summary>
    public UInt16 TransactionId { get; set; }

    /// <summary>协议标识符。对于MODBUS协议来说，这里恒为0</summary>
    public UInt16 ProtocolId { get; set; }
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Read(Stream stream, Object context)
    {
        var binary = context as Binary ?? new Binary { Stream = stream, IsLittleEndian = false };

        TransactionId = binary.Read<UInt16>();
        ProtocolId = binary.Read<UInt16>();

        var len = binary.Read<UInt16>();
        if (len < 1 + 1 || stream.Position + len > stream.Length) return false;

        return base.Read(stream, context ?? binary);
    }

    /// <summary>解析消息</summary>
    /// <param name="data">数据包</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public new static ModbusIpMessage Read(Packet data, Boolean reply = false)
    {
        var msg = new ModbusIpMessage { Reply = reply };
        return msg.Read(data.GetStream(), null) ? msg : null;
    }

    /// <summary>写入消息到数据流</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Write(Stream stream, Object context)
    {
        var binary = context as Binary ?? new Binary { Stream = stream, IsLittleEndian = false };

        binary.Write(TransactionId);
        binary.Write(ProtocolId);

        var pk = Payload;
        var len = 2 + (pk?.Total ?? 0);
        binary.Write((UInt16)len);

        return base.Write(stream, context ?? binary);
    }

    /// <summary>创建响应</summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override ModbusMessage CreateReply()
    {
        if (Reply) throw new InvalidOperationException();

        var msg = new ModbusIpMessage
        {
            Reply = true,
            TransactionId = TransactionId,
            ProtocolId = ProtocolId,
            Host = Host,
            Code = Code,
        };

        return msg;
    }
    #endregion
}