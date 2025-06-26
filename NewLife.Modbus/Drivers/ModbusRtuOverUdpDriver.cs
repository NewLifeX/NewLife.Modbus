﻿using System.ComponentModel;
using NewLife.IoT.Protocols;

namespace NewLife.IoT.Drivers;

/// <summary>UDP网络版ModbusRTU</summary>
/// <remarks>
/// 每个Tcp/Udp从站地址，对应一个Modbus驱动实例，避免多个虚拟设备实例化多个驱动实例导致网络连接过多。
/// 该唯一性由驱动工厂DriverFactory来保证。
/// </remarks>
[Driver("ModbusRtuOverUdp")]
[DisplayName("UDP网络版ModbusRTU")]
public class ModbusRtuOverUdpDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建驱动参数对象，可序列化成Xml/Json作为该协议的参数模板
    /// </summary>
    /// <returns></returns>
    protected override IDriverParameter OnCreateParameter() => new ModbusIpParameter
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
    /// <param name="parameter">参数</param>
    /// <returns></returns>
    internal protected override Modbus CreateModbus(IDevice device, ModbusNode node, ModbusParameter parameter)
    {
        var p = parameter as ModbusIpParameter;
        if (p == null || p.Server.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址Server");

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