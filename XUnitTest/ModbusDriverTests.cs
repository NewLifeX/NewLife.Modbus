using System.Collections.Generic;
using System.Linq;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
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
        var p = new ModbusTcpParameter();
        Rand.Fill(p);
        var dic = p.ToDictionary();

        var node = driver.Open(null, dic);

        var node2 = node as ModbusNode;
        Assert.NotNull(node2);

        Assert.Equal(p.Host, node2.Host);
        Assert.Equal(p.ReadCode, node2.ReadCode);
        Assert.Equal(p.WriteCode, node2.WriteCode);
        Assert.Null(node2.Device);

        var modbus = node2.Modbus as ModbusTcp;
        Assert.NotNull(modbus);
        Assert.Equal(p.Server, modbus.Server);
        Assert.Equal(p.Timeout, modbus.Timeout);
        //Assert.Equal(p.BatchSize, modbus.BatchSize);
        //Assert.Equal(p.Delay, modbus.Delay);
    }

    [Fact]
    public void CloseTest()
    {
        var driver = new ModbusTcpDriver();

        var p = new ModbusTcpParameter();
        Rand.Fill(p);
        var dic = p.ToDictionary();

        var node1 = driver.Open(null, dic);
        var m1 = driver.Modbus;

        var node2 = driver.Open(null, dic);
        var m2 = driver.Modbus;
        Assert.NotNull(driver.Modbus);
        Assert.Equal(m1, m2);
        Assert.Equal(m1, driver.Modbus);

        driver.Close(node1);
        Assert.NotNull(driver.Modbus);

        driver.Close(node2);
        Assert.Null(driver.Modbus);
    }
}