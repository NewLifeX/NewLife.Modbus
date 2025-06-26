﻿using System.ComponentModel;
using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.Serial.Protocols;

namespace NewLife.Serial.Drivers;

/// <summary>ModbusAscii协议驱动</summary>
/// <remarks>
/// 每个串口对应一个Modbus驱动实例，避免多个虚拟设备实例化多个驱动实例导致串口争夺。
/// 该唯一性由驱动工厂DriverFactory来保证。
/// </remarks>
[Driver("ModbusASCII")]
[DisplayName("串口ModbusASCII")]
public class ModbusAsciiDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建驱动参数对象，可序列化成Xml/Json作为该协议的参数模板
    /// </summary>
    /// <returns></returns>
    protected override IDriverParameter OnCreateParameter() => new ModbusRtuParameter
    {
        PortName = "COM1",
        Baudrate = 9600,

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
    protected override Modbus CreateModbus(IDevice device, ModbusNode node, ModbusParameter parameter)
    {
        var p = parameter as ModbusRtuParameter;
        if (p == null || p.PortName.IsNullOrEmpty()) throw new ArgumentException("参数中未指定端口PortName");

        if (p.Baudrate <= 0) p.Baudrate = 9600;

        node.Parameter = p;

        var modbus = new ModbusAscii
        {
            PortName = p.PortName,
            Baudrate = p.Baudrate,

            Tracer = Tracer,
            Log = Log,
        };
        //if (p.ByteTimeout > 0) modbus.ByteTimeout = p.ByteTimeout;
        //modbus.Init(parameters);

        return modbus;
    }
    #endregion
}