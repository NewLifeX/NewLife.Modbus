using System;
using System.IO;
using NewLife;
using NewLife.Data;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

public class ModbusIpMessageTests
{
    [Fact]
    public void Test1()
    {
        var str = "00-03-00-00-00-06-01-05-00-02-FF-00";
        var dt = str.ToHex();

        var msg = ModbusIpMessage.Read(dt, false);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.False(msg.Reply);
        Assert.Equal(FunctionCodes.WriteCoil, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x02, msg.GetAddress());
        Assert.Equal(0xFF00, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0x03, msg.TransactionId);
        Assert.Equal(0, msg.ProtocolId);
        Assert.Equal("WriteCoil (0x0002, FF00)", msg.ToString());

        var pk = msg.ToPacket();
        Assert.Equal(str, pk.ToHex(256, "-"));
    }

    [Fact]
    public void Test2()
    {
        var str = "00-03-00-00-00-06-01-05-00-02-00-00";
        var dt = str.ToHex();

        var msg = ModbusIpMessage.Read(dt, true);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.True(msg.Reply);
        Assert.Equal(FunctionCodes.WriteCoil, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x02, msg.GetAddress());
        Assert.Equal(0x0000, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0x03, msg.TransactionId);
        Assert.Equal(0, msg.ProtocolId);
        Assert.Equal("WriteCoil 00020000", msg.ToString());

        var buf = new Byte[1024];
        var count = msg.Writer(buf);
        Assert.Equal(str, buf.ToHex("-", 0, count));
    }

    [Fact]
    public void CreateReply()
    {
        var msg = new ModbusIpMessage { Code = FunctionCodes.ReadRegister };
        var rs = msg.CreateReply();

        Assert.True(rs is ModbusIpMessage);
        Assert.True(rs.Reply);
        Assert.Equal(msg.Code, rs.Code);
    }

    [Fact]
    public void Set()
    {
        var msg = new ModbusIpMessage { Code = FunctionCodes.WriteRegister };

        msg.SetRequest(0x0002, 0xABCD);

        Assert.Equal("00-02-AB-CD", msg.Payload.ToHex(256, "-"));
    }
}