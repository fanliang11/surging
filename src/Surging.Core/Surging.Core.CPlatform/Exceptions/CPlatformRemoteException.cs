using System;

namespace Surging.Core.CPlatform.Exceptions
{
    /// <summary>
    /// 远程执行异常（由服务端转发至客户端的异常信息）。
    /// </summary>
    public class CPlatformCommunicationException : CPlatformException
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CPlatformCommunicationException"/> class.
        /// </summary>
        /// <param name="message">异常消息。</param>
        /// <param name="StatusCode">The StatusCode<see cref="int"/></param>
        /// <param name="innerException">内部异常。</param>
        public CPlatformCommunicationException(string message, int StatusCode = 0, Exception innerException = null) : base(message, innerException)
        {
            base.HResult = StatusCode;
        }

        #endregion 构造函数
    }
}