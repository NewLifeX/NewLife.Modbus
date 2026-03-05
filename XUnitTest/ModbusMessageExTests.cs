using System;
using System.IO;
using NewLife;
using NewLife.Data;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusMessage的补充单元测试</summary>
public class ModbusMessageExTests
{
    #region Read - 异常码
    [Fact]
    public void Read_ErrorResponse()
    {
        // 错误码响应：host=01, code=0x83 (0x80|0x03), error=02
        var data = "01-83-02".ToHex();

        var msg = new ModbusMessage();
        var rs = msg.Read(data);
        Assert.Equal(3, rs);
        Assert.Equal(1, msg.Host);
        Assert.Equal(FunctionCodes.ReadRegister, msg.Code);
        Assert.Equal(ErrorCodes.IllegalDataAddress, msg.ErrorCode);
        Assert.Null(msg.Payload);
    }

    [Fact]
    public void Read_ErrorResponse_IllegalFunction()
    {
        var data = "01-81-01".ToHex();

        var msg = new ModbusMessage();
        var rs = msg.Read(data);
        Assert.Equal(3, rs);
        Assert.Equal(FunctionCodes.ReadCoil, msg.Code);
        Assert.Equal(ErrorCodes.IllegalFunction, msg.ErrorCode);
    }

    [Fact]
    public void Read_ErrorResponse_SlaveDeviceFailure()
    {
        var data = "01-84-04".ToHex();

        var msg = new ModbusMessage();
        var rs = msg.Read(data);
        Assert.Equal(3, rs);
        Assert.Equal(FunctionCodes.ReadInput, msg.Code);
        Assert.Equal(ErrorCodes.SlaveDeviceFailure, msg.ErrorCode);
    }
    #endregion

    #region Write - 异常码
    [Fact]
    public void Write_ErrorResponse()
    {
        var msg = new ModbusMessage
        {
            Host = 1,
            Code = FunctionCodes.ReadRegister,
            ErrorCode = ErrorCodes.IllegalDataAddress
        };

        var pk = msg.ToPacket();
        Assert.Equal("01-83-02", pk.ToHex(256, "-"));
    }

    [Fact]
    public void Write_ErrorResponse_RoundTrip()
    {
        // 写入再读取
        var msg1 = new ModbusMessage
        {
            Host = 2,
            Code = FunctionCodes.WriteCoil,
            ErrorCode = ErrorCodes.IllegalDataValue
        };

        var pk = msg1.ToPacket();
        var msg2 = new ModbusMessage();
        msg2.Read(pk.GetSpan());

        Assert.Equal(msg1.Host, msg2.Host);
        Assert.Equal(msg1.Code, msg2.Code);
        Assert.Equal(msg1.ErrorCode, msg2.ErrorCode);
    }
    #endregion

    #region ToString 覆盖
    [Fact]
    public void ToString_Request_WithPayload()
    {
        var msg = new ModbusMessage { Code = FunctionCodes.WriteRegister };
        msg.SetRequest(0x0064, 0x1234);

        var str = msg.ToString();
        Assert.Contains("WriteRegister", str);
        Assert.Contains("0x0064", str);
    }

    [Fact]
    public void ToString_Response_WithPayload()
    {
        var msg = new ModbusMessage
        {
            Reply = true,
            Code = FunctionCodes.ReadRegister,
            Payload = (ArrayPacket)"04-12-34-56-78".ToHex()
        };

        var str = msg.ToString();
        Assert.Contains("ReadRegister", str);
        Assert.Contains("04123456", str.Replace("-", "").Replace(" ", ""));
    }

    [Fact]
    public void ToString_NoPayload()
    {
        var msg = new ModbusMessage { Code = FunctionCodes.ReadCoil };
        var str = msg.ToString();
        Assert.Contains("ReadCoil", str);
    }

    [Fact]
    public void ToString_Request_HighFunctionCode()
    {
        // Code > WriteRegisters (16) won't show address format in request ToString
        var msg = new ModbusMessage
        {
            Code = FunctionCodes.ReadWriteMultipleRegisters,
            Payload = (ArrayPacket)"00-01-00-02".ToHex()
        };

        var str = msg.ToString();
        Assert.Contains("ReadWriteMultipleRegisters", str);
    }
    #endregion

