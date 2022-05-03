using System.Collections.Generic;
using System.Linq;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using Xunit;

namespace XUnitTest
{
    public class ModbusDriverTests
    {
        [Fact]
        public void Test1()
        {
            var driver = new ModbusTcpDriver();

            var p = new ModbusTcpParameter
            {
                Host = 3,
                ReadCode = FunctionCodes.ReadRegister,
                WriteCode = FunctionCodes.WriteRegister,
                Server = "tcp://localhost:502",
            };
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
            //Assert.Equal(p.Address, modbus.Server);
        }
    }
}