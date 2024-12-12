using System.Text;
using NewLife.Buffers;
using NewLife.Data;

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
    /// <summary>从数据读取消息</summary>
    /// <param name="reader">读取器</param>
    /// <returns></returns>
    public override Boolean Read(SpanReader reader)
    {
        var str = reader.ReadString(-1, Encoding.ASCII);
        if (str.Length < 1 + 2 + 2 + 2 || str[0] != ':') return false;

        // 找到结束符
        var p = str.IndexOf("\r\n");
        if (p < 0) return false;

        var buf = str[1..p].ToHex();
        var reader2 = new SpanReader(buf) { IsLittleEndian = false };

        if (!base.Read(reader2)) return false;

        Lrc = buf[^1];
        Lrc2 = ModbusHelper.Lrc(buf, 0, buf.Length - 1);

        return true;
    }

    /// <summary>从数据读取消息</summary>
    /// <param name="data">数据</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public static ModbusAsciiMessage Read(ReadOnlySpan<Byte> data, Boolean reply = false)
    {
        var msg = new ModbusAsciiMessage { Reply = reply };
        var reader = new SpanReader(data) { IsLittleEndian = false };
        return msg.Read(reader) ? msg : null;
    }

    /// <summary>写入消息到数据</summary>
    /// <param name="writer">写入器</param>
    /// <returns></returns>
    public override Boolean Write(SpanWriter writer)
    {
        using var pk = new OwnerPacket(256);
        var writer2 = new SpanWriter(pk.GetSpan()) { IsLittleEndian = false };
        if (!base.Write(writer2)) return false;

        var buf = pk.GetSpan()[..writer2.Position].ToArray();

        Lrc2 = ModbusHelper.Lrc(buf, 0, buf.Length);

        writer.Write((Byte)':');
        for (var i = 0; i < buf.Length; i++)
        {
            writer.Write(buf[i].ToString("X2").GetBytes());
        }
        writer.Write(Lrc2.ToString("X2").GetBytes());
        writer.Write((Byte)'\r');
        writer.Write((Byte)'\n');

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
    //public static Byte[] Decode(String value) => null;
    #endregion
}