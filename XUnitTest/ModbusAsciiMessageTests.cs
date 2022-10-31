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
        // :010600010017
        var str = "3A 30 31 30 36 30 30 30 31 30 30 31 37 45 31 0D 0A";
        var dt = str.ToHex();

        var msg = ModbusAsciiMessage.Read(dt, false);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.False(msg.Reply);
        Assert.Equal(FunctionCodes.WriteRegister, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x01, msg.GetAddress());
        Assert.Equal(0x0017, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0xE1, msg.Lrc);
        Assert.Equal(msg.Lrc, msg.Lrc2);
        Assert.Equal("WriteRegister (0x0001, 0017)", msg.ToString());

        var pk = msg.ToPacket();
        Assert.Equal(dt.ToHex("-"), pk.ToHex(256, "-"));
    }

    [Fact]
    public void Test2()
    {
        // :01030121
        var str = "3A 30 31 30 33 30 31 32 31 44 41 0D 0A";
        var dt = str.ToHex();

        var msg = ModbusAsciiMessage.Read(dt, true);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.True(msg.Reply);
        Assert.Equal(FunctionCodes.ReadRegister, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x0121, msg.GetAddress());
        //Assert.Equal(0x0000, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0xDA, msg.Lrc);
        Assert.Equal(msg.Lrc, msg.Lrc2);
        Assert.Equal("ReadRegister 0121", msg.ToString());

        var ms = new MemoryStream();
        msg.Write(ms, null);
        Assert.Equal(dt.ToHex("-"), ms.ToArray().ToHex("-"));
    }

    [Fact]
    public void Test3()
    {
        // :11-03-006B-0003-7E
        var str = "3A 3131 3033 3030 3642 3030 3033 3745 0D 0A";
        var dt = str.ToHex();

        //var vd = ModbusAsciiMessage.Decode(dt);
        var vd = dt.ToStr();
        var dt2 = vd.Substring(1, vd.Length - 3).ToHex();

        var msg = ModbusAsciiMessage.Read(dt, true);
        Assert.NotNull(msg);

        Assert.Equal(0x11, msg.Host);
        Assert.True(msg.Reply);
        Assert.Equal(FunctionCodes.ReadRegister, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x006B, msg.GetAddress());
        Assert.Equal(0x0003, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0x7E, msg.Lrc);
        Assert.Equal(msg.Lrc, msg.Lrc2);
        Assert.Equal("ReadRegister 006B0003", msg.ToString());

        var ms = new MemoryStream();
        msg.Write(ms, null);
        Assert.Equal(dt.ToHex("-"), ms.ToArray().ToHex("-"));
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