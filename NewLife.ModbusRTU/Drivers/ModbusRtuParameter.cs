using System.ComponentModel;

namespace NewLife.IoT.Drivers;

/// <summary>ModbusRtu参数</summary>
public class ModbusRtuParameter : ModbusParameter
{
    /// <summary>串口</summary>
    [Description("串口")]
    public String PortName { get; set; }

    /// <summary>波特率</summary>
    [Description("主机波特率站号")]
    public Int32 Baudrate { get; set; }
}