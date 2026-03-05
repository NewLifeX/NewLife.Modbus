using System;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusException单元测试</summary>
public class ModbusExceptionTests
{
    [Fact]
    public void Constructor_SetsErrorCodeAndMessage()
    {
        var ex = new ModbusException(ErrorCodes.IllegalFunction, "Invalid function");

        Assert.Equal(ErrorCodes.IllegalFunction, ex.ErrorCode);
        Assert.Equal("Invalid function", ex.Message);
    }

    [Fact]
    public void Constructor_IllegalDataAddress()
    {
        var ex = new ModbusException(ErrorCodes.IllegalDataAddress, "Bad address");
        Assert.Equal(ErrorCodes.IllegalDataAddress, ex.ErrorCode);
        Assert.Equal("Bad address", ex.Message);
    }

    [Fact]
    public void Constructor_IllegalDataValue()
    {
        var ex = new ModbusException(ErrorCodes.IllegalDataValue, "Bad value");
        Assert.Equal(ErrorCodes.IllegalDataValue, ex.ErrorCode);
    }

    [Fact]
    public void Constructor_SlaveDeviceFailure()
    {
        var ex = new ModbusException(ErrorCodes.SlaveDeviceFailure, "Device failure");
        Assert.Equal(ErrorCodes.SlaveDeviceFailure, ex.ErrorCode);
    }

    [Fact]
    public void Constructor_GatewayErrors()
    {
        var ex1 = new ModbusException(ErrorCodes.GatewayPathUnavailable, "Gateway path error");
        Assert.Equal(ErrorCodes.GatewayPathUnavailable, ex1.ErrorCode);

        var ex2 = new ModbusException(ErrorCodes.GatewayTargetDeviceFailed, "Target failed");
        Assert.Equal(ErrorCodes.GatewayTargetDeviceFailed, ex2.ErrorCode);
    }

    [Fact]
    public void IsException()
    {
        var ex = new ModbusException(ErrorCodes.SlaveDeviceBusy, "busy");
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void AllErrorCodes()
    {
        // 验证所有ErrorCodes枚举值
        Assert.Equal(1, (Int32)ErrorCodes.IllegalFunction);
        Assert.Equal(2, (Int32)ErrorCodes.IllegalDataAddress);
        Assert.Equal(3, (Int32)ErrorCodes.IllegalDataValue);
        Assert.Equal(4, (Int32)ErrorCodes.SlaveDeviceFailure);
        Assert.Equal(5, (Int32)ErrorCodes.Acknowledge);
        Assert.Equal(6, (Int32)ErrorCodes.SlaveDeviceBusy);
        Assert.Equal(7, (Int32)ErrorCodes.NegativeAcknowledgement);
        Assert.Equal(8, (Int32)ErrorCodes.MemoryParityError);
        Assert.Equal(10, (Int32)ErrorCodes.GatewayPathUnavailable);
        Assert.Equal(11, (Int32)ErrorCodes.GatewayTargetDeviceFailed);
    }
}
