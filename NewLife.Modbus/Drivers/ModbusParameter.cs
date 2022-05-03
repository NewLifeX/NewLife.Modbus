using System.ComponentModel;
using NewLife.IoT.Protocols;

namespace NewLife.IoT.Drivers;

/// <summary>Modbus参数</summary>
public class ModbusParameter : IDriverParameter
{
    /// <summary>主机/站号</summary>
    [Description("主机/站号")]
    public Byte Host { get; set; }

    /// <summary>读取功能码。若点位地址未指定区域，则采用该功能码</summary>
    [Description("读取功能码。若点位地址未指定区域，则采用该功能码")]
    public FunctionCodes ReadCode { get; set; }

    /// <summary>写入功能码。若点位地址未指定区域，则采用该功能码</summary>
    [Description("写入功能码。若点位地址未指定区域，则采用该功能码")]
    public FunctionCodes WriteCode { get; set; }
}