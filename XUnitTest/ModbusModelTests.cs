using System;
using NewLife.IoT.Models;
using Xunit;

namespace XUnitTest;

/// <summary>模型单元测试</summary>
public class ModbusModelTests
{
    #region RegisterUnit
    [Fact]
    public void RegisterUnit_DefaultValues()
    {
        var reg = new RegisterUnit();
        Assert.Equal(0, reg.Address);
        Assert.Equal(0, reg.Value);
    }

    [Fact]
    public void RegisterUnit_SetValues()
    {
        var reg = new RegisterUnit { Address = 100, Value = 0x1234 };
        Assert.Equal(100, reg.Address);
        Assert.Equal(0x1234, reg.Value);
    }

    [Fact]
    public void RegisterUnit_Hex()
    {
        var reg = new RegisterUnit { Value = 0x1234 };
        Assert.Equal("1234", reg.Hex);
    }

    [Fact]
    public void RegisterUnit_Hex_Zero()
    {
        var reg = new RegisterUnit { Value = 0 };
        Assert.Equal("0000", reg.Hex);
    }

    [Fact]
    public void RegisterUnit_Hex_MaxValue()
    {
        var reg = new RegisterUnit { Value = 0xFFFF };
        Assert.Equal("FFFF", reg.Hex);
    }

    [Fact]
    public void RegisterUnit_GetData()
    {
        var reg = new RegisterUnit { Value = 0x1234 };
        var data = reg.GetData();
        Assert.Equal(2, data.Length);
        Assert.Equal(0x12, data[0]);
        Assert.Equal(0x34, data[1]);
    }

    [Fact]
    public void RegisterUnit_GetData_Zero()
    {
        var reg = new RegisterUnit { Value = 0 };
        var data = reg.GetData();
        Assert.Equal(2, data.Length);
        Assert.Equal(0x00, data[0]);
        Assert.Equal(0x00, data[1]);
    }

    [Fact]
    public void RegisterUnit_GetData_BigEndian()
    {
        // Modbus大端序
        var reg = new RegisterUnit { Value = 0xABCD };
        var data = reg.GetData();
        Assert.Equal(0xAB, data[0]);
        Assert.Equal(0xCD, data[1]);
    }
    #endregion

    #region CoilUnit
    [Fact]
    public void CoilUnit_DefaultValues()
    {
        var coil = new CoilUnit();
        Assert.Equal(0, coil.Address);
        Assert.Equal(0, coil.Value);
    }

    [Fact]
    public void CoilUnit_SetValues()
    {
        var coil = new CoilUnit { Address = 50, Value = 1 };
        Assert.Equal(50, coil.Address);
        Assert.Equal(1, coil.Value);
    }

    [Fact]
    public void CoilUnit_Value_On()
    {
        var coil = new CoilUnit { Value = 1 };
        Assert.Equal(1, coil.Value);
    }

    [Fact]
    public void CoilUnit_Value_Off()
    {
        var coil = new CoilUnit { Value = 0 };
        Assert.Equal(0, coil.Value);
    }

    [Fact]
    public void CoilUnit_Value_ByteRange()
    {
        // CoilUnit.Value 是 Byte 类型，可以存储0-255
        var coil = new CoilUnit { Value = 255 };
        Assert.Equal(255, coil.Value);
    }
    #endregion
}
