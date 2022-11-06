using System.ComponentModel;
using NewLife.IoT.Protocols;
using NewLife.Serialization;

namespace NewLife.IoT.Drivers;

/// <summary>UDP网络版Modbus</summary>
[Driver("ModbusUdp")]
[DisplayName("UDP网络版ModbusUdp")]
public class ModbusUdpDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建驱动参数对象，可序列化成Xml/Json作为该协议的参数模板
    /// </summary>
    /// <returns></returns>
    public override IDriverParameter GetDefaultParameter() => new ModbusUdpParameter
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
        var p = JsonHelper.Convert<ModbusUdpParameter>(parameters);
        if (p.Server.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址Server");

        node.Parameter = p;

        var modbus = new ModbusUdp
        {
            Server = p.Server,
            ProtocolId = p.ProtocolId,

            Tracer = Tracer,
            Log = Log,
        };
        //modbus.Init(parameters);

        return modbus;
    }
    #endregion
}