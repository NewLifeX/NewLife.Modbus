using System.ComponentModel;

namespace NewLife.IoT.Drivers;

/// <summary>ModbusRtu参数</summary>
public class ModbusRtuParameter : ModbusParameter
{
    /// <summary>串口</summary>
    [Description("串口")]
    public String PortName { get; set; }

    /// <summary>波特率</summary>
    [Description("主机波特率")]
    public Int32 Baudrate { get; set; }

    ///// <summary>字节超时。数据包间隔，默认20ms</summary>
    //[Description("字节超时。数据包间隔，默认20ms")]
    //public Int32 ByteTimeout { get; set; } = 20;
}