using System.ComponentModel;
using NewLife.IoT.Protocols;
using NewLife.Serialization;

namespace NewLife.IoT.Drivers;

/// <summary>UDP网络版ModbusRTU</summary>
[Driver("ModbusRtuOverUdp")]
[DisplayName("UDP网络版ModbusRTU")]
public class ModbusRtuOverUdpDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建驱动参数对象，可序列化成Xml/Json作为该协议的参数模板
    /// </summary>
    /// <returns></returns>
    public override IDriverParameter GetDefaultParameter() => new ModbusIpParameter
    {
        Server = "127.0.0.1:502",

        Host = 1,
        ReadCode = FunctionCodes.ReadRegister,
        WriteCode = FunctionCodes.WriteRegister,
    };

    /// <summary>
    /// 创建Modbus通道
    /// </summary>
    /// <param name="device">逻辑设备</param>
    /// <param name="node">设备节点</param>
    /// <param name="parameters">参数</param>
    /// <returns></returns>
    internal protected override Modbus CreateModbus(IDevice device, ModbusNode node, IDictionary<String, Object> parameters)
    {
        var p = JsonHelper.Convert<ModbusIpParameter>(parameters);
        if (p.Server.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址Server");

        node.Parameter = p;

        var modbus = new ModbusRtuOverUdp
        {
            Server = p.Server,

            Tracer = Tracer,
            Log = Log,
        };
        //modbus.Init(parameters);

        return modbus;
    }
    #endregion
}