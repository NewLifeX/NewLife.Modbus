using System.IO.Ports;
using NewLife.Data;
using NewLife.IoT.Protocols;
using NewLife.IoT;

#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif

namespace NewLife.Serial.Protocols;

/// <summary>ModbusRTU串口通信</summary>
public class ModbusRtu : Modbus
{
    #region 属性
    /// <summary>端口</summary>
    public String PortName { get; set; }

    /// <summary>波特率</summary>
    public Int32 Baudrate { get; set; } = 9600;

    /// <summary>字节超时。数据包间隔，默认20ms</summary>
    public Int32 ByteTimeout { get; set; } = 20;

    private SerialPort _port;
    #endregion

    #region 构造
    /// <summary>
    /// 销毁
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _port.TryDispose();
        _port = null;
    }
    #endregion

    #region 方法
    /// <summary>初始化。传入配置字符串</summary>
    /// <param name="parameters"></param>
    public override void Init(IDictionary<String, Object> parameters)
    {
        if (parameters.TryGetValue("PortName", out var str))
            PortName = str + "";
        if (PortName.IsNullOrEmpty() && parameters.TryGetValue("Address", out str))
            PortName = str + "";

        if (parameters.TryGetValue("Baudrate", out str)) Baudrate = str.ToInt();
    }

    /// <summary>打开</summary>
    public override void Open()
    {
        if (_port == null)
        {
            var p = new SerialPort(PortName, Baudrate)
            {
                ReadTimeout = Timeout,
                WriteTimeout = Timeout
            };
            p.Open();
            _port = p;

            WriteLog("ModbusRtu.Open {0} Baudrate={1}", PortName, Baudrate);
        }
    }

    /// <summary>发送消息并接收返回</summary>
    /// <param name="message">Modbus消息</param>
    /// <returns></returns>
    protected override ModbusMessage SendCommand(ModbusMessage message)
    {
        Open();

        {
            Log?.Debug("=> {0}", message);

            var cmd = message.ToPacket();
            var buf = cmd.ToArray();

            var crc = ModbusHelper.Crc(buf, 0, buf.Length);
            cmd.Append(crc.GetBytes(true));
            buf = cmd.ToArray();

            using var span = Tracer?.NewSpan("modbus:SendCommand", buf.ToHex("-"));

            Log?.Debug("{0}=> {1}", PortName, buf.ToHex("-"));

            _port.Write(buf, 0, buf.Length);

            Thread.Sleep(10);
        }

        // 串口速度较慢，等待收完数据
        WaitMore(_port);

        {
            using var span = Tracer?.NewSpan("modbus:ReceiveCommand");
#if NETSTANDARD2_1_OR_GREATER
            var buf = ArrayPool<Byte>.Shared.Rent(BufferSize);
#else
            var buf = new Byte[BufferSize];
#endif
            try
            {
                var count = _port.Read(buf, 0, buf.Length);
                var pk = new Packet(buf, 0, count);
                Log?.Debug("{0}<= {1}", PortName, pk.ToHex(32, "-"));

                if (span != null) span.Tag = pk.ToHex();

                var len = pk.Total - 2;
                if (len < 2) return null;

                // 校验Crc
                var crc = ModbusHelper.Crc(buf, 0, len);
                var crc2 = buf.ToUInt16(len);
                if (crc != crc2) WriteLog("Crc Error {0:X4}!={1:X4} !", crc, crc2);

                var rs = ModbusRtuMessage.Read(pk, true);
                if (rs == null) return null;

                Log?.Debug("<= {0}", rs);

                // 检查功能码
                if (rs.ErrorCode > 0) throw new ModbusException(rs.ErrorCode, rs.ErrorCode + "");

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

    void WaitMore(SerialPort sp)
    {
        var ms = ByteTimeout;
        var end = DateTime.Now.AddMilliseconds(ms);
        var count = sp.BytesToRead;
        while (sp.IsOpen && end > DateTime.Now)
        {
            //Thread.SpinWait(1);
            Thread.Sleep(ms);
            if (count != sp.BytesToRead)
            {
                end = DateTime.Now.AddMilliseconds(ms);
                count = sp.BytesToRead;
            }
        }
    }

    /// <summary>获取串口列表</summary>
    /// <returns></returns>
    public static String[] GetPortNames() => SerialPort.GetPortNames();
    #endregion
}