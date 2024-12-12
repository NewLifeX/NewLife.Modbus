using System;
using Moq;
using NewLife;
using NewLife.Data;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

public class ModbusTests
{
    [Fact]
    public void Read()
    {
        // 模拟Modbus。CallBase 指定调用基类方法
        var mb = new Mock<Modbus>() { CallBase = true };
        //mb.Setup(e => e.Read(FunctionCodes.ReadRegister, 1, 100, 1))
        //    .Returns("01-02-00".ToHex());
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"02-02-00".ToHex());
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 2))
            .Returns((ArrayPacket)"04-01-02-03-04".ToHex());

        var modbus = mb.Object;

        Assert.Equal("ModbusProxy", modbus.Name);

        Assert.Throws<NotSupportedException>(() => modbus.Read(FunctionCodes.ReadWriteMultipleRegisters, 1, 1, 1));

        // 读取
        var rs = modbus.Read(FunctionCodes.ReadRegister, 1, 100, 1) as IPacket;
        Assert.NotNull(rs);
        Assert.Equal(0x0200, rs.ReadBytes().ToUInt16(0, false));

        rs = modbus.Read(FunctionCodes.ReadRegister, 1, 102, 2) as IPacket;
        Assert.NotNull(rs);
        Assert.Equal(0x01020304u, rs.ReadBytes().ToUInt32(0, false));
    }

    [Fact]
    public void ReadCoil()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, 100, 2))
            .Returns((ArrayPacket)"02-12-34-56-78".ToHex());

        var modbus = mb.Object;

        // 读取
        var rs = modbus.ReadCoil(1, 100, 2);
        Assert.NotNull(rs);

        Assert.Equal(2, rs.Length);
        Assert.True(rs[0]);
    }

    [Fact]
    public void ReadDiscrete()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadDiscrete, 1, 100, 2))
            .Returns((ArrayPacket)"02-12-34-56-78".ToHex());

        var modbus = mb.Object;

        // 读取
        var rs = modbus.ReadDiscrete(1, 100, 2);
        Assert.NotNull(rs);

        Assert.Equal(2, rs.Length);
        Assert.True(rs[0]);
    }

    [Fact]
    public void ReadRegister()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, 100, 2))
            .Returns((ArrayPacket)"04-12-34-56-78".ToHex());

        var modbus = mb.Object;

        // 读取
        var rs = modbus.ReadRegister(1, 100, 2);
        Assert.NotNull(rs);

        Assert.Equal(2, rs.Length);
        Assert.Equal(0x1234, rs[0]);
        Assert.Equal(0x5678, rs[1]);
    }

    [Fact]
    public void ReadInput()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadInput, 1, 100, 2))
            .Returns((ArrayPacket)"04-12-34-56-78".ToHex());

        var modbus = mb.Object;

        // 读取
        var rs = modbus.ReadInput(1, 100, 2);
        Assert.NotNull(rs);

        Assert.Equal(2, rs.Length);
        Assert.Equal(0x1234, rs[0]);
        Assert.Equal(0x5678, rs[1]);
    }

    [Fact]
    public void Write()
    {
        // 模拟Modbus。CallBase 指定调用基类方法
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = e.Payload.Slice(0, 2).Append("03-04".ToHex())
            });

        var modbus = mb.Object;

        Assert.Equal("ModbusProxy", modbus.Name);

        Assert.Throws<NotSupportedException>(() => modbus.Write(FunctionCodes.ReadWriteMultipleRegisters, 1, 1, new UInt16[] { 1 }));

        // 读取
        var rs = (Int32)modbus.Write(FunctionCodes.WriteRegister, 1, 100, new UInt16[] { 1 });
        Assert.NotEqual(-1, rs);
        Assert.Equal(0x0304, rs);
    }

    [Fact]
    public void WriteCoil()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = e.Payload.Slice(0, 4)
            });

        var modbus = mb.Object;

        // 读取
        var rs = modbus.WriteCoil(1, 100, 0xFF00);
        Assert.Equal(0xFF00, rs);
    }

    [Fact]
    public void WriteRegister()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = e.Payload.Slice(0, 2).Append(e.Payload.Slice(2, 2))
            });

        var modbus = mb.Object;

        // 读取
        var rs = modbus.WriteRegister(1, 100, 0x1234);
        Assert.Equal(0x1234, rs);
    }

    [Fact]
    public void WriteCoils()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = e.Payload.Slice(0, 2).Append(e.Payload.Slice(2, 2))
            });

        var modbus = mb.Object;

        // 读取
        var rs = modbus.WriteCoils(1, 100, new UInt16[] { 0xFF00, 0x0000, 0xFF00, 0x0000 });
        Assert.Equal(4, rs);
    }

    [Fact]
    public void WriteRegisters()
    {
        // 模拟Modbus
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = e.Payload.Slice(0, 2).Append(e.Payload.Slice(2, 2))
            });

        var modbus = mb.Object;

        // 读取
        var rs = modbus.WriteRegisters(1, 100, new UInt16[] { 2, 3, 4, 5, 7 });
        Assert.Equal(5, rs);
    }
}