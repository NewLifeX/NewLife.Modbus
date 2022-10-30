using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusAscii消息</summary>
public class ModbusAsciiMessage : ModbusMessage
{
    #region 属性
    /// <summary>LRC校验</summary>
    public UInt16 Lrc { get; set; }
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Read(Stream stream, Object context)
    {
        var flag = stream.ReadByte();
        if (flag != ':') return false;

        var ms = new MemoryStream();
        while (true)
        {
            var a = stream.ReadByte();
            var b = stream.ReadByte();
            if (a < 0 || b < 0) break;
            if (a == '\r' && b == '\n') break;

            if (a >= 'A')
                a -= 'A';
            else
                a -= '0';

            if (b >= 'A')
                b -= 'A';
            else
                b -= '0';

            ms.WriteByte((Byte)((a << 8) | b));
        }

        ms.Position = 0;
        var binary = context as Binary ?? new Binary { Stream = ms, IsLittleEndian = false };

        if (!base.Read(ms, context ?? binary)) return false;

        Lrc = binary.ReadUInt16();

        return true;
    }

    /// <summary>解析消息</summary>
    /// <param name="data">数据包</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public static new ModbusAsciiMessage Read(Packet data, Boolean reply = false)
    {
        var msg = new ModbusAsciiMessage { Reply = reply };
        return msg.Read(data.GetStream(), null) ? msg : null;
    }

    ///// <summary>写入消息到数据流</summary>
    ///// <param name="stream">数据流</param>
    ///// <param name="context">上下文</param>
    ///// <returns></returns>
    //public override Boolean Write(Stream stream, Object context) => base.Write(stream, context);

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