using System.IO;
using NewLife;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

public class ModbusAsciiMessageTests
{
    [Fact]
    public void Test1()
    {
        var str = "01-05-00-02-FF-00-2D-FA";
        var dt = str.ToHex();

        var msg = ModbusAsciiMessage.Read(dt, false);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.False(msg.Reply);
        Assert.Equal(FunctionCodes.WriteCoil, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x02, msg.GetAddress());
        Assert.Equal(0xFF00, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0xFA2D, msg.Lrc);
        Assert.Equal(msg.Lrc, msg.Lrc2);
        Assert.Equal("WriteCoil (0x0002, FF00)", msg.ToString());

        var pk = msg.ToPacket();
        Assert.Equal(str, pk.ToHex(256, "-"));
    }

    [Fact]
    public void Test2()
    {
        var str = "01-05-00-02-00-00-6C-0A";
        var dt = str.ToHex();

        var msg = ModbusAsciiMessage.Read(dt, true);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.True(msg.Reply);
        Assert.Equal(FunctionCodes.WriteCoil, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x02, msg.GetAddress());
        Assert.Equal(0x0000, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0x0A6C, msg.Lrc);
        Assert.Equal("WriteCoil 00020000", msg.ToString());

        var ms = new MemoryStream();
        msg.Write(ms, null);
        Assert.Equal(str, ms.ToArray().ToHex("-"));
    }

    [Fact]
    public void CreateReply()
    {
        var msg = new ModbusAsciiMessage { Code = FunctionCodes.ReadRegister };
        var rs = msg.CreateReply();

        Assert.True(rs is ModbusAsciiMessage);
        Assert.True(rs.Reply);
        Assert.Equal(msg.Code, rs.Code);
    }

    [Fact]
    public void Set()
    {
        var msg = new ModbusAsciiMessage { Code = FunctionCodes.WriteRegister };

        msg.Set(0x0002, 0xABCD);

        Assert.Equal("00-02-AB-CD", msg.Payload.ToHex(256, "-"));
    }
}