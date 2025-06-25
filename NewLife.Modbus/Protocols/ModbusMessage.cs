﻿using System.Runtime.Serialization;
using NewLife.Buffers;
using NewLife.Data;

namespace NewLife.IoT.Protocols;

/// <summary>Modbus消息</summary>
public class ModbusMessage //: IAccessor
{
    #region 属性
    /// <summary>是否响应</summary>
    [IgnoreDataMember]
    public Boolean Reply { get; set; }

    /// <summary>站号</summary>
    public Byte Host { get; set; }

    /// <summary>操作码</summary>
    public FunctionCodes Code { get; set; }

    /// <summary>错误码</summary>
    public ErrorCodes ErrorCode { get; set; }

    ///// <summary>地址。请求数据，地址与负载；响应数据没有地址只有负载</summary>
    //public UInt16 Address { get; set; }

    /// <summary>负载数据</summary>
    [IgnoreDataMember]
    public IPacket? Payload { get; set; }
    #endregion

    #region 构造
    /// <summary>已重载。友好字符串</summary>
    /// <returns></returns>
    public override String ToString()
    {
        var pk = Payload;

        if (!Reply && pk != null && Code <= FunctionCodes.WriteRegisters)
            return $"{Code} (0x{GetAddress():X4}, {pk?.Slice(2).ToHex()})";

        return $"{Code} {pk?.ToHex()}";
    }
    #endregion

    #region 方法
    /// <summary>从数据读取消息</summary>
    /// <param name="reader">读取器</param>
    /// <returns></returns>
    public virtual Boolean Read(ref SpanReader reader)
    {
        Host = reader.ReadByte();

        var b = reader.ReadByte();
        Code = (FunctionCodes)(b & 0x7F);

        // 异常码
        if ((b & 0x80) == 0x80)
        {
            ErrorCode = (ErrorCodes)reader.ReadByte();
            return true;
        }

        if (reader.FreeCapacity > 0)
            Payload = (ArrayPacket)reader.ReadBytes(reader.FreeCapacity).ToArray();

        return true;
    }

    /// <summary>从数据读取消息</summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual Int32 Read(ReadOnlySpan<Byte> data)
    {
        var reader = new SpanReader(data) { IsLittleEndian = false };
        if (!Read(ref reader)) return -1;

        return reader.Position;
    }

    /// <summary>写入消息到数据</summary>
    /// <param name="writer">写入器</param>
    /// <returns></returns>
    public virtual Boolean Write(ref SpanWriter writer)
    {
        writer.Write(Host);

        var b = (Byte)Code;
        if (ErrorCode > 0) b |= 0x80;
        writer.Write(b);

        // 异常码
        if (ErrorCode > 0)
        {
            writer.Write((Byte)ErrorCode);
            return true;
        }

        var pk = Payload;
        if (pk != null) writer.Write(pk.GetSpan());

        return true;
    }

    /// <summary>写入消息到数据</summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual Int32 Writer(Span<Byte> data)
    {
        var writer = new SpanWriter(data) { IsLittleEndian = false };
        if (!Write(ref writer)) return -1;

        return writer.Position;
    }

    /// <summary>消息转数据包</summary>
    /// <returns></returns>
    public virtual IPacket ToPacket(Int32 bufferSize = 256)
    {
        var pk = new OwnerPacket(bufferSize);
        var writer = new SpanWriter(pk.GetSpan()) { IsLittleEndian = false };
        if (!Write(ref writer)) return null!;

        pk.Resize(writer.Position);

        return pk;
    }

    /// <summary>创建响应</summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual ModbusMessage CreateReply()
    {
        if (Reply) throw new InvalidOperationException();

        var msg = new ModbusMessage
        {
            Reply = true,
            Host = Host,
            Code = Code,
        };

        return msg;
    }

    /// <summary>获取地址。取负载开始2字节作为地址，基础读写指令都有</summary>
    /// <returns></returns>
    public UInt16 GetAddress() => Payload?.ReadBytes(0, 2).ToUInt16(0, false) ?? 0;

    /// <summary>获取请求地址和数值</summary>
    /// <returns></returns>
    public (UInt16 address, UInt16 count) GetRequest()
    {
        var pk = Payload;
        if (pk == null || pk.Total < 4) throw new InvalidDataException();

        var address = pk.ReadBytes(0, 2).ToUInt16(0, false);
        var count = pk.ReadBytes(2, 2).ToUInt16(0, false);

        return (address, count);
    }

    /// <summary>设置请求地址和数值，填充负载数据</summary>
    /// <param name="address">地址</param>
    /// <param name="count">寄存器个数</param>
    public void SetRequest(UInt16 address, UInt16 count)
    {
        var buf = new Byte[4];
        buf.Write(address, 0, false);
        buf.Write(count, 2, false);

        Payload = (ArrayPacket)buf;
    }

    /// <summary>设置请求地址和数据，填充负载数据</summary>
    /// <param name="address"></param>
    /// <param name="data"></param>
    public void SetRequest(UInt16 address, IPacket data)
    {
        //Payload = new ArrayPacket(address.GetBytes(false));
        //Payload.Append(data);
        var pk = new ArrayPacket(address.GetBytes(false));
        pk.Next = data;
        Payload = pk;
    }
    #endregion
}