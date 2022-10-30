using NewLife.Data;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusTCP网口通信</summary>
/// <remarks>
/// ADU规定为256
/// </remarks>
public class ModbusTcp : ModbusIp
{
    #region 属性
    /// <summary>协议标识。默认0</summary>
    public UInt16 ProtocolId { get; set; }

    private Int32 _transactionId;
    #endregion

    #region 构造
    #endregion

    #region 方法
    /// <summary>初始化。传入配置</summary>
    /// <param name="parameters"></param>
    public override void Init(IDictionary<String, Object> parameters)
    {
        base.Init(parameters);

        if (parameters.TryGetValue("ProtocolId", out var str)) ProtocolId = (UInt16)str.ToInt();
    }

    /// <summary>创建消息</summary>
    /// <returns></returns>
    protected override ModbusMessage CreateMessage() => new ModbusIpMessage
    {
        ProtocolId = ProtocolId,
        TransactionId = (UInt16)Interlocked.Increment(ref _transactionId)
    };

    /// <summary>从数据包中解析Modbus消息</summary>
    /// <param name="request">请求消息</param>
    /// <param name="data">目标数据包</param>
    /// <param name="match">是否匹配请求</param>
    /// <returns>响应消息</returns>
    protected override ModbusMessage ReadMessage(ModbusMessage request, Packet data, out Boolean match)
    {
        match = true;

        var rs = ModbusIpMessage.Read(data, true);
        if (rs == null) return null;

        Log?.Debug("<= {0}", rs);

        // 检查事务标识
        if (request is ModbusIpMessage mtm && mtm.TransactionId != rs.TransactionId)
        {
            WriteLog("TransactionId Error {0}!={1}", rs.TransactionId, mtm.TransactionId);

            // 读取到前一条，抛弃它，继续读取
            if (rs.TransactionId < mtm.TransactionId)
            {
                match = false;
                return rs;
            }

            return null;
        }

        return rs;
    }
    #endregion
}