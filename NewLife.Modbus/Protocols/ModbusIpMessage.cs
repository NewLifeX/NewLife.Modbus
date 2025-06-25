using NewLife.Buffers;

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
    /// <summary>从数据读取消息</summary>
    /// <param name="reader">读取器</param>
    /// <returns></returns>
    public override Boolean Read(ref SpanReader reader)
    {
        TransactionId = reader.ReadUInt16();
        ProtocolId = reader.ReadUInt16();

        var len = reader.ReadUInt16();
        if (len < 1 + 1 || len > reader.FreeCapacity) return false;

        return base.Read(ref reader);
    }

    /// <summary>从数据读取消息</summary>
    /// <param name="data">数据</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public static ModbusIpMessage? Read(ReadOnlySpan<Byte> data, Boolean reply = false)
    {
        var msg = new ModbusIpMessage { Reply = reply };
        var reader = new SpanReader(data) { IsLittleEndian = false };
        return msg.Read(ref reader) ? msg : null;
    }

    /// <summary>写入消息到数据</summary>
    /// <param name="writer">写入器</param>
    /// <returns></returns>
    public override Boolean Write(ref SpanWriter writer)
    {
        writer.Write(TransactionId);
        writer.Write(ProtocolId);

        var pk = Payload;
        var len = 2 + (pk?.Total ?? 0);
        writer.Write((UInt16)len);

        return base.Write(ref writer);
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