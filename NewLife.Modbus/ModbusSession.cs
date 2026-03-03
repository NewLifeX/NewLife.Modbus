using NewLife.Data;
using NewLife.IoT.Protocols;
using NewLife.Net;

namespace NewLife.IoT;

/// <summary>Modbus请求会话</summary>
public class ModbusSession : NetSession<ModbusSlave>
{
    /// <summary>
    /// 接收请求
    /// </summary>
    /// <param name="e"></param>
    protected override void OnReceive(ReceivedEventArgs e)
    {
        if (e.Packet == null) return;

        var msg = ModbusIpMessage.Read(e.Packet.GetSpan());
        if (msg == null) return;

        Log?.Debug("<= {0}", msg);

        var regs = Host.Registers;
        var coils = Host.Coils;

        var rs = msg.CreateReply();
        switch (msg.Code)
        {
            case FunctionCodes.ReadCoil:
            case FunctionCodes.ReadDiscrete:
                if (coils != null) rs.Payload = OnReadCoil(msg);
                break;
            case FunctionCodes.ReadRegister:
            case FunctionCodes.ReadInput:
                if (regs != null) rs.Payload = OnReadRegister(msg);
                break;
            case FunctionCodes.WriteCoil:
                if (coils != null) rs.Payload = OnWriteCoil(msg);
                break;
            case FunctionCodes.WriteRegister:
                if (regs != null) rs.Payload = OnWriteRegister(msg);
                break;
            case FunctionCodes.WriteCoils:
                if (coils != null) rs.Payload = OnWriteCoils(msg);
                break;
            case FunctionCodes.WriteRegisters:
                if (regs != null) rs.Payload = OnWriteRegisters(msg);
                break;
            default:
                break;
        }

        Log?.Debug("=> {0}", rs);

        Send(rs.ToPacket(8192));

        base.OnReceive(e);
    }

    IPacket? OnReadCoil(ModbusMessage msg)
    {
        var coils = Host.Coils;
        if (coils == null) return null;

        // 连续地址，其实地址有可能不是8的倍数
        var (regAddr, regCount) = msg.GetRequest();
        var addr = regAddr - coils[0].Address;
        if (addr >= 0 && addr + regCount <= coils.Count)
        {
            // 取出该段存储单元
            var cs = coils.Skip(addr).Take(regCount).ToList();
            var count = (Int32)Math.Ceiling(regCount / 8.0);
            // 遍历存储单元，把数据聚合成为字节数组返回
            var rs = new Byte[1 + count];
            rs[0] = (Byte)count;
            for (var i = 0; i < count; i++)
            {
                var b = 0;
                // 每个字节最大可存储8位数据，最后一个字节可能不足8位
                var max = regCount - i * 8;
                if (max > 8) max = 8;
                for (var j = 0; j < max; j++)
                {
                    if (cs[i * 8 + j].Value > 0)
                        b |= 1 << j;
                }
                rs[1 + i] = (Byte)b;
            }

            return (ArrayPacket)rs;
        }

        return null;
    }

    IPacket? OnReadRegister(ModbusMessage msg)
    {
        var regs = Host.Registers;
        if (regs == null) return null;

        // 连续地址
        var (regAddr, regCount) = msg.GetRequest();
        var addr = regAddr - regs[0].Address;
        if (addr >= 0 && addr + regCount <= regs.Count)
        {
            var buf = regs.Skip(addr).Take(regCount).SelectMany(e => e.GetData()).ToArray();
            // 使用单一连续数组避免链式包序列化丢失后半段数据
            var rs = new Byte[1 + buf.Length];
            rs[0] = (Byte)buf.Length;
            Array.Copy(buf, 0, rs, 1, buf.Length);
            return (ArrayPacket)rs;
        }

        return null;
    }

    IPacket? OnWriteCoil(ModbusMessage msg)
    {
        var coils = Host.Coils;
        if (coils == null || msg.Payload == null || msg.Payload.Total < 4) return null;

        var reqAddr = msg.GetAddress();
        var value = msg.Payload.ReadBytes(2, 2).ToUInt16(0, false);

        var addr = reqAddr - coils[0].Address;
        if (addr >= 0 && addr < coils.Count)
            coils[addr].Value = value == 0xFF00 ? (Byte)1 : (Byte)0;

        // 响应回显请求：地址 + 值
        return msg.Payload;
    }

    IPacket? OnWriteRegister(ModbusMessage msg)
    {
        var regs = Host.Registers;
        if (regs == null || msg.Payload == null || msg.Payload.Total < 4) return null;

        // 地址在前两字节，值在后两字节
        var reqAddr = msg.GetAddress();
        var value = msg.Payload.ReadBytes(2, 2).ToUInt16(0, false);

        var addr = reqAddr - regs[0].Address;
        if (addr >= 0 && addr < regs.Count)
            regs[addr].Value = value;

        // 响应回显请求：地址 + 值
        return msg.Payload;
    }

    IPacket? OnWriteCoils(ModbusMessage msg)
    {
        var coils = Host.Coils;
        if (coils == null || msg.Payload == null || msg.Payload.Total < 5) return null;

        var (reqAddr, coilCount) = msg.GetRequest();
        var baseAddr = reqAddr - coils[0].Address;

        var byteCount = (coilCount + 7) / 8;
        var k = 0;
        for (var i = 0; i < byteCount; i++)
        {
            var b = msg.Payload.ReadBytes(5 + i, 1)[0];
            for (var j = 0; j < 8 && k < coilCount; j++, k++)
            {
                var addr = baseAddr + k;
                if (addr >= 0 && addr < coils.Count)
                    coils[addr].Value = (Byte)((b >> j) & 1);
            }
        }

        // 响应：地址 + 数量
        var buf = new Byte[4];
        buf.Write(reqAddr, 0, false);
        buf.Write(coilCount, 2, false);
        return (ArrayPacket)buf;
    }

    IPacket? OnWriteRegisters(ModbusMessage msg)
    {
        var regs = Host.Registers;
        if (regs == null || msg.Payload == null || msg.Payload.Total < 5) return null;

        var (reqAddr, regCount) = msg.GetRequest();
        if (msg.Payload.Total < 5 + regCount * 2) return null;

        for (var i = 0; i < regCount; i++)
        {
            var value = msg.Payload.ReadBytes(5 + i * 2, 2).ToUInt16(0, false);
            var addr = (reqAddr + i) - regs[0].Address;
            if (addr >= 0 && addr < regs.Count)
                regs[addr].Value = value;
        }

        // 响应：地址 + 数量
        var buf = new Byte[4];
        buf.Write(reqAddr, 0, false);
        buf.Write(regCount, 2, false);
        return (ArrayPacket)buf;
    }
}
