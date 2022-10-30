using NewLife.Data;

namespace NewLife.IoT.Protocols;

/// <summary>
/// ModbusRtu消息
/// </summary>
public class ModbusRtuMessage : ModbusMessage
{
    #region 属性
    #endregion

    #region 方法
    /// <summary>读取</summary>
    /// <param name="stream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Boolean Read(Stream stream, Object context) => base.Read(stream, context);

    /// <summary>解析消息</summary>
    /// <param name="pk"></param>
    /// <param name="reply"></param>
    /// <returns></returns>
    public static new ModbusIpMessage Read(Packet pk, Boolean reply = false)
    {
        var msg = new ModbusIpMessage { Reply = reply };
        return msg.Read(pk.GetStream(), null) ? msg : null;
    }

    /// <summary>写入消息到数据流</summary>
    /// <param name="stream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Boolean Write(Stream stream, Object context) => base.Write(stream, context);

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