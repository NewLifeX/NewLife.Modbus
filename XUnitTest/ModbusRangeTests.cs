using System;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusRange的补充单元测试</summary>
public class ModbusRangeTests
{
    #region Contain
    [Fact]
    public void Contain_InRange()
    {
        Assert.True(ModbusRange.DO.Contain(0));
        Assert.True(ModbusRange.DO.Contain(5000));
        Assert.True(ModbusRange.DO.Contain(9999));
    }

    [Fact]
    public void Contain_OutOfRange()
    {
        Assert.False(ModbusRange.DO.Contain(10000));
    }

    [Fact]
    public void Contain_BoundaryStart()
    {
        Assert.True(ModbusRange.DI.Contain(10000));
        Assert.False(ModbusRange.DI.Contain(9999));
    }

    [Fact]
    public void Contain_BoundaryEnd()
    {
        Assert.True(ModbusRange.DI.Contain(19999));
        Assert.False(ModbusRange.DI.Contain(20000));
    }

    [Fact]
    public void Contain_AI_Range()
    {
        Assert.True(ModbusRange.AI.Contain(30000));
        Assert.True(ModbusRange.AI.Contain(35000));
        Assert.True(ModbusRange.AI.Contain(39999));
        Assert.False(ModbusRange.AI.Contain(29999));
        Assert.False(ModbusRange.AI.Contain(40000));
    }

    [Fact]
    public void Contain_AO_Range()
    {
        Assert.True(ModbusRange.AO.Contain(40000));
        Assert.True(ModbusRange.AO.Contain(45000));
        Assert.True(ModbusRange.AO.Contain(49999));
        Assert.False(ModbusRange.AO.Contain(39999));
        Assert.False(ModbusRange.AO.Contain(50000));
    }

    [Fact]
    public void Contain_GapBetween_DI_AI()
    {
        // 20000-29999之间无标准范围
        Assert.False(ModbusRange.DI.Contain(20000));
        Assert.False(ModbusRange.AI.Contain(20000));
    }
    #endregion

    #region 静态范围定义
    [Fact]
    public void DO_Properties()
    {
        Assert.Equal(0, ModbusRange.DO.Start);
        Assert.Equal(9999, ModbusRange.DO.End);
        Assert.Equal(FunctionCodes.ReadCoil, ModbusRange.DO.ReadCode);
        Assert.Equal(FunctionCodes.WriteCoil, ModbusRange.DO.WriteCode);
    }

    [Fact]
    public void DI_Properties()
    {
        Assert.Equal(10000, ModbusRange.DI.Start);
        Assert.Equal(19999, ModbusRange.DI.End);
        Assert.Equal(FunctionCodes.ReadDiscrete, ModbusRange.DI.ReadCode);
        Assert.Equal((FunctionCodes)0, ModbusRange.DI.WriteCode);
    }

    [Fact]
    public void AI_Properties()
    {
        Assert.Equal(30000, ModbusRange.AI.Start);
        Assert.Equal(39999, ModbusRange.AI.End);
        Assert.Equal(FunctionCodes.ReadInput, ModbusRange.AI.ReadCode);
        Assert.Equal((FunctionCodes)0, ModbusRange.AI.WriteCode);
    }

    [Fact]
    public void AO_Properties()
    {
        Assert.Equal(40000, ModbusRange.AO.Start);
        Assert.Equal(49999, ModbusRange.AO.End);
        Assert.Equal(FunctionCodes.ReadRegister, ModbusRange.AO.ReadCode);
        Assert.Equal(FunctionCodes.WriteRegister, ModbusRange.AO.WriteCode);
    }
    #endregion

    #region 自定义范围
    [Fact]
    public void CustomRange_Contain()
    {
        var range = new ModbusRange
        {
            Start = 100,
            End = 200,
            ReadCode = FunctionCodes.ReadRegister,
            WriteCode = FunctionCodes.WriteRegister
        };

        Assert.True(range.Contain(100));
        Assert.True(range.Contain(150));
        Assert.True(range.Contain(200));
        Assert.False(range.Contain(99));
        Assert.False(range.Contain(201));
    }
    #endregion
}
