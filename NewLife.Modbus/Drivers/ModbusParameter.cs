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

    /// <summary>网络超时。发起请求后等待响应的超时时间，默认3000ms</summary>
    [Description("网络超时。发起请求后等待响应的超时时间，默认3000ms")]
    public Int32 Timeout { get; set; } = 3000;

    /// <summary>批大小。凑批请求时，每批最多点位个数</summary>
    [Description("批大小。凑批请求时，每批最多点位个数")]
    public Int32 BatchSize { get; set; }

    /// <summary>延迟。相邻请求之间的延迟时间，单位毫秒</summary>
    [Description("延迟。相邻请求之间的延迟时间，单位毫秒")]
    public Int32 Delay { get; set; }
}