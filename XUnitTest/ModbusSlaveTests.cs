using NewLife.IoT;
using NewLife.IoT.Models;
using NewLife.IoT.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace XUnitTest;

/// <summary>Modbus从机集成测试</summary>
public class ModbusSlaveTests : IDisposable
{
    private readonly ModbusSlave _slave;

    public ModbusSlaveTests()
    {
        _slave = new ModbusSlave
        {
            Port = 1506,
            Registers = Enumerable.Range(0, 10)
                .Select(i => new RegisterUnit { Address = i, Value = (UInt16)(100 + i) })
                .ToList(),
            Coils = Enumerable.Range(0, 16)
                .Select(i => new CoilUnit { Address = i, Value = (Byte)(i % 2) })
                .ToList(),
        };
        _slave.Start();
        Thread.Sleep(200);
    }

    public void Dispose() => _slave.Dispose();

    private ModbusTcp CreateClient()
    {
        var client = new ModbusTcp
        {
            Server = "tcp://localhost:1506",
            Timeout = 3000,
        };
        client.Open();
        return client;
    }

    [Fact]
    public void ReadRegister()
    {
        using var client = CreateClient();

        var rs = client.ReadRegister(1, 0, 5);
        Assert.NotNull(rs);
        Assert.Equal(5, rs.Length);
        for (var i = 0; i < 5; i++)
            Assert.Equal((UInt16)(100 + i), rs[i]);
    }

    [Fact]
    public void ReadRegister_Single()
    {
        using var client = CreateClient();

        var rs = client.ReadRegister(1, 7, 1);
        Assert.NotNull(rs);
        Assert.Single(rs);
        Assert.Equal((UInt16)107, rs[0]);
    }

    [Fact]
    public void ReadCoil()
    {
        using var client = CreateClient();

        // coils 0..7: value = i % 2, so odd indices are ON
        var rs = client.ReadCoil(1, 0, 8);
        Assert.NotNull(rs);
        Assert.Equal(8, rs.Length);
        for (var i = 0; i < 8; i++)
            Assert.Equal(i % 2 == 1, rs[i]);
    }

    [Fact]
    public void WriteRegister_EchosAddressAndValue()
    {
        using var client = CreateClient();

        var rs = client.WriteRegister(1, 3, 0xABCD);
        Assert.Equal(0xABCD, rs);

        // Verify the register was updated
        var regs = client.ReadRegister(1, 3, 1);
        Assert.Equal(0xABCD, regs[0]);
    }

    [Fact]
    public void WriteCoil_TurnOn()
    {
        using var client = CreateClient();

        // Coil 0 starts as 0 (off)
        var before = client.ReadCoil(1, 0, 1);
        Assert.False(before[0]);

        var rs = client.WriteCoil(1, 0, 0xFF00);
        Assert.Equal(0xFF00, rs);

        var after = client.ReadCoil(1, 0, 1);
        Assert.True(after[0]);
    }

    [Fact]
    public void WriteCoil_TurnOff()
    {
        using var client = CreateClient();

        // Coil 1 starts as 1 (on)
        var before = client.ReadCoil(1, 1, 1);
        Assert.True(before[0]);

        var rs = client.WriteCoil(1, 1, 0x0000);
        Assert.Equal(0x0000, rs);

        var after = client.ReadCoil(1, 1, 1);
        Assert.False(after[0]);
    }

    [Fact]
    public void WriteRegisters_MultipleValues()
    {
        using var client = CreateClient();

        var values = new UInt16[] { 0x1234, 0x5678, 0x9ABC };
        var rs = client.WriteRegisters(1, 5, values);
        Assert.Equal(3, rs);

        // Verify
        var regs = client.ReadRegister(1, 5, 3);
        Assert.Equal(0x1234, regs[0]);
        Assert.Equal(0x5678, regs[1]);
        Assert.Equal(0x9ABC, regs[2]);
    }

    [Fact]
    public void WriteCoils_MultipleValues()
    {
        using var client = CreateClient();

        // Write 8 coils: ON, OFF, ON, OFF, ON, OFF, ON, OFF
        var values = new UInt16[] { 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000 };
        var rs = client.WriteCoils(1, 0, values);
        Assert.Equal(8, rs);

        // Verify
        var coils = client.ReadCoil(1, 0, 8);
        for (var i = 0; i < 8; i++)
            Assert.Equal(i % 2 == 0, coils[i]);
    }
}
