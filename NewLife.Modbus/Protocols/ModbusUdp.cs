using System.Net.Sockets;
using NewLife.Data;
using NewLife.Net;

#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif

namespace NewLife.IoT.Protocols;

/// <summary>ModbusUDP网口通信</summary>
/// <remarks>
/// ADU规定为256
/// </remarks>
public class ModbusUdp : Modbus
{
    #region 属性
    /// <summary>服务端地址。127.0.0.1:502</summary>
    public String Server { get; set; }

    /// <summary>协议标识。默认0</summary>
    public UInt16 ProtocolId { get; set; }

    private Int32 _transactionId;
    private ISocketClient _client;
    #endregion

    #region 构造
    /// <summary>
    /// 销毁
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _client.TryDispose();
        _client = null;
    }
    #endregion

    #region 方法
    /// <summary>初始化。传入配置</summary>
    /// <param name="parameters"></param>
    public override void Init(IDictionary<String, Object> parameters)
    {
        if (parameters.TryGetValue("Server", out var str))
            Server = str + "";
        if (Server.IsNullOrEmpty() && parameters.TryGetValue("Address", out str))
            Server = str + "";

        if (parameters.TryGetValue("ProtocolId", out str)) ProtocolId = (UInt16)str.ToInt();
    }

    /// <summary>打开</summary>
    public override void Open()
    {
        if (_client == null || _client.Disposed)
        {
            if (Server.IsNullOrEmpty()) throw new Exception("ModbusUdp未指定服务端地址Server");

            var uri = new NetUri(Server);
            if (uri.Type <= 0) uri.Type = NetType.Udp;
            if (uri.Port == 0) uri.Port = 502;

            var client = uri.CreateRemote();
            client.Timeout = Timeout;

            if (client is TcpSession tcp)
                tcp.MaxAsync = 0;
            else if (client is UdpServer udp)
                udp.MaxAsync = 0;

            _client = client;

            WriteLog("ModbusUdp.Open {0}", uri);
        }
    }

    /// <summary>创建消息</summary>
    /// <returns></returns>
    protected override ModbusMessage CreateMessage() => new ModbusTcpMessage
    {
        ProtocolId = ProtocolId,
        TransactionId = (UInt16)Interlocked.Increment(ref _transactionId)
    };

    /// <summary>发送消息并接收返回</summary>
    /// <param name="message">Modbus消息</param>
    /// <returns></returns>
    protected override ModbusMessage SendCommand(ModbusMessage message)
    {
        Open();

        Log?.Debug("=> {0}", message);

        {
            var cmd = message.ToPacket().ToArray();
            using var span = Tracer?.NewSpan("modbus:SendCommand", cmd.ToHex("-"));
            try
            {
                _client.Send(cmd);
            }
            catch (Exception ex)
            {
                span?.SetError(ex, null);
                throw;
            }
        }

        {
            using var span = Tracer?.NewSpan("modbus:ReceiveCommand");
#if NETSTANDARD2_1_OR_GREATER
            var buf = ArrayPool<Byte>.Shared.Rent(BufferSize);
#else
            var buf = new Byte[BufferSize];
#endif
            try
            {
                while (true)
                {
                    // 设置协议最短长度，避免读取指令不完整。由于请求响应机制，不存在粘包返回。
                    var dataLength = 8;
                    var count = 0;
                    Packet pk = null;
                    while (count < dataLength)
                    {
                        var pk2 = _client.Receive();
                        if (pk == null)
                            pk = pk2;
                        else
                            pk.Append(pk2);

                        // 已取得请求头，计算真实长度
                        count = pk.Total;
                        if (count >= 6) dataLength = buf.ToUInt16(4, false);
                    }

                    if (span != null) span.Tag = pk.ToHex();

                    var rs = ModbusTcpMessage.Read(pk, true);
                    if (rs == null) return null;

                    Log?.Debug("<= {0}", rs);

                    // 检查事务标识
                    if (message is ModbusTcpMessage mtm && mtm.TransactionId != rs.TransactionId)
                    {
                        WriteLog("TransactionId Error {0}!={1}", rs.TransactionId, mtm.TransactionId);

                        // 读取到前一条，抛弃它，继续读取
                        if (rs.TransactionId < mtm.TransactionId) continue;

                        return null;
                    }

                    // 检查功能码
                    if (rs.ErrorCode > 0) throw new ModbusException(rs.ErrorCode, rs.ErrorCode + "");

                    return rs;
                }
            }
            catch (Exception ex)
            {
                span?.SetError(ex, null);
                if (ex is TimeoutException) return null;
                throw;
            }
            finally
            {
#if NETSTANDARD2_1_OR_GREATER
                ArrayPool<Byte>.Shared.Return(buf);
#endif
            }
        }
    }
    #endregion
}