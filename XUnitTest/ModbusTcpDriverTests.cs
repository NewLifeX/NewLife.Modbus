using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.IoT.Thing;
using NewLife.IoT.ThingSpecification;
using NewLife.Net;
using Xunit;

namespace XUnitTest
{
    public class ModbusTcpDriverTests
    {
        private NetServer _server;

        public ModbusTcpDriverTests()
        {
            _server = new NetServer(502);
            _server.Start();
        }

        class MyChannel : IChannel
        {
            public IThing Thing => throw new NotImplementedException();

            public ThingSpec Specification => throw new NotImplementedException();
        }

        [Fact]
        public void Test1()
        {
            var driver = new ModbusTcpDriver();

            var p = new ModbusParameter
            {
                Host = 3,
                ReadCode = FunctionCodes.ReadRegister,
                WriteCode = FunctionCodes.WriteRegister,
                Address = "tcp://localhost:502",
            };
            var dic = p.ToDictionary();

            var node = driver.Open(new MyChannel(), dic);

            var node2 = node as ModbusNode;
            Assert.NotNull(node2);

            Assert.Equal(p.Host, node2.Host);
            Assert.Equal(p.ReadCode, node2.ReadCode);
            Assert.Equal(p.WriteCode, node2.WriteCode);
            Assert.NotNull(node2.Channel);

            var modbus = node2.Modbus as ModbusTcp;
            Assert.NotNull(modbus);
            Assert.Equal(p.Address, modbus.Server);
        }
    }
}