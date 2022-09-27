using System.ComponentModel;

namespace NewLife.IoT.Models;

/// <summary>线圈单元</summary>
public class CoilUnit
{
    /// <summary>线圈地址</summary>
    [ReadOnly(true)]
    public Int32 Address { get; set; }

    /// <summary>数值。用户视角的数值，实际上线圈只有一位</summary>
    public Byte Value { get; set; }
}