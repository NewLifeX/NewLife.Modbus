using NewLife.IoT.Protocols;

namespace NewLife.IoT.Drivers;

/// <summary>
/// Modbus节点
/// </summary>
public class ModbusNode : INode
{
    ///// <summary>Modbus对象</summary>
    //public Modbus Modbus { get; set; }

    /// <summary>主机地址</summary>
    public Byte Host { get; set; }

    /// <summary>读取功能码</summary>
    public FunctionCodes ReadCode { get; set; }

    /// <summary>写入功能码</summary>
    public FunctionCodes WriteCode { get; set; }

    /// <summary>通道</summary>
    public IDriver Driver { get; set; }

    /// <summary>设备</summary>
    public IDevice Device { get; set; }

    /// <summary>参数</summary>
    public IDriverParameter Parameter { get; set; }
}