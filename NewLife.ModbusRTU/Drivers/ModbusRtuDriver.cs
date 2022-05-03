﻿using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.Serial.Protocols;
using NewLife.Serialization;

namespace NewLife.Serial.Drivers;

/// <summary>
/// ModbusRtu协议封装
/// </summary>
[Driver("ModbusRtu")]
public class ModbusRtuDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建驱动参数对象，可序列化成Xml/Json作为该协议的参数模板
    /// </summary>
    /// <returns></returns>
    public override IDriverParameter CreateParameter() => new ModbusRtuParameter
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
    /// <param name="parameters">参数</param>
    /// <returns></returns>
    protected override Modbus CreateModbus(IDevice device, ModbusNode node, IDictionary<String, Object> parameters)
    {
        var p = JsonHelper.Convert<ModbusRtuParameter>(parameters);
        if (p.PortName.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址address");

        if (p.Baudrate <= 0) p.Baudrate = 9600;

        node.Parameter = p;

        var modbus = new ModbusRtu
        {
            PortName = p.PortName,
            Baudrate = p.Baudrate,

            Tracer = Tracer,
            Log = Log,
        };
        //modbus.Init(parameters);

        return modbus;
    }
    #endregion
}