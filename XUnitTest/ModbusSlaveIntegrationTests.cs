using NewLife.IoT;
using NewLife.IoT.Models;
using NewLife.IoT.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusSlave集成测试的共享fixture，服务器仅启动一次</summary>
public class ModbusSlaveFixture : IDisposable
{
    public ModbusSlave Slave { get; }
    public Int32 Port { get; }

    public ModbusSlaveFixture()
    {
        // 使用OS分配的空闲端口，避免端口冲突
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        Slave = new ModbusSlave
        {
            Port = Port,
            Registers = Enumerable.Range(0, 20)
                .Select(i => new RegisterUnit { Address = i, Value = (UInt16)(200 + i) })
                .ToList(),
            Coils = Enumerable.Range(0, 32)
                .Select(i => new CoilUnit { Address = i, Value = (Byte)(i % 2) })
                .ToList(),
        };
        Slave.Start();
        Thread.Sleep(200);
    }

    public void Dispose() => Slave.Dispose();
}

/// <summary>ModbusSlave集成测试 - 覆盖更多场景</summary>
/// <remarks>
/// 使用IClassFixture共享服务器实例，避免每个测试重复启动/停止。
/// 读取初始值的测试使用寄存器0-9和线圈0-15（不被写测试修改）；
/// 写入测试使用寄存器10-19和线圈16-31（自包含，写后立即读回验证）。
/// </remarks>
public class ModbusSlaveIntegrationTests : IClassFixture<ModbusSlaveFixture>
{
    private readonly ModbusSlaveFixture _fixture;

    public ModbusSlaveIntegrationTests(ModbusSlaveFixture fixture)
    {
        _fixture = fixture;
    }

    private ModbusTcp CreateClient()
    {
        var client = new ModbusTcp
        {
            Server = $"tcp://localhost:{_fixture.Port}",
            Timeout = 3000,
        };
        client.Open();
        return client;
    }

