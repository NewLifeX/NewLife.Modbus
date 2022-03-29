using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;

namespace NewLife.Serial.Protocols;

/// <summary>
/// ModbusRtu协议封装
/// </summary>
[Driver("ModbusRtu")]
public class ModbusRtuDriver : ModbusDriver, IDriver
{
    #region 方法
    /// <summary>
    /// 创建Modbus通道
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    protected override Modbus CreateModbus(IChannel channel, IDictionary<String, Object> parameters)
    {
        //var portName = parameter.PortName ?? parameter.Address;
        //if (portName.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址address");

        //var baudrate = parameter.Baudrate;
        //if (baudrate <= 0) baudrate = 9600;

        var modbus = new ModbusRtu
        {
            //PortName = portName,
            //Baudrate = baudrate,
            Tracer = Tracer,
            Log = Log,
        };
        modbus.Init(parameters);

        return modbus;
    }
    #endregion
}