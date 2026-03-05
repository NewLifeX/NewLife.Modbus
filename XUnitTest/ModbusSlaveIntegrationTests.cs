using NewLife.IoT;
using NewLife.IoT.Models;
using NewLife.IoT.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusSlave集成测试 - 覆盖更多场景</summary>
public class ModbusSlaveIntegrationTests : IDisposable
{
    private readonly ModbusSlave _slave;
    private readonly Int32 _port;

    public ModbusSlaveIntegrationTests()
    {
        _port = 1507;
        _slave = new ModbusSlave
        {
            Port = _port,
            Registers = Enumerable.Range(0, 20)
                .Select(i => new RegisterUnit { Address = i, Value = (UInt16)(200 + i) })
                .ToList(),
            Coils = Enumerable.Range(0, 32)
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
            Server = $"tcp://localhost:{_port}",
            Timeout = 3000,
        };
        client.Open();
        return client;
    }

    #region 读取寄存器
    [Fact]
    public void ReadRegister_AllRegisters()
    {
        using var client = CreateClient();
        var rs = client.ReadRegister(1, 0, 20);
        Assert.NotNull(rs);
        Assert.Equal(20, rs.Length);
        for (var i = 0; i < 20; i++)
            Assert.Equal((UInt16)(200 + i), rs[i]);
    }

    [Fact]
    public void ReadRegister_MiddleRange()
    {
        using var client = CreateClient();
        var rs = client.ReadRegister(1, 5, 5);
        Assert.NotNull(rs);
        Assert.Equal(5, rs.Length);
        for (var i = 0; i < 5; i++)
            Assert.Equal((UInt16)(205 + i), rs[i]);
    }

    [Fact]
    public void ReadRegister_LastRegister()
    {
        using var client = CreateClient();
        var rs = client.ReadRegister(1, 19, 1);
        Assert.NotNull(rs);
        Assert.Single(rs);
        Assert.Equal((UInt16)219, rs[0]);
    }

    [Fact]
    public void ReadRegister_FirstRegister()
    {
        using var client = CreateClient();
        var rs = client.ReadRegister(1, 0, 1);
        Assert.NotNull(rs);
        Assert.Single(rs);
        Assert.Equal((UInt16)200, rs[0]);
    }
    #endregion

    #region 读取线圈
    [Fact]
    public void ReadCoil_AllCoils()
    {
        using var client = CreateClient();
        var rs = client.ReadCoil(1, 0, 32);
        Assert.NotNull(rs);
        Assert.Equal(32, rs.Length);
        for (var i = 0; i < 32; i++)
            Assert.Equal(i % 2 == 1, rs[i]);
    }

    [Fact]
    public void ReadCoil_NonMultipleOf8()
    {
        using var client = CreateClient();
        var rs = client.ReadCoil(1, 0, 10);
        Assert.NotNull(rs);
        Assert.Equal(10, rs.Length);
        for (var i = 0; i < 10; i++)
            Assert.Equal(i % 2 == 1, rs[i]);
    }

    [Fact]
    public void ReadCoil_SingleCoil()
    {
        using var client = CreateClient();
        var rs = client.ReadCoil(1, 1, 1);
        Assert.NotNull(rs);
        Assert.Single(rs);
        Assert.True(rs[0]);
    }

    [Fact]
    public void ReadCoil_OffCoil()
    {
        using var client = CreateClient();
        var rs = client.ReadCoil(1, 0, 1);
        Assert.NotNull(rs);
        Assert.Single(rs);
        Assert.False(rs[0]);
    }

    [Fact]
    public void ReadCoil_PartialByte()
    {
        using var client = CreateClient();
        // 请求3个线圈，不是8的整数倍
        var rs = client.ReadCoil(1, 0, 3);
        Assert.NotNull(rs);
        Assert.Equal(3, rs.Length);
    }
    #endregion

    #region 写入寄存器
    [Fact]
    public void WriteRegister_ThenRead()
    {
        using var client = CreateClient();
        var rs = client.WriteRegister(1, 0, 0x9999);
        Assert.Equal(0x9999, rs);

        var regs = client.ReadRegister(1, 0, 1);
        Assert.Equal(0x9999, regs[0]);
    }

    [Fact]
    public void WriteRegister_MaxValue()
    {
        using var client = CreateClient();
        var rs = client.WriteRegister(1, 1, 0xFFFF);
        Assert.Equal(0xFFFF, rs);

        var regs = client.ReadRegister(1, 1, 1);
        Assert.Equal(0xFFFF, regs[0]);
    }

    [Fact]
    public void WriteRegister_ZeroValue()
    {
        using var client = CreateClient();
        var rs = client.WriteRegister(1, 2, 0x0000);
        Assert.Equal(0, rs);

        var regs = client.ReadRegister(1, 2, 1);
        Assert.Equal(0, regs[0]);
    }

    [Fact]
    public void WriteRegisters_ThenRead()
    {
        using var client = CreateClient();
        var values = new UInt16[] { 0xAAAA, 0xBBBB, 0xCCCC, 0xDDDD };
        var rs = client.WriteRegisters(1, 10, values);
        Assert.Equal(4, rs);

        var regs = client.ReadRegister(1, 10, 4);
        Assert.Equal(0xAAAA, regs[0]);
        Assert.Equal(0xBBBB, regs[1]);
        Assert.Equal(0xCCCC, regs[2]);
        Assert.Equal(0xDDDD, regs[3]);
    }

