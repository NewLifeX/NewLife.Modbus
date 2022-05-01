using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.IoT.Models;
using NewLife.IoT.Protocols;
using NewLife.IoT.ThingModels;
using NewLife.IoT.ThingSpecification;
using NewLife.Log;
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

        class MyChannel : IDevice
        {
            public String Code { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IDictionary<String, Object> Properties => throw new NotImplementedException();

            public ThingSpec Specification { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IPoint[] Points { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IDictionary<String, Delegate> Services => throw new NotImplementedException();

            public ITracer Tracer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ILog Log { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public Boolean AddData(String name, String value) => throw new NotImplementedException();
            public void AddDevice(IDeviceInfo[] devices) => throw new NotImplementedException();
            public void PostProperty() => throw new NotImplementedException();
            public void RegisterService(String service, Delegate method) => throw new NotImplementedException();
            public void SetProperty(String name, Object value) => throw new NotImplementedException();
            public Task Start() => throw new NotImplementedException();
            public void Stop() => throw new NotImplementedException();
            public Boolean WriteEvent(String type, String name, String remark) => throw new NotImplementedException();
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
            Assert.NotNull(node2.Device);

            var modbus = node2.Modbus as ModbusTcp;
            Assert.NotNull(modbus);
            Assert.Equal(p.Address, modbus.Server);
        }
    }
}