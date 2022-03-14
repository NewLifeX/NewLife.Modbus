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
        var portName = parameters["PortName"] as String ?? parameters["Address"] as String;
        if (portName.IsNullOrEmpty()) throw new ArgumentException("参数中未指定地址address");

        var baudrate = parameters["Baudrate"].ToInt();
        if (baudrate <= 0) baudrate = 9600;

        var modbus = new ModbusRtu
        {
            PortName = portName,
            Baudrate = baudrate,
            Tracer = Tracer,
            Log = Log,
        };

        return modbus;
    }
    #endregion
}