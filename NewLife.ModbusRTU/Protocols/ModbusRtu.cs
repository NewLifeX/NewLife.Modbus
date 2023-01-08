using System.IO.Ports;
using NewLife.Data;
using NewLife.IoT.Protocols;
using NewLife.IoT;
using System.Diagnostics;
using NewLife.Log;

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

    /// <summary>数据位长度。默认8</summary>
    public Int32 DataBits { get; set; } = 8;

    /// <summary>奇偶校验位。默认None无校验</summary>
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>停止位。默认One</summary>
    public StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>字节超时。数据包间隔，默认10ms</summary>
    public Int32 ByteTimeout { get; set; } = 10;

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
                DataBits = DataBits,
                Parity = Parity,
                StopBits = StopBits,

                ReadTimeout = Timeout,
                WriteTimeout = Timeout
            };
            p.Open();
            _port = p;

            WriteLog("ModbusRtu.Open {0} Baudrate={1} DataBits={2} Parity={3} StopBits={4}", PortName, Baudrate, p.DataBits, p.Parity, p.StopBits);
        }
    }

    /// <summary>发送消息并接收返回</summary>
    /// <param name="message">Modbus消息</param>
    /// <returns></returns>
    protected override ModbusMessage SendCommand(ModbusMessage message)
    {
        Open();

        // 清空缓冲区
        _port.DiscardInBuffer();

        Log?.Debug("=> {0}", message);

        var cmd = message.ToPacket();
        var buf = cmd.ToArray();

        var crc = ModbusHelper.Crc(buf, 0, buf.Length);
        cmd.Append(crc.GetBytes(true));
        buf = cmd.ToArray();

        using var span = Tracer?.NewSpan("modbus:SendCommand", buf.ToHex("-"));

        Log?.Debug("{0}=> {1}", PortName, buf.ToHex("-"));

        _port.Write(buf, 0, buf.Length);

        if (ByteTimeout > 10) Thread.Sleep(ByteTimeout);

        // 串口速度较慢，等待收完数据
        WaitMore(_port, 1 + 1 + 2);

        //using var span = Tracer?.NewSpan("modbus:ReceiveCommand");
        buf = new Byte[BufferSize];
        try
        {
            var count = _port.Read(buf, 0, buf.Length);
            var pk = new Packet(buf, 0, count);
            Log?.Debug("{0}<= {1}", PortName, pk.ToHex(32, "-"));

            if (span != null) span.Tag += Environment.NewLine + pk.ToHex(64, "-");

            var len = pk.Total - 2;
            if (len < 2) return null;

            // 校验Crc
            crc = ModbusHelper.Crc(buf, 0, len);
            var crc2 = buf.ToUInt16(len);
            if (crc != crc2) WriteLog("Crc Error {0:X4}!={1:X4} !", crc, crc2);

            var rs = ModbusRtuMessage.Read(pk, true);
            if (rs == null) return null;

            Log?.Debug("<= {0}", rs);

            // 检查功能码
            return rs.ErrorCode > 0 ? throw new ModbusException(rs.ErrorCode, rs.ErrorCode + "") : (ModbusMessage)rs;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            if (ex is TimeoutException) return null;
            throw;
        }
    }

    private void WaitMore(SerialPort sp, Int32 minLength)
    {
        var count = sp.BytesToRead;
        if (count >= minLength) return;

        var ms = ByteTimeout > 0 ? ByteTimeout : 10;
        var sw = Stopwatch.StartNew();
        while (sp.IsOpen && sw.ElapsedMilliseconds < Timeout)
        {
            //Thread.SpinWait(1);
            Thread.Sleep(ms);
            if (count != sp.BytesToRead)
            {
                count = sp.BytesToRead;
                if (count >= minLength) break;

                //sw.Restart();
            }
        }
    }

    /// <summary>获取串口列表</summary>
    /// <returns></returns>
    public static String[] GetPortNames() => SerialPort.GetPortNames();
    #endregion
}