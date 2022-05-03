namespace NewLife.IoT.Protocols;

/// <summary>Modbus地址分为四个区</summary>
public class ModbusRange
{
    #region 属性
    /// <summary>开始地址</summary>
    public UInt16 Start { get; set; }

    /// <summary>结束地址</summary>
    public UInt16 End { get; set; }

    /// <summary>读取功能码</summary>
    public FunctionCodes ReadCode { get; set; }

    /// <summary>写入功能码</summary>
    public FunctionCodes WriteCode { get; set; }
    #endregion

    #region 方法
    /// <summary>
    /// 指定地址是否包含在该区域
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public Boolean Contain(UInt16 address) => address >= Start && address <= End;
    #endregion

    #region 静态
    /// <summary>数字输出，线圈读写 0x</summary>
    public static ModbusRange DO { get; set; } = new ModbusRange
    {
        Start = 0,
        End = 09999,
        ReadCode = FunctionCodes.ReadCoil,
        WriteCode = FunctionCodes.WriteCoil
    };

    /// <summary>数字输入，触点只读 1x</summary>
    public static ModbusRange DI { get; set; } = new ModbusRange
    {
        Start = 10000,
        End = 19999,
        ReadCode = FunctionCodes.ReadDiscrete
    };

    /// <summary>模拟输入，输入寄存器只读 3x</summary>
    public static ModbusRange AI { get; set; } = new ModbusRange
    {
        Start = 30000,
        End = 39999,
        ReadCode = FunctionCodes.ReadInput
    };

    /// <summary>模拟输出，输出寄存器读写 4x</summary>
    public static ModbusRange AO { get; set; } = new ModbusRange
    {
        Start = 40000,
        End = 49999,
        ReadCode = FunctionCodes.ReadRegister,
        WriteCode = FunctionCodes.WriteRegister
    };
    #endregion
}