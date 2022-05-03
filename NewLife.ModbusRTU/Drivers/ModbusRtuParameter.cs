namespace NewLife.IoT.Drivers;

/// <summary>ModbusRtu参数</summary>
public class ModbusRtuParameter : ModbusParameter
{
    /// <summary>串口</summary>
    public String PortName { get; set; }

    /// <summary>波特率</summary>
    public Int32 Baudrate { get; set; }
}