    #region 读取寄存器（使用地址0-9，不被写测试修改）
    [Fact]
    public void ReadRegister_AllRegisters()
    {
        using var client = CreateClient();
        var rs = client.ReadRegister(1, 0, 10);
        Assert.NotNull(rs);
        Assert.Equal(10, rs.Length);
        for (var i = 0; i < 10; i++)
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
        var rs = client.ReadRegister(1, 9, 1);
        Assert.NotNull(rs);
        Assert.Single(rs);
        Assert.Equal((UInt16)209, rs[0]);
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

    #region 读取线圈（使用地址0-15，不被写测试修改）
    [Fact]
    public void ReadCoil_AllCoils()
    {
        using var client = CreateClient();
        var rs = client.ReadCoil(1, 0, 16);
        Assert.NotNull(rs);
        Assert.Equal(16, rs.Length);
        for (var i = 0; i < 16; i++)
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

    #region 写入寄存器（使用地址10-19，自包含写后读回）
    [Fact]
    public void WriteRegister_ThenRead()
    {
        using var client = CreateClient();
        var rs = client.WriteRegister(1, 10, 0x9999);
        Assert.Equal(0x9999, rs);

        var regs = client.ReadRegister(1, 10, 1);
        Assert.Equal(0x9999, regs[0]);
    }

    [Fact]
    public void WriteRegister_MaxValue()
    {
        using var client = CreateClient();
        var rs = client.WriteRegister(1, 11, 0xFFFF);
        Assert.Equal(0xFFFF, rs);

        var regs = client.ReadRegister(1, 11, 1);
        Assert.Equal(0xFFFF, regs[0]);
    }

    [Fact]
    public void WriteRegister_ZeroValue()
    {
        using var client = CreateClient();
        var rs = client.WriteRegister(1, 12, 0x0000);
        Assert.Equal(0, rs);

        var regs = client.ReadRegister(1, 12, 1);
        Assert.Equal(0, regs[0]);
    }

    [Fact]
    public void WriteRegisters_ThenRead()
    {
        using var client = CreateClient();
        var values = new UInt16[] { 0xAAAA, 0xBBBB, 0xCCCC, 0xDDDD };
        var rs = client.WriteRegisters(1, 13, values);
        Assert.Equal(4, rs);

        var regs = client.ReadRegister(1, 13, 4);
        Assert.Equal(0xAAAA, regs[0]);
        Assert.Equal(0xBBBB, regs[1]);
        Assert.Equal(0xCCCC, regs[2]);
        Assert.Equal(0xDDDD, regs[3]);
    }

    [Fact]
    public void WriteRegisters_SingleValue()
    {
        using var client = CreateClient();
        var rs = client.WriteRegisters(1, 17, new UInt16[] { 0x1111 });
        Assert.Equal(1, rs);

        var regs = client.ReadRegister(1, 17, 1);
        Assert.Equal(0x1111, regs[0]);
    }
    #endregion

    #region 写入线圈（使用地址16-31，自包含写后读回）
    [Fact]
    public void WriteCoil_ThenRead()
    {
        using var client = CreateClient();
        // 写入ON
        client.WriteCoil(1, 16, 0xFF00);
        var rs = client.ReadCoil(1, 16, 1);
        Assert.True(rs[0]);

        // 再关闭
        client.WriteCoil(1, 16, 0x0000);
        rs = client.ReadCoil(1, 16, 1);
        Assert.False(rs[0]);
    }

    [Fact]
    public void WriteCoils_ThenRead()
    {
        using var client = CreateClient();
        // 写入8个线圈：全开
        var values = new UInt16[] { 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00 };
        var rs = client.WriteCoils(1, 16, values);
        Assert.Equal(8, rs);

        var coils = client.ReadCoil(1, 16, 8);
        for (var i = 0; i < 8; i++)
            Assert.True(coils[i]);
    }

    [Fact]
    public void WriteCoils_AllOff_ThenRead()
    {
        using var client = CreateClient();
        // 全关
        var values = new UInt16[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        var rs = client.WriteCoils(1, 16, values);
        Assert.Equal(8, rs);

        var coils = client.ReadCoil(1, 16, 8);
        for (var i = 0; i < 8; i++)
            Assert.False(coils[i]);
    }

    [Fact]
    public void WriteCoils_NonMultipleOf8_ThenRead()
    {
        using var client = CreateClient();
        // 写5个线圈（非8的倍数）
        var values = new UInt16[] { 0xFF00, 0x0000, 0xFF00, 0x0000, 0xFF00 };
        var rs = client.WriteCoils(1, 24, values);
        Assert.Equal(5, rs);

        var coils = client.ReadCoil(1, 24, 5);
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

    #region 读写组合场景（使用写区域地址，自包含）
    [Fact]
    public void ReadWrite_RegisterRoundTrip()
    {
        using var client = CreateClient();

        // 写入一系列值，然后读回（使用寄存器10-19）
        for (UInt16 i = 0; i < 10; i++)
        {
            var val = (UInt16)(i * 1000);
            client.WriteRegister(1, (UInt16)(10 + i), val);
        }

        var regs = client.ReadRegister(1, 10, 10);
        for (var i = 0; i < 10; i++)
            Assert.Equal((UInt16)(i * 1000), regs[i]);
    }

    [Fact]
    public void ReadWrite_CoilRoundTrip()
    {
        using var client = CreateClient();

        // 逐个设置线圈（使用线圈24-31）
        for (var i = 0; i < 8; i++)
        {
            var value = (UInt16)(i % 3 == 0 ? 0xFF00 : 0x0000);
            client.WriteCoil(1, (UInt16)(24 + i), value);
        }

        var coils = client.ReadCoil(1, 24, 8);
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
        Assert.Equal(20, _fixture.Slave.Registers.Count);
        Assert.Equal(32, _fixture.Slave.Coils.Count);
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
