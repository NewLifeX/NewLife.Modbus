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
        var msg = ModbusIpMessage.Read(e.Packet);
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
                break;
            case FunctionCodes.WriteRegister:
                if (regs != null) rs.Payload = OnWriteRegister(msg);
                break;
            case FunctionCodes.WriteCoils:
                break;
            case FunctionCodes.WriteRegisters:
                break;
            default:
                break;
        }

        Log?.Debug("=> {0}", rs);

        Send(rs.ToPacket());

        base.OnReceive(e);
    }

    Packet OnReadCoil(ModbusMessage msg)
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

            return rs;
        }

        return null;
    }

    Packet OnReadRegister(ModbusMessage msg)
    {
        var regs = Host.Registers;
        if (regs == null) return null;

        // 连续地址
        var (regAddr, regCount) = msg.GetRequest();
        var addr = regAddr - regs[0].Address;
        if (addr >= 0 && addr + regCount <= regs.Count)
        {
            var buf = regs.Skip(addr).Take(regCount).SelectMany(e => e.GetData()).ToArray();
            var pk = new Packet(new Byte[] { (Byte)buf.Length });
            pk.Append(buf);
            return pk;
        }

        return null;
    }

    Packet OnWriteRegister(ModbusMessage msg)
    {
        var regs = Host.Registers;
        if (regs == null) return null;

        // 连续地址
        var regCount = 0;
        var reqAddr = msg.GetAddress();
        for (var i = 0; i < 256 && i + 1 < msg.Payload.Total; i += 2)
        {
            var value = msg.Payload.ReadBytes(i, 2).ToUInt16(0, false);
            var addr = reqAddr - regs[0].Address;
            if (addr >= 0 && addr < regs.Count)
            {
                var ru = regs[addr];
                ru.Value = value;
                regCount++;
            }
        }
        //Invoke(() => { dgv.Refresh(); });

        {
            var addr = reqAddr - regs[0].Address;
            return regs.Skip(addr).Take(regCount).SelectMany(e => e.GetData()).ToArray();
        }
    }
}
