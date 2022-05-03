﻿using NewLife.IoT.Protocols;
using NewLife.Serialization;

namespace NewLife.IoT.Drivers;

/// <summary>
/// ModbusTcp协议封装
/// </summary>
[Driver("ModbusTcp")]
public class ModbusTcpDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建驱动参数对象，可序列化成Xml/Json作为该协议的参数模板
    /// </summary>
    /// <returns></returns>
    public override IDriverParameter CreateParameter() => new ModbusTcpParameter
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
    protected override Modbus CreateModbus(IDevice device, ModbusNode node, IDictionary<String, Object> parameters)
    {
        var p = JsonHelper.Convert<ModbusTcpParameter>(parameters);
        if (p.Server.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址server");

        node.Parameter = p;

        var modbus = new ModbusTcp
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