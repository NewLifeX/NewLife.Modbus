using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NewLife;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.IoT.ThingModels;
using NewLife.Security;
using Xunit;

namespace XUnitTest;

public class ModbusDriverTests
{
    [Fact]
    public void OpenTest()
    {
        var driver = new ModbusTcpDriver();

        //var p = new ModbusTcpParameter
        //{
        //    Host = 3,
        //    ReadCode = FunctionCodes.ReadRegister,
        //    WriteCode = FunctionCodes.WriteRegister,
        //    Server = "tcp://localhost:502",

        //    Timeout = Rand.Next(),
        //    BatchSize = Rand.Next(),
        //};
        var p = new ModbusIpParameter();
        Rand.Fill(p);
        var dic = p.ToDictionary();

        var node = driver.Open(null, dic);

        var node2 = node as ModbusNode;
        Assert.NotNull(node2);

        Assert.Equal(p.Host, node2.Host);
        Assert.Equal(p.ReadCode, node2.ReadCode);
        Assert.Equal(p.WriteCode, node2.WriteCode);
        Assert.Null(node2.Device);

        var modbus = driver.Modbus as ModbusTcp;
        Assert.NotNull(modbus);
        Assert.Equal(p.Server, modbus.Server);
        Assert.Equal(p.Timeout, modbus.Timeout);
        Assert.Equal(256, modbus.BufferSize);
        //Assert.Equal(p.BatchSize, modbus.BatchSize);
        //Assert.Equal(p.Delay, modbus.Delay);
    }

    [Fact]
    public void CloseTest()
    {
        var driver = new ModbusTcpDriver();

        var p = new ModbusIpParameter();
        Rand.Fill(p);
        var dic = p.ToDictionary();

        var node1 = driver.Open(null, dic);
        var m1 = driver.Modbus;

        var node2 = driver.Open(null, dic);
        var m2 = driver.Modbus;
        Assert.NotNull(driver.Modbus);
        Assert.Equal(m1, m2);
        Assert.Equal(m1, driver.Modbus);

        driver.Close(node1);
        Assert.NotNull(driver.Modbus);

        driver.Close(node2);
        Assert.Null(driver.Modbus);
    }

    [Fact]
    public void ReadTest()
    {
        var driver = new ModbusTcpDriver();

        var p = driver.GetDefaultParameter() as ModbusParameter;
        var dic = p.ToDictionary();

        var node = driver.Open(null, dic);

        // 模拟Modbus
        var mb = new Mock<Modbus>();
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 0, 10))
            .Returns("12-34-56-78-90-12-34-56-78-90-12-34-56-78-90-12-34-56-78-90".ToHex());
        driver.Modbus = mb.Object;

        var points = new List<IPoint>();
        for (var i = 0; i < 10; i++)
        {
            var pt = new PointModel
            {
                Name = "p" + i,
                Address = i + "",
                Length = 2
            };

            points.Add(pt);
        }

        // 读取
        var rs = driver.Read(node, points.ToArray());
        Assert.NotNull(rs);
        Assert.Equal(10, rs.Count);

        for (var i = 0; i < 10; i++)
        {
            var name = "p" + i;
            Assert.True(rs.ContainsKey(name));
        }
    }

    [Fact]
    public void ReadWithBatch()
    {
        var driver = new ModbusTcpDriver();

        var p = driver.GetDefaultParameter() as ModbusParameter;
        var dic = p.ToDictionary();

        var node = driver.Open(null, dic);
        p = node.Parameter as ModbusParameter;

        // 模拟Modbus
        var mb = new Mock<Modbus>();
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 0, 8))
            .Returns("12-34-56-78-90-12-34-56-78-90-12-34-56-78-90-12".ToHex());
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 8, 2))
            .Returns("34-56-78-90".ToHex());
        driver.Modbus = mb.Object;

        var points = new List<IPoint>();
        for (var i = 0; i < 10; i++)
        {
            var pt = new PointModel
            {
                Name = "p" + i,
                Address = i + "",
                Length = 2
            };

            points.Add(pt);
        }

        // 打断
        p.BatchSize = 8;

        // 读取
        var rs = driver.Read(node, points.ToArray());
        Assert.NotNull(rs);
        Assert.Equal(10, rs.Count);

        for (var i = 0; i < 10; i++)
        {
            var name = "p" + i;
            Assert.True(rs.ContainsKey(name));
        }
    }

    [Fact]
    public void ReadWithBatch2()
    {
        var driver = new ModbusTcpDriver();

        var p = driver.GetDefaultParameter() as ModbusParameter;
        var dic = p.ToDictionary();

        var node = driver.Open(null, dic);
        p = node.Parameter as ModbusParameter;

        // 模拟Modbus
        var mb = new Mock<Modbus>();
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 0, 4))
            .Returns("12-34-56-78-90-12-34-56".ToHex());
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 4, 4))
            .Returns("78-90-12-34-56-78-90-12".ToHex());
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 8, 2))
            .Returns("34-56-78-90".ToHex());
        driver.Modbus = mb.Object;

        var points = new List<IPoint>();
        for (var i = 0; i < 10; i++)
        {
            var pt = new PointModel
            {
                Name = "p" + i,
                Address = i + "",
                Length = 2
            };

            points.Add(pt);
        }

        // 打断
        p.BatchSize = 4;

        // 读取
        var rs = driver.Read(node, points.ToArray());
        Assert.NotNull(rs);
        Assert.Equal(10, rs.Count);

        for (var i = 0; i < 10; i++)
        {
            var name = "p" + i;
            Assert.True(rs.ContainsKey(name));
        }
    }

    [Fact]
    public void ReadRegister()
    {
        var driver = new ModbusTcpDriver();

        var p = driver.GetDefaultParameter() as ModbusParameter;

        var node = driver.Open(null, p);

        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        //mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 100, 1))
        //    .Returns("01-02-00".ToHex());
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, 100, 1))
            .Returns("02-02-00".ToHex());
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, 102, 1))
            .Returns("02-05-00".ToHex());
        driver.Modbus = mb.Object;

        var points = new List<IPoint>
        {
            new PointModel
            {
                Name = "调节池运行时间",
                Address = "4x100",
                Length = 2
            },
            new PointModel
            {
                Name = "调节池停止时间",
                Address = "4x102",
                Length = 2
            }
        };

        // 读取
        var rs = driver.Read(node, points.ToArray());
        Assert.NotNull(rs);
        Assert.Equal(2, rs.Count);

        Assert.Equal(0x0200, (rs["调节池运行时间"] as Byte[]).ToUInt16(0, false));
        Assert.Equal(0x0500, (rs["调节池停止时间"] as Byte[]).ToUInt16(0, false));
    }
}