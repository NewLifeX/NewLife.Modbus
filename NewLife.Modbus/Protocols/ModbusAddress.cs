namespace NewLife.IoT.Protocols;

/// <summary>Modbus点位地址。支持区域和位域</summary>
public class ModbusAddress
{
    #region 属性
    /// <summary>区域</summary>
    public ModbusRange Range { get; set; }

    /// <summary>地址</summary>
    public UInt16 Address { get; set; }
    #endregion

    #region 方法
    /// <summary>
    /// 解析字符串地址，识别其中的区域信息
    /// </summary>
    /// <param name="address"></param>
    /// <param name="modbusAddress"></param>
    /// <returns></returns>
    public static Boolean TryParse(String address, out ModbusAddress modbusAddress)
    {
        modbusAddress = null;
        if (address.IsNullOrEmpty()) return false;

        // 去掉冒号后面的位域
        var addr = address;
        var p = addr.IndexOfAny([':', '.']);
        if (p > 0) addr = addr.Substring(0, p);

        modbusAddress = new ModbusAddress();

        // 字母开头 DO/DI/AI/AO
        if (addr.StartsWithIgnoreCase("DO", "0x"))
        {
            modbusAddress.Range = ModbusRange.DO;
            modbusAddress.Address = (UInt16)addr.Substring(2).ToInt();
        }
        else if (addr.StartsWithIgnoreCase("DI", "1x"))
        {
            modbusAddress.Range = ModbusRange.DI;
            modbusAddress.Address = (UInt16)addr.Substring(2).ToInt();
        }
        else if (addr.StartsWithIgnoreCase("AI", "3x"))
        {
            modbusAddress.Range = ModbusRange.AI;
            modbusAddress.Address = (UInt16)addr.Substring(2).ToInt();
        }
        else if (addr.StartsWithIgnoreCase("AO", "4x"))
        {
            modbusAddress.Range = ModbusRange.AO;
            modbusAddress.Address = (UInt16)addr.Substring(2).ToInt();
        }
        else
        {
            modbusAddress.Address = (UInt16)addr.ToInt();

            // 字段计算 DI/AI/AO，不计算 DO，因为部分非标设备的保存寄存器从0开始
            if (modbusAddress.Address >= 10000)
            {
                if (ModbusRange.DI.Contain(modbusAddress.Address))
                    modbusAddress.Range = ModbusRange.DI;
                else if (ModbusRange.AI.Contain(modbusAddress.Address))
                    modbusAddress.Range = ModbusRange.AI;
                else if (ModbusRange.AO.Contain(modbusAddress.Address))
                    modbusAddress.Range = ModbusRange.AO;
            }
        }

        return true;
    }

    /// <summary>
    /// 根据地址所在区域，获取读取功能码
    /// </summary>
    /// <returns></returns>
    public FunctionCodes GetReadCode() => Range?.ReadCode ?? 0;

    /// <summary>
    /// 根据地址所在区域，获取写入功能码
    /// </summary>
    /// <returns></returns>
    public FunctionCodes GetWriteCode() => Range?.WriteCode ?? 0;
    #endregion
}