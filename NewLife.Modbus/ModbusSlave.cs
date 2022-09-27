using NewLife.IoT.Models;
using NewLife.Net;
using NewLife.Net.Handlers;

namespace NewLife.IoT;

/// <summary>Modbus从机/服务器</summary>
public class ModbusSlave : NetServer<ModbusSession>
{
    #region 属性
    /// <summary>寄存器区</summary>
    public List<RegisterUnit> Registers { get; set; }

    /// <summary>线圈区</summary>
    public List<CoilUnit> Coils { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public ModbusSlave()
    {
        Port = 502;
    }

    /// <summary>
    /// 启动
    /// </summary>
    protected override void OnStart()
    {
        // 加入定长编码器，处理Tcp粘包
        Add(new LengthFieldCodec { Offset = 4, Size = -2 });

        base.OnStart();
    }
    #endregion
}