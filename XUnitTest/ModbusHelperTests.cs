using System;
using System.IO;
using NewLife;
using NewLife.IoT;
using Xunit;

namespace XUnitTest;

public class ModbusHelperTests
{
    #region CRC
    [Fact]
    public void Crc_KnownValue()
    {
        // 01-05-00-02-FF-00 → CRC = 0xFA2D (little-endian stored as 2D FA)
        var data = "01-05-00-02-FF-00".ToHex();
        var crc = ModbusHelper.Crc(data, 0, data.Length);
        Assert.Equal(0xFA2D, crc);
    }

    [Fact]
    public void Crc_KnownValue2()
    {
        // 01-05-00-02-00-00 → CRC = 0x0A6C
        var data = "01-05-00-02-00-00".ToHex();
        var crc = ModbusHelper.Crc(data, 0, data.Length);
        Assert.Equal(0x0A6C, crc);
    }

    [Fact]
    public void Crc_Span()
    {
        var data = "01-05-00-02-FF-00".ToHex();
        var crc = ModbusHelper.Crc((ReadOnlySpan<Byte>)data);
        Assert.Equal(0xFA2D, crc);
    }

    [Fact]
    public void Crc_Stream()
    {
        var data = "01-05-00-02-FF-00".ToHex();
        using var stream = new MemoryStream(data);
        var crc = ModbusHelper.Crc(stream);
        Assert.Equal(0xFA2D, crc);
    }

    [Fact]
    public void Crc_Empty()
    {
        var crc = ModbusHelper.Crc([], 0, 0);
        Assert.Equal(0, crc);
    }

    [Fact]
    public void Crc_EmptySpan()
    {
        var crc = ModbusHelper.Crc(ReadOnlySpan<Byte>.Empty);
        Assert.Equal(0, crc);
    }

    [Fact]
    public void Crc_WithOffset()
    {
        // Pad with 2 extra bytes at start, use offset=2
        // Note: the Crc(data, offset, count) uses count as END INDEX (exclusive)
        var raw = "01-05-00-02-FF-00".ToHex();
        var data = new Byte[raw.Length + 2];
        Array.Copy(raw, 0, data, 2, raw.Length);

        // count 参数在 Crc(data, offset, count) 中实际作为截止索引（exclusive end index）使用，
        // 即循环条件为 i < count，因此 count = offset + 实际字节数
        var crc = ModbusHelper.Crc(data, 2, 2 + raw.Length);
        Assert.Equal(0xFA2D, crc);
    }
    #endregion

    #region LRC
    [Fact]
    public void Lrc_KnownValue()
    {
        // :010600010017 → LRC = 0xE1
        // Decoded: 01-06-00-01-00-17
        var data = "01-06-00-01-00-17".ToHex();
        var lrc = ModbusHelper.Lrc(data, 0, data.Length);
        Assert.Equal(0xE1, lrc);
    }

    [Fact]
    public void Lrc_KnownValue2()
    {
        // :0103006B0003 → LRC = 0x7E
        // Decoded: 11-03-00-6B-00-03
        var data = "11-03-00-6B-00-03".ToHex();
        var lrc = ModbusHelper.Lrc(data, 0, data.Length);
        Assert.Equal(0x7E, lrc);
    }

    [Fact]
    public void Lrc_Span()
    {
        var data = "01-06-00-01-00-17".ToHex();
        var lrc = ModbusHelper.Lrc((ReadOnlySpan<Byte>)data);
        Assert.Equal(0xE1, lrc);
    }

    [Fact]
    public void Lrc_Stream()
    {
        var data = "01-06-00-01-00-17".ToHex();
        using var stream = new MemoryStream(data);
        var lrc = ModbusHelper.Lrc(stream);
        Assert.Equal(0xE1, lrc);
    }

    [Fact]
    public void Lrc_Empty()
    {
        var lrc = ModbusHelper.Lrc([], 0, 0);
        Assert.Equal(0, lrc);
    }

    [Fact]
    public void Lrc_EmptySpan()
    {
        var lrc = ModbusHelper.Lrc(ReadOnlySpan<Byte>.Empty);
        Assert.Equal(0, lrc);
    }

    [Fact]
    public void Lrc_WithOffset()
    {
        var raw = "01-06-00-01-00-17".ToHex();
        var data = new Byte[raw.Length + 2];
        Array.Copy(raw, 0, data, 2, raw.Length);

        var lrc = ModbusHelper.Lrc(data, 2, raw.Length);
        Assert.Equal(0xE1, lrc);
    }
    #endregion
}
