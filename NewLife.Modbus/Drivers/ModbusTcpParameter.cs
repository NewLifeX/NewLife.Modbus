using System.ComponentModel;

namespace NewLife.IoT.Drivers;

/// <summary>ModbusTcp参数</summary>
public class ModbusTcpParameter : ModbusParameter
{
    /// <summary>地址。tcp地址如127.0.0.1:502</summary>
    [Description("地址。tcp地址如127.0.0.1:502")]
    public String Server { get; set; }

    /// <summary>协议标识。默认0</summary>
    [Description("协议标识。默认0")]
    public UInt16 ProtocolId { get; set; }
}