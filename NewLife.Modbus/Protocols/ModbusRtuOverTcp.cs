using NewLife.Data;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusRtu基于TCP网口通信</summary>
/// <remarks>
/// ADU规定为256
/// </remarks>
public class ModbusRtuOverTcp : ModbusIp
{
    #region 属性
    #endregion

    #region 构造
    #endregion

    #region 方法
    /// <summary>创建消息</summary>
    /// <returns></returns>
    protected override ModbusMessage CreateMessage() => new ModbusRtuMessage();

    /// <summary>接收响应</summary>
    /// <returns></returns>
    protected override Packet ReceiveCommand()
    {
        // 设置协议最短长度，避免读取指令不完整。由于请求响应机制，不存在粘包返回。
        var dataLength = 4; // 1+1+2
        var count = 0;
        Packet pk = null;
        while (count < dataLength)
        {
            var pk2 = _client.Receive();
            if (pk == null)
                pk = pk2;
            else
                pk.Append(pk2);

            count = pk.Total;
        }

        return pk;
    }

    /// <summary>从数据包中解析Modbus消息</summary>
    /// <param name="request">请求消息</param>
    /// <param name="data">目标数据包</param>
    /// <param name="match">是否匹配请求</param>
    /// <returns>响应消息</returns>
    protected override ModbusMessage ReadMessage(ModbusMessage request, Packet data, out Boolean match)
    {
        match = true;

        var rs = ModbusRtuMessage.Read(data, true);
        if (rs == null) return null;

        Log?.Debug("<= {0}", rs);

        return rs;
    }
    #endregion
}