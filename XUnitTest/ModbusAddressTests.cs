using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

public class ModbusAddressTests
{
    [Fact]
    public void ParseNumericAddress()
    {
        var ok = ModbusAddress.TryParse("100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(100, addr.Address);
        Assert.Null(addr.Range);
    }

    [Fact]
    public void ParseZeroAddress()
    {
        var ok = ModbusAddress.TryParse("0", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(0, addr.Address);
        Assert.Null(addr.Range);
    }

    [Fact]
    public void ParseHexAddress_0x()
    {
        // "0x64" → DO range, Address = 64 (the digits "64" are parsed as decimal)
        var ok = ModbusAddress.TryParse("0x64", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DO, addr.Range);
        Assert.Equal(64, addr.Address);
    }

    [Fact]
    public void ParseDOAddress()
    {
        var ok = ModbusAddress.TryParse("DO100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DO, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseDIAddress()
    {
        var ok = ModbusAddress.TryParse("DI100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DI, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseAIAddress()
    {
        var ok = ModbusAddress.TryParse("AI100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AI, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseAOAddress()
    {
        var ok = ModbusAddress.TryParse("AO100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AO, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseRange1x_DI()
    {
        // "1x" prefix → DI range
        var ok = ModbusAddress.TryParse("1x100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DI, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseRange3x_AI()
    {
        // "3x" prefix → AI range
        var ok = ModbusAddress.TryParse("3x100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AI, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseRange4x_AO()
    {
        // "4x" prefix → AO range
        var ok = ModbusAddress.TryParse("4x100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AO, addr.Range);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseAutoDetect_DI()
    {
        // 10100 → DI range (10000–19999)
        var ok = ModbusAddress.TryParse("10100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.DI, addr.Range);
        Assert.Equal(10100, addr.Address);
    }

    [Fact]
    public void ParseAutoDetect_AI()
    {
        // 30100 → AI range (30000–39999)
        var ok = ModbusAddress.TryParse("30100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AI, addr.Range);
        Assert.Equal(30100, addr.Address);
    }

    [Fact]
    public void ParseAutoDetect_AO()
    {
        // 40100 → AO range (40000–49999)
        var ok = ModbusAddress.TryParse("40100", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(ModbusRange.AO, addr.Range);
        Assert.Equal(40100, addr.Address);
    }

    [Fact]
    public void ParseWithBitField_Colon()
    {
        // "100:3" → bit field stripped, Address = 100
        var ok = ModbusAddress.TryParse("100:3", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseWithBitField_Dot()
    {
        // "100.3" → bit field stripped, Address = 100
        var ok = ModbusAddress.TryParse("100.3", out var addr);
        Assert.True(ok);
        Assert.NotNull(addr);
        Assert.Equal(100, addr.Address);
    }

    [Fact]
    public void ParseEmpty_ReturnsFalse()
    {
        var ok = ModbusAddress.TryParse("", out var addr);
        Assert.False(ok);
        Assert.Null(addr);
    }

    [Fact]
    public void ParseNull_ReturnsFalse()
    {
        var ok = ModbusAddress.TryParse(null!, out var addr);
        Assert.False(ok);
        Assert.Null(addr);
    }

    [Fact]
    public void GetReadCode_NoRange()
    {
        var ok = ModbusAddress.TryParse("100", out var addr);
        Assert.True(ok);
        Assert.Equal((FunctionCodes)0, addr!.GetReadCode());
    }

    [Fact]
    public void GetReadCode_DO()
    {
        var ok = ModbusAddress.TryParse("DO100", out var addr);
        Assert.True(ok);
        Assert.Equal(FunctionCodes.ReadCoil, addr!.GetReadCode());
    }

    [Fact]
    public void GetReadCode_DI()
    {
        var ok = ModbusAddress.TryParse("DI100", out var addr);
        Assert.True(ok);
        Assert.Equal(FunctionCodes.ReadDiscrete, addr!.GetReadCode());
    }

    [Fact]
    public void GetReadCode_AI()
    {
        var ok = ModbusAddress.TryParse("AI100", out var addr);
        Assert.True(ok);
        Assert.Equal(FunctionCodes.ReadInput, addr!.GetReadCode());
    }

    [Fact]
    public void GetReadCode_AO()
    {
        var ok = ModbusAddress.TryParse("AO100", out var addr);
        Assert.True(ok);
        Assert.Equal(FunctionCodes.ReadRegister, addr!.GetReadCode());
    }

    [Fact]
    public void GetWriteCode_DO()
    {
        var ok = ModbusAddress.TryParse("DO100", out var addr);
        Assert.True(ok);
        Assert.Equal(FunctionCodes.WriteCoil, addr!.GetWriteCode());
    }

    [Fact]
    public void GetWriteCode_AO()
    {
        var ok = ModbusAddress.TryParse("AO100", out var addr);
        Assert.True(ok);
        Assert.Equal(FunctionCodes.WriteRegister, addr!.GetWriteCode());
    }

    [Fact]
    public void GetWriteCode_DI_ReadOnly()
    {
        // DI (discrete input) is read-only, no write code
        var ok = ModbusAddress.TryParse("DI100", out var addr);
        Assert.True(ok);
        Assert.Equal((FunctionCodes)0, addr!.GetWriteCode());
    }
}
