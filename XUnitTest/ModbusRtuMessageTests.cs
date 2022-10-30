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
        Assert.Equal(0x02, msg.Address);
        Assert.Equal(0xFF00, msg.Payload.ToArray().ToUInt16(0, false));
    }
}