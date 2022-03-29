using NewLife.IoT.Protocols;

namespace NewLife.IoT.Drivers;

/// <summary>Modbus参数</summary>
public class ModbusParameter
{
    /// <summary>主机地址</summary>
    public Byte Host { get; set; }

    /// <summary>读取功能码</summary>
    public FunctionCodes ReadCode { get; set; }

    /// <summary>写入功能码</summary>
    public FunctionCodes WriteCode { get; set; }

    /// <summary>地址</summary>
    public String Address { get; set; }

    /// <summary>串口</summary>
    public String PortName { get; set; }

    /// <summary>波特率</summary>
    public Int32 Baudrate { get; set; }
}