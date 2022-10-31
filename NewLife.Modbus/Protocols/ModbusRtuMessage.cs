using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusRtu消息</summary>
public class ModbusRtuMessage : ModbusMessage
{
    #region 属性
    /// <summary>CRC校验。小端字节序</summary>
    public UInt16 Crc { get; set; }

    /// <summary>CRC校验。根据数据流计算出来的值</summary>
    public UInt16 Crc2 { get; set; }
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Read(Stream stream, Object context)
    {
        var binary = context as Binary ?? new Binary { Stream = stream, IsLittleEndian = false };

        var p = stream.Position;
        if (!base.Read(stream, context ?? binary)) return false;

        // 从负载数据里把Crc取出来
        var pk = Payload;
        var count = pk?.Total ?? 0;
        if (count >= 2)
        {
            Crc = pk.ReadBytes(count - 2, 2).ToUInt16(0, true);
            Payload = pk.Slice(0, count - 2);
        }

        stream.Position = p;
        Crc2 = ModbusHelper.Crc(stream, (Int32)(stream.Length - stream.Position - 2));

        return true;
    }

    /// <summary>解析消息</summary>
    /// <param name="data">数据包</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public static new ModbusRtuMessage Read(Packet data, Boolean reply = false)
    {
        var msg = new ModbusRtuMessage { Reply = reply };
        return msg.Read(data.GetStream(), null) ? msg : null;
    }

    /// <summary>写入消息到数据流</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Write(Stream stream, Object context)
    {
        var p = stream.Position;
        if (!base.Write(stream, context)) return false;

        stream.Position = p;
        Crc2 = ModbusHelper.Crc(stream);

        stream.Write(Crc2.GetBytes(true));

        return true;
    }

    /// <summary>创建响应</summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override ModbusMessage CreateReply()
    {
        if (Reply) throw new InvalidOperationException();

        var msg = new ModbusRtuMessage
        {
            Reply = true,
            Host = Host,
            Code = Code,
        };

        return msg;
    }
    #endregion
}