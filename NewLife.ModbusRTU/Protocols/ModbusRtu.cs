using System.IO.Ports;
using NewLife.IoT.Protocols;
using NewLife.Log;

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

    /// <summary>缓冲区大小。默认256</summary>
    public Int32 BufferSize { get; set; } = 256;

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
                ReadTimeout = 3_000,
                WriteTimeout = 3_000
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
            if (Log != null && Log.Level <= LogLevel.Debug) WriteLog("=> {0}", message);
            var cmd = message.ToPacket();
            var buf = cmd.ToArray();

            var crc = Crc(buf, 0, buf.Length);
            cmd.Append(crc.GetBytes(true));
            buf = cmd.ToArray();

            using var span = Tracer?.NewSpan("modbus:SendCommand", buf.ToHex("-"));

            if (Log != null && Log.Level <= LogLevel.Debug) WriteLog("{0}=> {1}", PortName, buf.ToHex("-"));
            _port.Write(buf, 0, buf.Length);
            Thread.Sleep(10);
        }

        // 串口速度较慢，等待收完数据
        WaitMore(_port);

        try
        {
            using var span = Tracer?.NewSpan("modbus:ReceiveCommand");

            var buf = new Byte[BufferSize];
            var c = _port.Read(buf, 0, buf.Length);
            buf = buf.ReadBytes(0, c);
            if (Log != null && Log.Level <= LogLevel.Debug) WriteLog("{0}<= {1}", PortName, buf.ToHex("-"));

            if (span != null) span.Tag = buf.ToHex();

            if (buf.Length < 2 + 2) return null;

            // 校验Crc
            var crc = Crc(buf, 0, buf.Length - 2);
            var crc2 = buf.ToUInt16(buf.Length - 2);
            if (crc != crc2) WriteLog("Crc Error {0:X4}!={1:X4} !", crc, crc2);

            var rs = ModbusMessage.Read(buf, true);
            if (rs == null) return null;

            if (Log != null && Log.Level <= LogLevel.Debug) WriteLog("<= {0}", rs);

            return rs;
        }
        catch (TimeoutException) { return null; }
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

    #region CRC
    private static readonly UInt16[] crc_ta = new UInt16[16] { 0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400, };

    /// <summary>Crc校验</summary>
    /// <param name="data"></param>
    /// <param name="offset">偏移</param>
    /// <param name="count">数量</param>
    /// <returns></returns>
    public static UInt16 Crc(Byte[] data, Int32 offset, Int32 count = -1)
    {
        if (data == null || data.Length < 1) return 0;

        UInt16 u = 0xFFFF;
        Byte b;

        if (count == 0) count = data.Length - offset;

        for (var i = offset; i < count; i++)
        {
            b = data[i];
            u = (UInt16)(crc_ta[(b ^ u) & 15] ^ (u >> 4));
            u = (UInt16)(crc_ta[((b >> 4) ^ u) & 15] ^ (u >> 4));
        }

        return u;
    }
    #endregion
}