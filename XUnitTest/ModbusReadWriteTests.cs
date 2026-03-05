using System;
using Moq;
using NewLife;
using NewLife.Data;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

/// <summary>Modbus读写操作的补充单元测试，覆盖未测试的代码路径</summary>
public class ModbusReadWriteTests
{
    #region Read - 空/null响应
    [Fact]
    public void Read_NullResponse_ReturnsNull()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 1))
            .Returns((IPacket)null);

        var modbus = mb.Object;
        var rs = modbus.Read(FunctionCodes.ReadRegister, 1, 100, 1);
        Assert.Null(rs);
    }

    [Fact]
    public void Read_ReadCoil_NullResponse_ReturnsNull()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 1))
            .Returns((IPacket)null);

        var modbus = mb.Object;
        var rs = modbus.Read(FunctionCodes.ReadCoil, 1, 100, 1);
        Assert.Null(rs);
    }

    [Fact]
    public void Read_ReadDiscrete_Works()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadDiscrete, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"01-03".ToHex());

        var modbus = mb.Object;
        var rs = modbus.Read(FunctionCodes.ReadDiscrete, 1, 100, 1);
        Assert.NotNull(rs);
    }

    [Fact]
    public void Read_ReadInput_Works()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadInput, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"02-00-64".ToHex());

        var modbus = mb.Object;
        var rs = modbus.Read(FunctionCodes.ReadInput, 1, 100, 1);
        Assert.NotNull(rs);
    }
    #endregion

    #region Read - ValidResponse=false
    [Fact]
    public void Read_ValidResponseFalse_SkipsLengthCheck()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        // 返回不完整的数据，但关闭校验
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"02-00".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = false;

        var rs = modbus.Read(FunctionCodes.ReadRegister, 1, 100, 1);
        Assert.NotNull(rs);
    }

    [Fact]
    public void Read_ValidResponseTrue_InsufficientData_ReturnsNull()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        // len=0x04 但只有2字节数据，Total < 1+len
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"04-00".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = true;

        var rs = modbus.Read(FunctionCodes.ReadRegister, 1, 100, 1);
        Assert.Null(rs);
    }
    #endregion

    #region Read - 不支持的功能码
    [Fact]
    public void Read_UnsupportedCode_Throws()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        var modbus = mb.Object;

        Assert.Throws<NotSupportedException>(() => modbus.Read(FunctionCodes.Diagnostics, 1, 100, 1));
    }

    [Fact]
    public void Read_WriteCoilCode_Throws()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        var modbus = mb.Object;

        Assert.Throws<NotSupportedException>(() => modbus.Read(FunctionCodes.WriteCoil, 1, 100, 1));
    }
    #endregion

    #region ReadCoil - 边界情况
    [Fact]
    public void ReadCoil_NullResponse_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 8))
            .Returns((IPacket)null);

        var modbus = mb.Object;
        var rs = modbus.ReadCoil(1, 0, 8);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadCoil_ValidResponse_InsufficientData_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        // 请求16个线圈需要2字节数据，但只返回1字节
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 16))
            .Returns((ArrayPacket)"01".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = true;
        var rs = modbus.ReadCoil(1, 0, 16);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadCoil_NonMultipleOf8()
    {
        // 请求10个线圈（非8的倍数），需要2字节
        var mb = new Mock<Modbus>() { CallBase = true };
        // 返回: count_byte=02, data: 0xFF, 0x03 (前8位全开，后2位全开)
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 10))
            .Returns((ArrayPacket)"02-FF-03".ToHex());

        var modbus = mb.Object;
        var rs = modbus.ReadCoil(1, 0, 10);
        Assert.Equal(10, rs.Length);
        for (var i = 0; i < 10; i++)
            Assert.True(rs[i]);
    }

    [Fact]
    public void ReadCoil_ValidResponseFalse_SkipsLengthCheck()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 8))
            .Returns((ArrayPacket)"01-AA".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = false;
        var rs = modbus.ReadCoil(1, 0, 8);
        Assert.Equal(8, rs.Length);
    }

    [Fact]
    public void ReadCoil_SingleCoil()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"01-01".ToHex());

        var modbus = mb.Object;
        var rs = modbus.ReadCoil(1, 0, 1);
        Assert.Single(rs);
        Assert.True(rs[0]);
    }

    [Fact]
    public void ReadCoil_AllOff()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadCoil, 1, It.IsAny<UInt16>(), 8))
            .Returns((ArrayPacket)"01-00".ToHex());

        var modbus = mb.Object;
        var rs = modbus.ReadCoil(1, 0, 8);
        Assert.Equal(8, rs.Length);
        for (var i = 0; i < 8; i++)
            Assert.False(rs[i]);
    }
    #endregion

    #region ReadDiscrete - 边界情况
    [Fact]
    public void ReadDiscrete_NullResponse_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadDiscrete, 1, It.IsAny<UInt16>(), 8))
            .Returns((IPacket)null);

        var modbus = mb.Object;
        var rs = modbus.ReadDiscrete(1, 0, 8);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadDiscrete_ValidResponse_InsufficientData_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        // 请求16个离散量需要2字节数据，但只返回1字节
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadDiscrete, 1, It.IsAny<UInt16>(), 16))
            .Returns((ArrayPacket)"01".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = true;
        var rs = modbus.ReadDiscrete(1, 0, 16);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadDiscrete_NonMultipleOf8()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadDiscrete, 1, It.IsAny<UInt16>(), 3))
            .Returns((ArrayPacket)"01-05".ToHex());

        var modbus = mb.Object;
        var rs = modbus.ReadDiscrete(1, 0, 3);
        Assert.Equal(3, rs.Length);
        Assert.True(rs[0]);   // bit0 = 1
        Assert.False(rs[1]);  // bit1 = 0
        Assert.True(rs[2]);   // bit2 = 1
    }
    #endregion

    #region ReadRegister - 边界情况
    [Fact]
    public void ReadRegister_NullResponse_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 2))
            .Returns((IPacket)null);

        var modbus = mb.Object;
        var rs = modbus.ReadRegister(1, 0, 2);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadRegister_ValidResponse_InsufficientData_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        // len=04，但只有2字节数据
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 2))
            .Returns((ArrayPacket)"04-00-01".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = true;
        var rs = modbus.ReadRegister(1, 0, 2);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadRegister_SingleRegister()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadRegister, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"02-AB-CD".ToHex());

        var modbus = mb.Object;
        var rs = modbus.ReadRegister(1, 100, 1);
        Assert.Single(rs);
        Assert.Equal(0xABCD, rs[0]);
    }
    #endregion

    #region ReadInput - 边界情况
    [Fact]
    public void ReadInput_NullResponse_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadInput, 1, It.IsAny<UInt16>(), 2))
            .Returns((IPacket)null);

        var modbus = mb.Object;
        var rs = modbus.ReadInput(1, 0, 2);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadInput_ValidResponse_InsufficientData_ReturnsEmpty()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadInput, 1, It.IsAny<UInt16>(), 2))
            .Returns((ArrayPacket)"04-00-01".ToHex());

        var modbus = mb.Object;
        modbus.ValidResponse = true;
        var rs = modbus.ReadInput(1, 0, 2);
        Assert.Empty(rs);
    }

    [Fact]
    public void ReadInput_SingleRegister()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(FunctionCodes.ReadInput, 1, It.IsAny<UInt16>(), 1))
            .Returns((ArrayPacket)"02-12-34".ToHex());

        var modbus = mb.Object;
        var rs = modbus.ReadInput(1, 100, 1);
        Assert.Single(rs);
        Assert.Equal(0x1234, rs[0]);
    }
    #endregion

    #region WriteCoil - 边界情况
    [Fact]
    public void WriteCoil_NullResponse_ReturnsMinus1()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns((ModbusMessage)null);

        var modbus = mb.Object;
        var rs = modbus.WriteCoil(1, 100, 0xFF00);
        Assert.Equal(-1, rs);
    }

    [Fact]
    public void WriteCoil_ShortResponse_ReturnsMinus1()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = (ArrayPacket)"00-01".ToHex()
            });

        var modbus = mb.Object;
        var rs = modbus.WriteCoil(1, 100, 0xFF00);
        Assert.Equal(-1, rs);
    }

    [Fact]
    public void WriteCoil_WriteOff()
    {
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
        var rs = modbus.WriteCoil(1, 100, 0x0000);
        Assert.Equal(0x0000, rs);
    }
    #endregion

    #region WriteRegister - 边界情况
    [Fact]
    public void WriteRegister_NullResponse_ReturnsMinus1()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns((ModbusMessage)null);

        var modbus = mb.Object;
        var rs = modbus.WriteRegister(1, 100, 0x1234);
        Assert.Equal(-1, rs);
    }

    [Fact]
    public void WriteRegister_ShortResponse_ReturnsMinus1()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = (ArrayPacket)"00".ToHex()
            });

        var modbus = mb.Object;
        var rs = modbus.WriteRegister(1, 100, 0x1234);
        Assert.Equal(-1, rs);
    }

    [Fact]
    public void WriteRegister_ZeroValue()
    {
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
        var rs = modbus.WriteRegister(1, 100, 0x0000);
        Assert.Equal(0, rs);
    }

    [Fact]
    public void WriteRegister_MaxValue()
    {
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
        var rs = modbus.WriteRegister(1, 100, 0xFFFF);
        Assert.Equal(0xFFFF, rs);
    }
    #endregion

    #region WriteCoils - 边界情况
    [Fact]
    public void WriteCoils_NullResponse_ReturnsMinus1()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns((ModbusMessage)null);

        var modbus = mb.Object;
        var rs = modbus.WriteCoils(1, 0, new UInt16[] { 0xFF00 });
        Assert.Equal(-1, rs);
    }

    [Fact]
    public void WriteCoils_SingleCoil()
    {
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
        var rs = modbus.WriteCoils(1, 100, new UInt16[] { 0xFF00 });
        Assert.Equal(1, rs);
    }

    [Fact]
    public void WriteCoils_NonMultipleOf8()
    {
        // 10个线圈（非8的倍数），测试余数位的打包
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
        var values = new UInt16[] { 0xFF00, 0, 0xFF00, 0, 0xFF00, 0, 0xFF00, 0, 0xFF00, 0xFF00 };
        var rs = modbus.WriteCoils(1, 0, values);
        Assert.Equal(10, rs);
    }

    [Fact]
    public void WriteCoils_AllOff()
    {
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
        var rs = modbus.WriteCoils(1, 0, new UInt16[] { 0, 0, 0, 0 });
        Assert.Equal(4, rs);
    }
    #endregion

    #region WriteRegisters - 边界情况
    [Fact]
    public void WriteRegisters_NullResponse_ReturnsMinus1()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns((ModbusMessage)null);

        var modbus = mb.Object;
        var rs = modbus.WriteRegisters(1, 0, new UInt16[] { 1 });
        Assert.Equal(-1, rs);
    }

    [Fact]
    public void WriteRegisters_SingleRegister()
    {
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
        var rs = modbus.WriteRegisters(1, 100, new UInt16[] { 0xABCD });
        Assert.Equal(1, rs);
    }
    #endregion

    #region Write - 派发不支持的功能码
    [Fact]
    public void Write_UnsupportedCode_Throws()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        var modbus = mb.Object;

        Assert.Throws<NotSupportedException>(() => modbus.Write(FunctionCodes.ReadRegister, 1, 100, new UInt16[] { 1 }));
    }

    [Fact]
    public void Write_Diagnostics_Throws()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        var modbus = mb.Object;

        Assert.Throws<NotSupportedException>(() => modbus.Write(FunctionCodes.Diagnostics, 1, 100, new UInt16[] { 1 }));
    }

    [Fact]
    public void Write_WriteCoil_Dispatches()
    {
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
        var rs = (Int32)modbus.Write(FunctionCodes.WriteCoil, 1, 100, new UInt16[] { 0xFF00 });
        Assert.Equal(0xFF00, rs);
    }

    [Fact]
    public void Write_WriteCoils_Dispatches()
    {
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
        var rs = (Int32)modbus.Write(FunctionCodes.WriteCoils, 1, 0, new UInt16[] { 0xFF00, 0, 0xFF00 });
        Assert.Equal(3, rs);
    }

    [Fact]
    public void Write_WriteRegisters_Dispatches()
    {
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
        var rs = (Int32)modbus.Write(FunctionCodes.WriteRegisters, 1, 100, new UInt16[] { 1, 2 });
        Assert.Equal(2, rs);
    }
    #endregion

    #region SendCommand 重载
    [Fact]
    public void SendCommand_CodeHostData_Works()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns<ModbusMessage>(e => new ModbusMessage
            {
                Reply = true,
                Host = e.Host,
                Code = e.Code,
                Payload = (ArrayPacket)"01-02-03-04".ToHex()
            });

        var modbus = mb.Object;
        var pk = (ArrayPacket)"AA-BB".ToHex();
        var rs = modbus.SendCommand(FunctionCodes.WriteCoils, 1, pk);
        Assert.NotNull(rs);
        Assert.Equal("01-02-03-04", rs.ToHex(256, "-"));
    }

    [Fact]
    public void SendCommand_CodeHostData_NullResponse()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        mb.Setup(e => e.SendCommand(It.IsAny<ModbusMessage>()))
            .Returns((ModbusMessage)null);

        var modbus = mb.Object;
        var pk = (ArrayPacket)"AA-BB".ToHex();
        var rs = modbus.SendCommand(FunctionCodes.WriteCoils, 1, pk);
        Assert.Null(rs);
    }
    #endregion

    #region 属性测试
    [Fact]
    public void Properties_DefaultValues()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        var modbus = mb.Object;

        Assert.Equal(3000, modbus.Timeout);
        Assert.Equal(256, modbus.BufferSize);
        Assert.True(modbus.ValidResponse);
        Assert.Null(modbus.Tracer);
        Assert.Null(modbus.Log);
    }

    [Fact]
    public void Properties_CanSet()
    {
        var mb = new Mock<Modbus>() { CallBase = true };
        var modbus = mb.Object;

        modbus.Timeout = 5000;
        modbus.BufferSize = 512;
        modbus.ValidResponse = false;

        Assert.Equal(5000, modbus.Timeout);
        Assert.Equal(512, modbus.BufferSize);
        Assert.False(modbus.ValidResponse);
    }
    #endregion
}
