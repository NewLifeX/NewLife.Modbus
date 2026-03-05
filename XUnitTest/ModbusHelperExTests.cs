using System;
using System.IO;
using NewLife;
using NewLife.IoT;
using Xunit;

namespace XUnitTest;

/// <summary>ModbusHelper的补充单元测试</summary>
public class ModbusHelperExTests
{
    #region CRC 补充
    [Fact]
    public void Crc_SingleByte()
    {
        var data = new Byte[] { 0x01 };
        var crc = ModbusHelper.Crc(data, 0, data.Length);
        Assert.NotEqual(0, crc);
    }

    [Fact]
    public void Crc_AllZeros()
    {
        var data = new Byte[] { 0x00, 0x00, 0x00, 0x00 };
        var crc = ModbusHelper.Crc(data, 0, data.Length);
        Assert.NotEqual(0, crc);
    }

    [Fact]
    public void Crc_AllFF()
    {
        var data = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        var crc = ModbusHelper.Crc(data, 0, data.Length);
        Assert.NotEqual(0, crc);
    }

    [Fact]
    public void Crc_ConsistencyBetweenOverloads()
    {
        var data = "01-03-00-00-00-0A".ToHex();

        var crc1 = ModbusHelper.Crc(data, 0, data.Length);
        var crc2 = ModbusHelper.Crc((ReadOnlySpan<Byte>)data);
        using var stream = new MemoryStream(data);
        var crc3 = ModbusHelper.Crc(stream);

        Assert.Equal(crc1, crc2);
        Assert.Equal(crc2, crc3);
    }

    [Fact]
    public void Crc_DifferentData_DifferentResults()
    {
        var data1 = "01-03-00-00".ToHex();
        var data2 = "01-03-00-01".ToHex();

        var crc1 = ModbusHelper.Crc(data1, 0, data1.Length);
        var crc2 = ModbusHelper.Crc(data2, 0, data2.Length);

        Assert.NotEqual(crc1, crc2);
    }
    #endregion

    #region LRC 补充
    [Fact]
    public void Lrc_SingleByte()
    {
        var data = new Byte[] { 0x01 };
        var lrc = ModbusHelper.Lrc(data, 0, data.Length);
        // LRC = (-(0x01)) & 0xFF = 0xFF
        Assert.Equal(0xFF, lrc);
    }

    [Fact]
    public void Lrc_AllZeros()
    {
        var data = new Byte[] { 0x00, 0x00, 0x00, 0x00 };
        var lrc = ModbusHelper.Lrc(data, 0, data.Length);
        Assert.Equal(0, lrc);
    }

    [Fact]
    public void Lrc_ConsistencyBetweenOverloads()
    {
        var data = "01-06-00-01-00-17".ToHex();

        var lrc1 = ModbusHelper.Lrc(data, 0, data.Length);
        var lrc2 = ModbusHelper.Lrc((ReadOnlySpan<Byte>)data);
        using var stream = new MemoryStream(data);
        var lrc3 = ModbusHelper.Lrc(stream);

        Assert.Equal(lrc1, lrc2);
        Assert.Equal(lrc2, lrc3);
    }

    [Fact]
    public void Lrc_DifferentData_DifferentResults()
    {
        var data1 = "01-06-00-01".ToHex();
        var data2 = "01-06-00-02".ToHex();

        var lrc1 = ModbusHelper.Lrc(data1, 0, data1.Length);
        var lrc2 = ModbusHelper.Lrc(data2, 0, data2.Length);

        Assert.NotEqual(lrc1, lrc2);
    }

    [Fact]
    public void Lrc_Stream_EmptyStream()
    {
        using var stream = new MemoryStream();
        var lrc = ModbusHelper.Lrc(stream);
        Assert.Equal(0, lrc);
    }
    #endregion

    #region CRC Stream 补充
    [Fact]
    public void Crc_Stream_EmptyStream()
    {
        using var stream = new MemoryStream();
        var crc = ModbusHelper.Crc(stream);
        Assert.Equal(0, crc);
    }

    [Fact]
    public void Crc_Stream_Position()
    {
        // 流从中间开始读取
        var data = "FF-FF-01-05-00-02-FF-00".ToHex();
        using var stream = new MemoryStream(data);
        stream.Position = 2;
        var crc = ModbusHelper.Crc(stream);
        // 只对01-05-00-02-FF-00计算CRC，已知CRC值与Crc_KnownValue测试一致
        Assert.Equal(0xFA2D, crc);
    }
    #endregion
}
