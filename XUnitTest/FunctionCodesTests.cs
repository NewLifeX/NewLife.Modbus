using System;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

/// <summary>FunctionCodes枚举单元测试</summary>
public class FunctionCodesTests
{
    [Fact]
    public void ReadCoil_Value()
    {
        Assert.Equal(1, (Byte)FunctionCodes.ReadCoil);
    }

    [Fact]
    public void ReadDiscrete_Value()
    {
        Assert.Equal(2, (Byte)FunctionCodes.ReadDiscrete);
    }

    [Fact]
    public void ReadRegister_Value()
    {
        Assert.Equal(3, (Byte)FunctionCodes.ReadRegister);
    }

    [Fact]
    public void ReadInput_Value()
    {
        Assert.Equal(4, (Byte)FunctionCodes.ReadInput);
    }

    [Fact]
    public void WriteCoil_Value()
    {
        Assert.Equal(5, (Byte)FunctionCodes.WriteCoil);
    }

    [Fact]
    public void WriteRegister_Value()
    {
        Assert.Equal(6, (Byte)FunctionCodes.WriteRegister);
    }

    [Fact]
    public void Diagnostics_Value()
    {
        Assert.Equal(8, (Byte)FunctionCodes.Diagnostics);
    }

    [Fact]
    public void WriteCoils_Value()
    {
        Assert.Equal(15, (Byte)FunctionCodes.WriteCoils);
    }

    [Fact]
    public void WriteRegisters_Value()
    {
        Assert.Equal(16, (Byte)FunctionCodes.WriteRegisters);
    }

    [Fact]
    public void WriteFileRecord_Value()
    {
        Assert.Equal(21, (Byte)FunctionCodes.WriteFileRecord);
    }

    [Fact]
    public void ReadWriteMultipleRegisters_Value()
    {
        Assert.Equal(23, (Byte)FunctionCodes.ReadWriteMultipleRegisters);
    }

    [Fact]
    public void ReadDevId_Value()
    {
        Assert.Equal(43, (Byte)FunctionCodes.ReadDevId);
    }
}
