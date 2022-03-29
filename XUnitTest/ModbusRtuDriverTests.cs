using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.Serial.Protocols;
using Xunit;

namespace XUnitTest
{
    public class ModbusRtuDriverTests
    {
        [Fact]
        public void Test1()
        {
            var driver = new ModbusRtuDriver();

            var p = new ModbusParameter
            {
                Host = 3,
                ReadCode = FunctionCodes.ReadRegister,
                WriteCode = FunctionCodes.WriteRegister,
                Address = "COM1",
            };
            var dic = p.ToDictionary();

            var node = driver.Open(null, dic);

            var node2 = node as ModbusNode;
            Assert.NotNull(node2);

            Assert.Equal(p.Host, node2.Host);
            Assert.Equal(p.ReadCode, node2.ReadCode);
            Assert.Equal(p.WriteCode, node2.WriteCode);
            Assert.Null(node2.Channel);

            var modbus = node2.Modbus as ModbusRtu;
            Assert.NotNull(modbus);
            Assert.Equal(p.Address, modbus.PortName);
        }
    }
}