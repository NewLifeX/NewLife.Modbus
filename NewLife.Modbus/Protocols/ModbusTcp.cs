using System.Net.Sockets;
using NewLife.Data;
using NewLife.Log;
using NewLife.Net;

#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif

namespace NewLife.IoT.Protocols;

/// <summary>ModbusTCP网口通信</summary>
/// <remarks>
/// ADU规定为256
/// </remarks>
public class ModbusTcp : Modbus
{
    #region 属性
    /// <summary>服务端地址。127.0.0.1:502</summary>
    public String Server { get; set; }

    /// <summary>协议标识。默认0</summary>
    public UInt16 ProtocolId { get; set; }

    private Int32 _transactionId;
    private TcpClient _client;
    private NetworkStream _stream;
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
        _stream = null;
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
        if (_client == null || !_client.Connected)
        {
            if (Server.IsNullOrEmpty()) throw new Exception("ModbusTcp未指定服务端地址Server");

            var uri = new NetUri(Server);
            if (uri.Type <= 0) uri.Type = NetType.Tcp;
            if (uri.Port == 0) uri.Port = 502;

            var client = new TcpClient
            {
                SendTimeout = Timeout,
                ReceiveTimeout = Timeout
            };
            client.Connect(uri.Address, uri.Port);

            _client = client;
            _stream = client.GetStream();

            WriteLog("ModbusTcp.Open {0}", uri);
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

        if (Log != null && Log.Level <= LogLevel.Debug) WriteLog("=> {0}", message);

        // 剔除剩余未读取数据
        if (_stream.DataAvailable) _stream.ReadBytes();

        {
            var cmd = message.ToPacket().ToArray();
            using var span = Tracer?.NewSpan("modbus:SendCommand", cmd.ToHex("-"));
            try
            {
                _stream.Write(cmd, 0, cmd.Length);
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
                // 设置协议最短长度，避免读取指令不完整。由于请求响应机制，不存在粘包返回。
                var dataLength = 8;
                var count = 0;
                while (count < dataLength)
                {
                    count += _stream.Read(buf, count, buf.Length - count);

                    // 已取得请求头，计算真实长度
                    if (count >= 6) dataLength = buf.ToUInt16(4, false);
                }
                var pk = new Packet(buf, 0, count);

                if (span != null) span.Tag = pk.ToHex();

                var rs = ModbusTcpMessage.Read(pk, true);
                if (rs == null) return null;

                // 检查事务标识
                if (message is ModbusTcpMessage mtm && mtm.TransactionId != rs.TransactionId) return null;

                if (Log != null && Log.Level <= LogLevel.Debug) WriteLog("<= {0}", rs);

                return rs;
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