    #region CreateReply
    [Fact]
    public void CreateReply_FromReply_Throws()
    {
        var msg = new ModbusMessage { Reply = true, Code = FunctionCodes.ReadRegister };
        Assert.Throws<InvalidOperationException>(() => msg.CreateReply());
    }

    [Fact]
    public void CreateReply_CopiesHostAndCode()
    {
        var msg = new ModbusMessage
        {
            Host = 5,
            Code = FunctionCodes.WriteCoil,
        };

        var reply = msg.CreateReply();
        Assert.True(reply.Reply);
        Assert.Equal(5, reply.Host);
        Assert.Equal(FunctionCodes.WriteCoil, reply.Code);
        Assert.Null(reply.Payload);
    }
    #endregion

    #region GetRequest / SetRequest
    [Fact]
    public void GetRequest_NoPayload_Throws()
    {
        var msg = new ModbusMessage();
        Assert.Throws<InvalidDataException>(() => msg.GetRequest());
    }

    [Fact]
    public void GetRequest_ShortPayload_Throws()
    {
        var msg = new ModbusMessage
        {
            Payload = (ArrayPacket)"00-01".ToHex()
        };
        Assert.Throws<InvalidDataException>(() => msg.GetRequest());
    }

    [Fact]
    public void SetRequest_AddressAndData()
    {
        var msg = new ModbusMessage { Code = FunctionCodes.WriteRegisters };
        var data = (ArrayPacket)"12-34-56-78".ToHex();
        msg.SetRequest(0x0064, data);

        Assert.Equal(0x0064, msg.GetAddress());
    }

    [Fact]
    public void GetAddress_NoPayload_ReturnsZero()
    {
        var msg = new ModbusMessage();
        Assert.Equal(0, msg.GetAddress());
    }

    [Fact]
    public void SetRequest_RoundTrip()
    {
        var msg = new ModbusMessage { Code = FunctionCodes.ReadRegister };
        msg.SetRequest(0x1234, 0x0005);

        var (addr, count) = msg.GetRequest();
        Assert.Equal(0x1234, addr);
        Assert.Equal(5, count);
    }
    #endregion

    #region ToPacket / Writer
    [Fact]
    public void ToPacket_NoPayload()
    {
        var msg = new ModbusMessage
        {
            Host = 1,
            Code = FunctionCodes.ReadCoil,
        };

        var pk = msg.ToPacket();
        Assert.NotNull(pk);
        Assert.Equal("01-01", pk.ToHex(256, "-"));
    }

    [Fact]
    public void Writer_ToBuffer()
    {
        var msg = new ModbusMessage
        {
            Host = 1,
            Code = FunctionCodes.WriteRegister,
        };
        msg.SetRequest(0x0002, 0xABCD);

        var buf = new Byte[256];
        var len = msg.Writer(buf);
        Assert.Equal(6, len);
        Assert.Equal("01-06-00-02-AB-CD", buf.ToHex("-", 0, len));
    }
    #endregion

    #region Read - SpanReader 路径
    [Fact]
    public void Read_EmptyPayload()
    {
        // 只有host和code，没有payload
        var data = "01-03".ToHex();
        var msg = new ModbusMessage();
        var rs = msg.Read(data);
        Assert.Equal(2, rs);
        Assert.Equal(1, msg.Host);
        Assert.Equal(FunctionCodes.ReadRegister, msg.Code);
        Assert.Null(msg.Payload);
    }

    [Fact]
    public void Read_ViaSpanReader()
    {
        var data = "01-03-04-12-34-56-78".ToHex();
        var msg = new ModbusMessage();

        var reader = new NewLife.Buffers.SpanReader(data) { IsLittleEndian = false };
        var ok = msg.Read(ref reader);
        Assert.True(ok);
        Assert.Equal(1, msg.Host);
        Assert.Equal(FunctionCodes.ReadRegister, msg.Code);
        Assert.NotNull(msg.Payload);
    }
    #endregion
}
