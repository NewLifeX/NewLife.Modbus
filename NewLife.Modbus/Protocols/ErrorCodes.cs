using System.ComponentModel;

namespace NewLife.IoT.Protocols;

/// <summary>错误码</summary>
public enum ErrorCodes
{
    /// <summary>非法功能。功能码不能被从机识别</summary>
    /// <remarks>
    /// 对于服务器(或从站)来说，询问中接收到的功能码是不可允许的操作。这也许 是因为功能码仅仅适用于新设备而在被选单元中是不可实现的。
    /// 同时，还指出服务器(或从站)在错误状态中处理这种请求，例如：因为它是未配置的，并且要求返回寄存器值。
    /// </remarks>
    [Description("01非法功能")]
    IllegalFunction = 1,

    /// <summary>非法数据地址。从机的单元标识符不正确</summary>
    /// <remarks>
    /// 对于服务器(或从站)来说，询问中接收到的数据地址是不可允许的地址。特别 是，参考号和传输长度的组合是无效的。
    /// 对于带有 100 个寄存器的控制器来说， 带有偏移量 96 和长度 4 的请求会成功，带有偏移量 96 和长度 5 的请求将产生异常码 02。
    /// </remarks>
    [Description("02非法数据地址")]
    IllegalDataAddress = 2,

    /// <summary>非法数据值。值不被从机接受</summary>
    /// <remarks>
    /// 对于服务器(或从站)来说，询问中包括的值是不可允许的值。这个值指示了组 合请求剩余结构中的故障，例如：隐含长度是不正确的。
    /// 并不意味着，因为MODBUS 协议不知道任何特殊寄存器的任何特殊值的重要意义，寄存器中被 提交存储的数据项有一个应用程序期望之外的值。
    /// </remarks>
    [Description("03非法数据值")]
    IllegalDataValue = 3,

    /// <summary>从站设备故障。当从机试图执行请求的操作时，发生了不可恢复的错误</summary>
    /// <remarks>
    /// 当服务器(或从站)正在设法执行请求的操作时，产生不可重新获得的差错。
    /// </remarks>
    [Description("04从站设备故障")]
    SlaveDeviceFailure = 4,

    /// <summary>从机已接受请求并正在处理，但需要很长时间。返回此响应是为了防止在主机中发生超时错误。主站可以在下一个轮询程序中发出一个完整的消息，以确定处理是否完成。</summary>
    /// <remarks>
    /// 与编程命令一起使用。服务器(或从站)已经接受请求，并切正在处理这个请求，但是需要长的持续时间进行这些操作。
    /// 返回这个响应防止在客户机(或主站)中 发生超时错误。客户机(或主站)可以继续发送轮询程序完成报文来确定是否完 成处理。
    /// </remarks>
    [Description("05确认正在处理")]
    Acknowledge = 5,

    /// <summary>从属设备忙。从站正在处理长时间命令。Master应该稍后重试</summary>
    /// <remarks>
    /// 与编程命令一起使用。服务器(或从站)正在处理长持续时间的程序命令。张服 务器(或从站)空闲时，用户(或主站)应该稍后重新传输报文。
    /// </remarks>
    [Description("06从属设备忙")]
    SlaveDeviceBusy = 6,

    /// <summary>从站不能执行程序功能。主站应该向从站请求诊断或错误信息。</summary>
    [Description("07从站不能执行程序功能")]
    NegativeAcknowledgement = 7,

    /// <summary>存储奇偶性差错。从站在内存中检测到奇偶校验错误。主设备可以重试请求，但从设备上可能需要服务。</summary>
    /// <remarks>
    /// 与功能码 20 和 21 以及参考类型 6 一起使用，指示扩展文件区不能通过一致性校验。
    /// 服务器(或从站)设法读取记录文件，但是在存储器中发现一个奇偶校验错误。 客户机(或主方)可以重新发送请求，但可以在服务器(或从站)设备上要求服务。
    /// </remarks>
    [Description("08存储奇偶性差错")]
    MemoryParityError = 8,

    /// <summary>不可用网关路径。专门用于Modbus网关。表示配置错误的网关。</summary>
    /// <remarks>
    /// 与网关一起使用，指示网关不能为处理请求分配输入端口至输出端口的内部通信路径。通常意味着网关是错误配置的或过载的。
    /// </remarks>
    [Description("10不可用网关路径")]
    GatewayPathUnavailable = 10,

    /// <summary>网关目标设备响应失败。专用于Modbus网关的响应。当从站无法响应时发送。</summary>
    /// <remarks>
    /// 与网关一起使用，指示没有从目标设备中获得响应。通常意味着设备未在网络中。
    /// </remarks>
    [Description("11网关目标设备响应失败")]
    GatewayTargetDeviceFailed = 11,
}