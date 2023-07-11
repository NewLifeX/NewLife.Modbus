using System;
using System.Collections.Generic;
using System.Linq;
using NewLife;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.Log;
using NewLife.Net;
using Xunit;

namespace XUnitTest
{
    public class ModbusTcpDriverTests : DisposeBase
    {
        private readonly NetServer _server;

        public ModbusTcpDriverTests()
        {
            _server = new NetServer(1502)
            {
                Log = XTrace.Log
            };
            _server.Start();
        }

        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            _server.Dispose();
        }

        [Fact]
        public void Test1()
        {
            var driver = new ModbusTcpDriver();

            var p = new ModbusIpParameter
            {
                Host = 3,
                ReadCode = FunctionCodes.ReadRegister,
                WriteCode = FunctionCodes.WriteRegister,
                Server = "tcp://localhost:1502",
            };
            var dic = p.ToDictionary();

            var node = driver.Open(null, dic);

            var node2 = node as ModbusNode;
            Assert.NotNull(node2);

            Assert.Equal(p.Host, node2.Host);
            Assert.Equal(p.ReadCode, node2.ReadCode);
            Assert.Equal(p.WriteCode, node2.WriteCode);
            //Assert.NotNull(node2.Device);

            var modbus = driver.Modbus as ModbusTcp;
            Assert.NotNull(modbus);
            Assert.Equal(p.Server, modbus.Server);
        }

        [Fact]
        public void Test2()
        {
            var driver = new ModbusTcpDriver();

            var p = new ModbusTcpParameter
            {
                Host = 3,
                ReadCode = FunctionCodes.ReadRegister,
                WriteCode = FunctionCodes.WriteRegister,
                Server = "tcp://localhost:1502",
            };

            var node = driver.Open(null, p);

            var node2 = node as ModbusNode;
            Assert.NotNull(node2);

            Assert.Equal(p.Host, node2.Host);
            Assert.Equal(p.ReadCode, node2.ReadCode);
            Assert.Equal(p.WriteCode, node2.WriteCode);
            //Assert.NotNull(node2.Device);

            var modbus = driver.Modbus as ModbusTcp;
            Assert.NotNull(modbus);
            Assert.Equal(p.Server, modbus.Server);
        }
    }
}