using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NewLife;
using NewLife.Data;
using NewLife.IoT;
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
        var p = driver.CreateParameter(null) as ModbusIpParameter;
        Rand.Fill(p);

        var node = driver.Open(null, p);

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

        var p = driver.CreateParameter(null);
        Rand.Fill(p);

        var node1 = driver.Open(null, p);
        var m1 = driver.Modbus;

        var node2 = driver.Open(null, p);
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

        var p = driver.CreateParameter(null);

        var node = driver.Open(null, p);

        // 模拟Modbus
        var mb = new Mock<Modbus>();
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 0, 10))
            .Returns((ArrayPacket)"12-34-56-78-90-12-34-56-78-90-12-34-56-78-90-12-34-56-78-90".ToHex());
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
    public void BuildSegments()
    {
        var mockModbus = new Mock<Modbus> { CallBase = true };

        var mockDriver = new Mock<ModbusDriver> { CallBase = true };
        mockDriver.Setup(e => e.CreateModbus(It.IsAny<IDevice>(), It.IsAny<ModbusNode>(), It.IsAny<ModbusParameter>()))
            .Returns(mockModbus.Object);

        var driver = mockDriver.Object;

        // 10个点位
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

        // 凑批成为一个
        var segs = driver.BuildSegments(points, new ModbusParameter());
        Assert.Equal(1, segs.Count);
        Assert.Equal(0, segs[0].Address);
        Assert.Equal(10, segs[0].Count);

        // 每4个一批，凑成3批
        segs = driver.BuildSegments(points, new ModbusParameter { BatchSize = 4 });
        Assert.Equal(3, segs.Count);
        Assert.Equal(4, segs[1].Address);
        Assert.Equal(4, segs[1].Count);
    }

    [Fact]
    public void BuildSegmentsOnCoil()
    {
        var mockModbus = new Mock<Modbus> { CallBase = true };

        var mockDriver = new Mock<ModbusDriver> { CallBase = true };
        mockDriver.Setup(e => e.CreateModbus(It.IsAny<IDevice>(), It.IsAny<ModbusNode>(), It.IsAny<ModbusParameter>()))
            .Returns(mockModbus.Object);

        var driver = mockDriver.Object;

        // 10个点位
        var points = new List<IPoint>
        {
            new PointModel { Name = "p0", Address = "0x00", },
            new PointModel { Name = "p2", Address = "0x02", },
            new PointModel { Name = "p4", Address = "0x04", },
            new PointModel { Name = "p8", Address = "0x08", },
            new PointModel { Name = "p16", Address = "0x10", },
            new PointModel { Name = "p20", Address = "0x14", }
        };

        // 凑批成为一个
        var segs = driver.BuildSegments(points, new ModbusParameter());
        Assert.Equal(1, segs.Count);
        Assert.Equal(0, segs[0].Address);
        Assert.Equal(15, segs[0].Count);

        // 每4个一批，凑成3批
        segs = driver.BuildSegments(points, new ModbusParameter { BatchSize = 4 });
        Assert.Equal(2, segs.Count);
        Assert.Equal(10, segs[1].Address);
        Assert.Equal(5, segs[1].Count);
    }

    [Fact]
    public void ReadWithBatch()
    {
        var driver = new ModbusTcpDriver();

        var p = driver.CreateParameter(null) as ModbusParameter;

        var node = driver.Open(null, p);
        p = node.Parameter as ModbusParameter;

        // 模拟Modbus
        var mb = new Mock<Modbus>();
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 0, 8))
            .Returns((ArrayPacket)"12-34-56-78-90-12-34-56-78-90-12-34-56-78-90-12".ToHex());
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 8, 2))
            .Returns((ArrayPacket)"34-56-78-90".ToHex());
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

        var p = driver.CreateParameter(null) as ModbusParameter;

        var node = driver.Open(null, p);
        p = node.Parameter as ModbusParameter;

        // 模拟Modbus
        var mb = new Mock<Modbus>();
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 0, 4))
            .Returns((ArrayPacket)"12-34-56-78-90-12-34-56".ToHex());
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 4, 4))
            .Returns((ArrayPacket)"78-90-12-34-56-78-90-12".ToHex());
        mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 8, 2))
            .Returns((ArrayPacket)"34-56-78-90".ToHex());
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

        var p = driver.CreateParameter(null) as ModbusParameter;

        var node = driver.Open(null, p);

        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        //mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 100, 1))
        //    .Returns("01-02-00".ToHex());
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, 100, 1))
            .Returns((ArrayPacket)"02-02-00".ToHex());
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, 102, 1))
            .Returns((ArrayPacket)"02-05-00".ToHex());
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

    [Fact]
    public void Write()
    {
        var mockModbus = new Mock<Modbus> { CallBase = true };
        mockModbus.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = e.Payload.Slice(0, 2).Append(e.Payload.Slice(2, 2))
            });

        var mockDriver = new Mock<ModbusDriver> { CallBase = true };
        mockDriver.Setup(e => e.CreateModbus(It.IsAny<IDevice>(), It.IsAny<ModbusNode>(), It.IsAny<ModbusParameter>()))
            .Returns(mockModbus.Object);

        var driver = mockDriver.Object;

        var node = driver.Open(null, new ModbusParameter());

        var pt = new PointModel
        {
            Name = "调节池运行时间",
            Address = "4x100",
            Type = "short",
            Length = 2
        };

        var rs = (Int32)driver.Write(node, pt, "15");
        Assert.Equal(0x000F, rs);
    }
}