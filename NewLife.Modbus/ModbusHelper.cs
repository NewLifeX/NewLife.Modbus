using System.Text;

namespace NewLife.IoT;

/// <summary>Modbus帮助类</summary>
public static class ModbusHelper
{
    /// <summary>获取字节数组的ASCII字符表示</summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
    public static Byte[] GetAsciiBytes(params Byte[] numbers) => Encoding.UTF8.GetBytes(numbers.SelectMany((Byte n) => n.ToString("X2")).ToArray());

    #region CRC
    private static readonly UInt16[] crc_ta = new UInt16[16] { 0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400, };

    /// <summary>Crc校验</summary>
    /// <param name="data">数据</param>
    /// <param name="offset">偏移</param>
    /// <param name="count">数量</param>
    /// <returns></returns>
    public static UInt16 Crc(Byte[] data, Int32 offset, Int32 count = -1)
    {
        if (data == null || data.Length < 1) return 0;

        if (count <= 0) count = data.Length - offset;

        UInt16 u = 0xFFFF;
        for (var i = offset; i < count; i++)
        {
            var b = data[i];
            u = (UInt16)(crc_ta[(b ^ u) & 15] ^ (u >> 4));
            u = (UInt16)(crc_ta[((b >> 4) ^ u) & 15] ^ (u >> 4));
        }

        return u;
    }

    /// <summary>Crc校验</summary>
    /// <param name="data">数据流</param>
    /// <param name="count">数量</param>
    /// <returns></returns>
    public static UInt16 Crc(Stream data, Int32 count = -1)
    {
        if (data == null || data.Length < 1) return 0;

        UInt16 u = 0xFFFF;

        for (var i = 0; count < 0 || i < count; i++)
        {
            var b = data.ReadByte();
            if (b < 0) break;

            u = (UInt16)(crc_ta[(b ^ u) & 15] ^ (u >> 4));
            u = (UInt16)(crc_ta[((b >> 4) ^ u) & 15] ^ (u >> 4));
        }

        return u;
    }
    #endregion

    #region LRC
    /// <summary>LRC校验</summary>
    /// <param name="data">数据</param>
    /// <param name="offset">偏移</param>
    /// <param name="count">数量</param>
    /// <returns></returns>
    public static Byte Lrc(Byte[] data, Int32 offset, Int32 count = -1)
    {
        if (data == null || data.Length < 1) return 0;

        if (count <= 0) count = data.Length - offset;

        Byte rs = 0;
        for (var i = 0; i < count; i++)
        {
            rs += data[offset + i];
        }

        return (Byte)((rs ^ 0xFF) + 1);
    }

    /// <summary>LRC校验</summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Byte Lrc(Stream stream)
    {
        Byte rs = 0;
        while (true)
        {
            var b = stream.ReadByte();
            if (b < 0) break;

            rs += (Byte)b;
        }

        return (Byte)((rs ^ 0xFF) + 1);
    }
    #endregion
}