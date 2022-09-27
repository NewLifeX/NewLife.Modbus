using System.ComponentModel;

namespace NewLife.IoT.Models;

/// <summary>寄存器单元</summary>
public class RegisterUnit
{
    /// <summary>地址</summary>
    [ReadOnly(true)]
    public Int32 Address { get; set; }

    /// <summary>数值。用户视角的数值，Modbus是大端字节序</summary>
    public UInt16 Value { get; set; }

    /// <summary>十六进制表示</summary>
    public String Hex => Value.GetBytes(false).ToHex();

    /// <summary>获取该寄存器单元的字节数据。Modbus是大端字节序</summary>
    /// <returns></returns>
    public Byte[] GetData() => Value.GetBytes(false);
}
