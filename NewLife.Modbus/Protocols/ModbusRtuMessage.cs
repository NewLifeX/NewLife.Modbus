using System.IO;
using NewLife.Buffers;
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
    /// <param name="reader">读取器</param>
    /// <returns></returns>
    public override Boolean Read(SpanReader reader)
    {
        var p = reader.Position;
        if (!base.Read(reader)) return false;

        // 从负载数据里把Crc取出来
        var pk = Payload;
        var count = pk?.Total ?? 0;
        if (count >= 2)
        {
            Crc = pk.ReadBytes(count - 2, 2).ToUInt16(0, true);
            Payload = pk.Slice(0, count - 2);
        }

        //reader.Position = p;
        //Crc2 = ModbusHelper.Crc(stream, (Int32)(stream.Length - stream.Position - 2));

        return true;
    }

    /// <summary>解析消息</summary>
    public static ModbusRtuMessage Read(ReadOnlySpan<Byte> data, Boolean reply = false)
    {
        var msg = new ModbusRtuMessage { Reply = reply };
        var reader = new SpanReader(data) { IsLittleEndian = false };
        return msg.Read(reader) ? msg : null;
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