using System.ComponentModel;
using System.IO.Ports;
using NewLife.IoT.Drivers;

namespace NewLife.Serial.Drivers;

/// <summary>ModbusRtu参数</summary>
public class ModbusRtuParameter : ModbusParameter, IDriverParameterKey
{
    /// <summary>串口</summary>
    [Description("串口")]
    public String PortName { get; set; }

    /// <summary>波特率</summary>
    [Description("波特率")]
    public Int32 Baudrate { get; set; }

    /// <summary>数据位。默认8</summary>
    [Description("数据位")]
    public Int32 DataBits { get; set; } = 8;

    /// <summary>奇偶校验位。默认None无校验</summary>
    [Description("奇偶校验位")]
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>停止位。默认One</summary>
    [Description("停止位")]
    public StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>字节超时。数据包间隔，默认10ms</summary>
    [Description("字节超时。数据包间隔，默认10ms")]
    public Int32 ByteTimeout { get; set; } = 10;

    /// <summary>获取唯一标识</summary>
    /// <returns></returns>
    public String GetKey() => PortName;
}