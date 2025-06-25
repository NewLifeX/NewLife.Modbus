﻿using NewLife.Buffers;
using NewLife.Data;
using NewLife.Reflection;
using NewLife.Security;

namespace NewLife.IoT.Protocols;

/// <summary>ModbusRtu消息</summary>
public class ModbusRtuMessage : ModbusMessage
{
    #region 属性
    /// <summary>CRC校验。小端字节序</summary>
    public UInt16 Crc { get; set; }

    /// <summary>CRC校验。根据数据流计算出来的值</summary>
    public UInt16 Crc2 { get; set; }
    #endregion

    #region 方法
    /// <summary>从数据读取消息</summary>
    /// <param name="reader">读取器</param>
    /// <returns></returns>
    public override Boolean Read(ref SpanReader reader)
    {
        var p = reader.Position;
        if (!base.Read(ref reader)) return false;

        // 从负载数据里把Crc取出来
        var pk = Payload;
        var count = pk?.Total ?? 0;
        if (count >= 2 && pk != null)
        {
            Crc = pk.ReadBytes(count - 2, 2).ToUInt16(0, true);
            Payload = pk.Slice(0, count - 2);
        }

        // 计算CRC
        var p2 = reader.Position - 2;
        var sp = reader.Span.Slice(p, p2 - p);
        var buf = sp.ToArray();

        Crc2 = Crc16.ComputeModbus(buf, 0, buf.Length);

        return true;
    }

    /// <summary>从数据读取消息</summary>
    /// <param name="data">数据</param>
    /// <param name="reply">是否响应</param>
    /// <returns></returns>
    public static ModbusRtuMessage? Read(ReadOnlySpan<Byte> data, Boolean reply = false)
    {
        var msg = new ModbusRtuMessage { Reply = reply };
        var reader = new SpanReader(data) { IsLittleEndian = false };
        return msg.Read(ref reader) ? msg : null;
    }

    /// <summary>写入消息到数据</summary>
    /// <param name="writer">写入器</param>
    /// <returns></returns>
    public override Boolean Write(ref SpanWriter writer)
    {
        var p = writer.Position;
        if (!base.Write(ref writer)) return false;

        var size = writer.Position - p;
        writer.Position = p;
        Crc2 = ModbusHelper.Crc(writer.Span[..size]);

        writer.Position = p + size;
        writer.Write(Crc2.GetBytes(true));

        return true;
    }

    /// <summary>创建响应</summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override ModbusMessage CreateReply()
    {
        if (Reply) throw new InvalidOperationException();

        var msg = new ModbusRtuMessage
        {
            Reply = true,
            Host = Host,
            Code = Code,
        };

        return msg;
    }
    #endregion
}