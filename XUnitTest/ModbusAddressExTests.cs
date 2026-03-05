using System;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusAddress的补充单元测试</summary>
public class ModbusAddressExTests
{
    #region TryParse - 未覆盖的边界
    [Fact]
    public void ParseAddress_LowercaseDO()
    {
        var ok = ModbusAddress.TryParse("do100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DO, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseAddress_LowercaseDI()
    {
        var ok = ModbusAddress.TryParse("di50", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DI, addr.Range);
        Assert.Equal(50, addr.Address);
    }

    [Fact]
    public void ParseAddress_LowercaseAI()
    {
        var ok = ModbusAddress.TryParse("ai200", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AI, addr.Range);
        Assert.Equal(200, addr.Address);
    }

    [Fact]
    public void ParseAddress_LowercaseAO()
    {
        var ok = ModbusAddress.TryParse("ao300", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AO, addr.Range);
        Assert.Equal(300, addr.Address);
    }

    [Fact]
    public void ParseAddress_GapRange_20000()
    {
        // 20000-29999 不匹配任何标准范围
        var ok = ModbusAddress.TryParse("20000", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Null(addr.Range);
        Assert.Equal(20000, addr.Address);
    }

    [Fact]
    public void ParseAddress_BelowDI_9999()
    {
        // 9999 < 10000 不自动检测
        var ok = ModbusAddress.TryParse("9999", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Null(addr.Range);
    }

    [Fact]
    public void ParseAddress_AtDI_Boundary_10000()
    {
        var ok = ModbusAddress.TryParse("10000", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DI, addr.Range);
    }

    [Fact]
    public void ParseAddress_AtDI_End_19999()
    {
        var ok = ModbusAddress.TryParse("19999", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DI, addr.Range);
    }

    [Fact]
    public void ParseAddress_AtAI_Start_30000()
    {
        var ok = ModbusAddress.TryParse("30000", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AI, addr.Range);
    }

    [Fact]
    public void ParseAddress_AtAO_Start_40000()
    {
        var ok = ModbusAddress.TryParse("40000", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AO, addr.Range);
    }

    [Fact]
    public void ParseAddress_AboveAO_50000()
    {
        // 50000超过AO范围（40000-49999）
        var ok = ModbusAddress.TryParse("50000", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Null(addr.Range);
    }
    #endregion

    #region 位域处理
    [Fact]
    public void ParseWithBitField_DOPrefix()
    {
        var ok = ModbusAddress.TryParse("DO100:5", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DO, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseWithBitField_DotSeparator_WithPrefix()
    {
        var ok = ModbusAddress.TryParse("AO100.7", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AO, addr.Range);
        Assert.Equal(100, addr.Address);
    }
    #endregion

    #region GetWriteCode - 只读范围
    [Fact]
    public void GetWriteCode_AI_ReadOnly()
    {
        var ok = ModbusAddress.TryParse("AI100", out var addr);
        Assert.True(ok);
        Assert.Equal((FunctionCodes)0, addr!.GetWriteCode());
    }

    [Fact]
    public void GetWriteCode_NoRange()
    {
        var ok = ModbusAddress.TryParse("100", out var addr);
        Assert.True(ok);
        Assert.Equal((FunctionCodes)0, addr!.GetWriteCode());
    }
    #endregion

    #region 0x前缀
    [Fact]
    public void ParseHex_0x0()
    {
        var ok = ModbusAddress.TryParse("0x0", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DO, addr.Range);
        Assert.Equal(0, addr.Address);
    }

    [Fact]
    public void ParseHex_0X_Uppercase()
    {
        var ok = ModbusAddress.TryParse("0X100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DO, addr.Range);
        Assert.Equal(100, addr.Address);
    }
    #endregion
}
