using NewLife;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

public class ModbusRtuMessageTests
{
    [Fact]
    public void Test1()
    {
        var str = "01-05-00-02-FF-00-2D-FA";
        var dt = str.ToHex();

        var msg = ModbusRtuMessage.Read(dt, false);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.False(msg.Reply);
        Assert.Equal(FunctionCodes.WriteCoil, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x02, msg.GetAddress());
        Assert.Equal(0xFF00, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0x2DFA, msg.Crc);
        Assert.Equal("WriteCoil (0x0002, FF00)", msg.ToString());
    }

    [Fact]
    public void Test2()
    {
        var str = "01-05-00-02-00-00-6C-0A";
        var dt = str.ToHex();

        var msg = ModbusRtuMessage.Read(dt, true);
        Assert.NotNull(msg);

        Assert.Equal(1, msg.Host);
        Assert.True(msg.Reply);
        Assert.Equal(FunctionCodes.WriteCoil, msg.Code);
        Assert.Equal((ErrorCodes)0, msg.ErrorCode);
        Assert.Equal(0x02, msg.GetAddress());
        Assert.Equal(0x0000, msg.Payload.ReadBytes(2, 2).ToUInt16(0, false));
        Assert.Equal(0x6C0A, msg.Crc);
        Assert.Equal("WriteCoil 00020000", msg.ToString());
    }
}