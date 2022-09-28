namespace NewLife.IoT.Protocols;

/// <summary>Modbus异常</summary>
public class ModbusException : Exception
{
    /// <summary>异常代码</summary>
    public ErrorCodes ErrorCode { get; set; }

    /// <summary>
    /// 实例化Modbus异常
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    public ModbusException(ErrorCodes errorCode, String message) : base(message) => ErrorCode = errorCode;
}