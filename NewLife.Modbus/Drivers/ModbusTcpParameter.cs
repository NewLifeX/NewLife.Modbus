using System.ComponentModel;

namespace NewLife.IoT.Drivers;

/// <summary>ModbusTcp参数</summary>
public class ModbusTcpParameter : ModbusIpParameter
{
    /// <summary>协议标识。默认0</summary>
    [Description("协议标识。默认0")]
    public UInt16 ProtocolId { get; set; }
}