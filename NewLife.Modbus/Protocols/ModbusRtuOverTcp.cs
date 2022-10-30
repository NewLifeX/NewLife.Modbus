using NewLife.Data;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusTCP网口通信</summary>
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