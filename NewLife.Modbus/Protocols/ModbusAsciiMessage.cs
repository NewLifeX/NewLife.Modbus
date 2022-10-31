using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusAscii消息</summary>
public class ModbusAsciiMessage : ModbusMessage
{
    #region 属性
    /// <summary>LRC校验</summary>
    public UInt16 Lrc { get; set; }

    /// <summary>LRC校验</summary>
    public UInt16 Lrc2 { get; set; }
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public override Boolean Read(Stream stream, Object context)
    {
        var buf = stream.ReadBytes();
        if (buf.Length < 1 + 2 + 2 + 2 || buf[0] != ':') return false;

        // 找到结束符
        var p = -1;
        for (var i = 0; i < buf.Length - 1; i++)
        {
            if (buf[i] == '\r' && buf[i + 1] == '\n')
            {
                p = i;
                break;
            }
        }
        if (p < 0) return false;

        var ms = new MemoryStream();
        for (var i = 1; i < p - 2; i += 2)
        {
            var a = (Char)buf[i];
            var b = (Char)buf[i + 1];

            var a2 = (Int32)a;
            if (a is >= 'A' and <= 'F')
                a2 = a - 'A' + 10;
            else if (a is >= '0' and <= '9')
                a2 -= '0';

            var b2 = (Int32)b;
            if (b is >= 'A' and <= 'F')
                b2 = b - 'A' + 10;
            else if (b is >= '0' and <= '9')
                b2 -= '0';

            ms.WriteByte((Byte)((a2 << 4) | b2));
        }

        Lrc = buf.ToUInt16(p - 2, true);

        ms.Position = 0;
        var binary = context as Binary ?? new Binary { Stream = ms, IsLittleEndian = false };

        if (!base.Read(ms, context ?? binary)) return false;

        ms.Position = 0;
        var bt = ModbusHelper.Lrc(ms);
        //var bt = ModbusHelper.Lrc(buf, 1, buf.Length - 1 - 2 - 2);
        Lrc2 = ModbusHelper.GetAsciiBytes(bt).ToUInt16(0, true);

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
    #endregion
}