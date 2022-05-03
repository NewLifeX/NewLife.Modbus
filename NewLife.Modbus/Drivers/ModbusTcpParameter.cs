namespace NewLife.IoT.Drivers;

/// <summary>ModbusTcp参数</summary>
public class ModbusTcpParameter : ModbusParameter
{
    /// <summary>地址。tcp地址</summary>
    public String Server { get; set; }

    /// <summary>协议标识。默认0</summary>
    public UInt16 ProtocolId { get; set; }
}