using NewLife.Data;
using NewLife.Net;

namespace NewLife.IoT.Protocols;

/// <summary>Modbus以太网通信</summary>
/// <remarks>
/// ADU规定为256
/// </remarks>
public abstract class ModbusIp : Modbus
{
    #region 属性
    /// <summary>服务端地址。tcp://127.0.0.1:502</summary>
    public String Server { get; set; }

    /// <summary>网络客户端</summary>
    protected ISocketClient _client;
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
    }

    /// <summary>打开</summary>
    public override void Open()
    {
        if (_client == null || _client.Disposed)
        {
            if (Server.IsNullOrEmpty()) throw new Exception($"{Name}未指定服务端地址Server");

            var uri = new NetUri(Server);
            if (uri.Type <= 0) uri.Type = NetType.Tcp;
            if (uri.Port == 0) uri.Port = 502;

            var client = uri.CreateRemote();
            client.Timeout = Timeout;

            // 使用同步接收，每个数据帧最大256字节
            if (client is SessionBase session)
            {
                session.MaxAsync = 0;
                session.BufferSize = 256;
            }

            //if (client is TcpSession tcp)
            //    tcp.MaxAsync = 0;
            //else if (client is UdpServer udp)
            //    udp.MaxAsync = 0;

            client.Open();

            _client = client;

            WriteLog("{0}.Open {1}", Name, uri);
        }
    }

    /// <summary>从数据包中解析Modbus消息</summary>
    /// <param name="request">请求消息</param>
    /// <param name="data">目标数据包</param>
    /// <param name="match">是否匹配请求</param>
    /// <returns>响应消息</returns>
    protected abstract ModbusMessage ReadMessage(ModbusMessage request, Packet data, out Boolean match);

    /// <summary>接收响应</summary>
    /// <returns></returns>
    protected virtual Packet ReceiveCommand()
    {
        // 设置协议最短长度，避免读取指令不完整。由于请求响应机制，不存在粘包返回。
        var dataLength = 8; // 2+2+2+1+1
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
            if (count >= 6) dataLength = pk.ReadBytes(4, 2).ToUInt16(0, false);
        }

        return pk;
    }

    /// <summary>发送消息并接收返回</summary>
    /// <param name="message">Modbus消息</param>
    /// <returns></returns>
    protected override ModbusMessage SendCommand(ModbusMessage message)
    {
        Open();

        Log?.Debug("=> {0}", message);

        {
            var cmd = message.ToPacket();
            using var span = Tracer?.NewSpan("modbus:SendCommand", cmd.ToHex(64, "-"));
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
            try
            {
                while (true)
                {
                    // 设置协议最短长度，避免读取指令不完整。由于请求响应机制，不存在粘包返回。
                    var pk = ReceiveCommand();

                    if (span != null) span.Tag = pk.ToHex();

                    var rs = ReadMessage(message, pk, out var match);
                    if (rs == null) return null;

                    Log?.Debug("<= {0}", rs);

                    // 检查是否匹配
                    if (!match) continue;

                    // 检查功能码
                    if (rs.ErrorCode > 0) throw new ModbusException(rs.ErrorCode, rs.ErrorCode.GetDescription());

                    return rs;
                }
            }
            catch (Exception ex)
            {
                span?.SetError(ex, null);
                if (ex is TimeoutException) return null;
                throw;
            }
        }
    }
    #endregion
}