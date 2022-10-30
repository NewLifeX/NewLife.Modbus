using System.ComponentModel;

namespace NewLife.IoT.Drivers;

/// <summary>ModbusUdp参数</summary>
public class ModbusUdpParameter : ModbusIpParameter
{
    /// <summary>协议标识。默认0</summary>
    [Description("协议标识。默认0")]
    public UInt16 ProtocolId { get; set; }
}