    [Fact]
    public void WriteRegisters_SingleValue()
    {
        using var client = CreateClient();
        var rs = client.WriteRegisters(1, 15, new UInt16[] { 0x1111 });
        Assert.Equal(1, rs);

        var regs = client.ReadRegister(1, 15, 1);
        Assert.Equal(0x1111, regs[0]);
    }
    #endregion

    #region 写入线圈
    [Fact]
    public void WriteCoil_ThenRead()
    {
        using var client = CreateClient();
        // 线圈0初始为0，写入1
        client.WriteCoil(1, 0, 0xFF00);
        var rs = client.ReadCoil(1, 0, 1);
        Assert.True(rs[0]);

        // 再关闭
        client.WriteCoil(1, 0, 0x0000);
        rs = client.ReadCoil(1, 0, 1);
        Assert.False(rs[0]);
    }

    [Fact]
    public void WriteCoils_ThenRead()
    {
        using var client = CreateClient();
        // 写入8个线圈：全开
        var values = new UInt16[] { 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00 };
        var rs = client.WriteCoils(1, 0, values);
        Assert.Equal(8, rs);

        var coils = client.ReadCoil(1, 0, 8);
        for (var i = 0; i < 8; i++)
            Assert.True(coils[i]);
    }

    [Fact]
    public void WriteCoils_AllOff_ThenRead()
    {
        using var client = CreateClient();
        // 全关
        var values = new UInt16[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        var rs = client.WriteCoils(1, 0, values);
        Assert.Equal(8, rs);

        var coils = client.ReadCoil(1, 0, 8);
        for (var i = 0; i < 8; i++)
            Assert.False(coils[i]);
    }

    [Fact]
    public void WriteCoils_NonMultipleOf8_ThenRead()
    {
        using var client = CreateClient();
        // 写5个线圈（非8的倍数）
        var values = new UInt16[] { 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00 };
        var rs = client.WriteCoils(1, 16, values);
        Assert.Equal(5, rs);

        var coils = client.ReadCoil(1, 16, 5);
        Assert.True(coils[0]);
        Assert.False(coils[1]);
        Assert.True(coils[2]);
        Assert.False(coils[3]);
        Assert.True(coils[4]);
    }

    [Fact]
    public void WriteCoils_Pattern_ThenRead()
    {
        using var client = CreateClient();
        // 交替模式
        var values = new UInt16[] { 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00, 0x0000 };
        var rs = client.WriteCoils(1, 16, values);
        Assert.Equal(16, rs);

        var coils = client.ReadCoil(1, 16, 16);
        for (var i = 0; i < 16; i++)
            Assert.Equal(i % 2 == 0, coils[i]);
    }
    #endregion

    #region 读写组合场景
    [Fact]
    public void ReadWrite_RegisterRoundTrip()
    {
        using var client = CreateClient();

        // 写入一系列值，然后读回
        for (UInt16 i = 0; i < 10; i++)
        {
            var val = (UInt16)(i * 1000);
            client.WriteRegister(1, i, val);
        }

        var regs = client.ReadRegister(1, 0, 10);
        for (var i = 0; i < 10; i++)
            Assert.Equal((UInt16)(i * 1000), regs[i]);
    }

    [Fact]
    public void ReadWrite_CoilRoundTrip()
    {
        using var client = CreateClient();

        // 逐个设置线圈
        for (var i = 0; i < 8; i++)
        {
            var value = (UInt16)(i % 3 == 0 ? 0xFF00 : 0x0000);
            client.WriteCoil(1, (UInt16)i, value);
        }

        var coils = client.ReadCoil(1, 0, 8);
        for (var i = 0; i < 8; i++)
            Assert.Equal(i % 3 == 0, coils[i]);
    }

    [Fact]
    public void Read_FunctionCodes_Dispatch()
    {
        using var client = CreateClient();

        // 通过Read方法使用不同的功能码读取
        var rs = client.Read(FunctionCodes.ReadRegister, 1, 0, 3);
        Assert.NotNull(rs);

        var rs2 = client.Read(FunctionCodes.ReadCoil, 1, 0, 8);
        Assert.NotNull(rs2);
    }
    #endregion

    #region ModbusSlave属性
    [Fact]
    public void Slave_DefaultPort()
    {
        var slave = new ModbusSlave();
        Assert.Equal(502, slave.Port);
        slave.Dispose();
    }

    [Fact]
    public void Slave_RegistersAndCoils()
    {
        Assert.Equal(20, _slave.Registers.Count);
        Assert.Equal(32, _slave.Coils.Count);
    }

    [Fact]
    public void Slave_EmptyRegistersAndCoils()
    {
        var slave = new ModbusSlave();
        Assert.NotNull(slave.Registers);
        Assert.NotNull(slave.Coils);
        Assert.Empty(slave.Registers);
        Assert.Empty(slave.Coils);
        slave.Dispose();
    }
    #endregion
}
