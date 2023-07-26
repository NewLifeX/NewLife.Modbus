using System.ComponentModel;

namespace NewLife.IoT.Drivers;

/// <summary>ModbusIp参数</summary>
public class ModbusIpParameter : ModbusParameter, IDriverParameterKey
{
    /// <summary>地址。tcp地址如127.0.0.1:502</summary>
    [Description("地址。tcp地址如127.0.0.1:502")]
    public String Server { get; set; }

    /// <summary>获取唯一标识</summary>
    /// <returns></returns>
    public String GetKey() => Server;
}