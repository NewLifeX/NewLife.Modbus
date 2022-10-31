using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusAscii消息</summary>
public class ModbusAsciiMessage : ModbusMessage
{
    #region 属性
    /// <summary>LRC校验</summary>
    public Byte Lrc { get; set; }

    /// <summary>LRC校验</summary>
    public Byte Lrc2 { get; set; }
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Read(Stream stream, Object context)
    {
        var str = stream.ToStr();
        if (str.Length < 1 + 2 + 2 + 2 || str[0] != ':') return false;

        // 找到结束符
        var p = str.IndexOf("\r\n");
        if (p < 0) return false;

        var buf = str[1..p].ToHex();
        var ms = new MemoryStream(buf, 0, buf.Length - 1);

        var binary = context as Binary ?? new Binary { Stream = ms, IsLittleEndian = false };

        if (!base.Read(ms, context ?? binary)) return false;

        Lrc = buf[^1];
        Lrc2 = ModbusHelper.Lrc(buf, 0, buf.Length - 1);

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

    /// <summary>写入消息到数据流</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Write(Stream stream, Object context)
    {
        var ms = new MemoryStream();
        if (!base.Write(ms, context)) return false;

        var buf = ms.ToArray();

        Lrc2 = ModbusHelper.Lrc(buf, 0, buf.Length);

        stream.Write((Byte)':');
        for (var i = 0; i < buf.Length; i++)
        {
            stream.Write(buf[i].ToString("X2").GetBytes());
        }
        stream.Write(Lrc2.ToString("X2").GetBytes());
        stream.Write((Byte)'\r');
        stream.Write((Byte)'\n');

        return true;
    }

    /// <summary>创建响应</summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override ModbusMessage CreateReply()
    {
        if (Reply) throw new InvalidOperationException();

        var msg = new ModbusAsciiMessage
        {
            Reply = true,
            Host = Host,
            Code = Code,
        };

        return msg;
    }
    #endregion

    #region 辅助
    public static Byte[] Decode(String value) => null;
    #endregion
}