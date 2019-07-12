﻿using System;

namespace Surging.Core.CPlatform.Exceptions
{
    /// <summary>
    /// 通讯异常（与服务端进行通讯时发生的异常）。
    /// </summary>
    public class CommunicationException : CPlatformException
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">异常消息。</param>
        /// <param name="innerException">内部异常。</param>
        public CommunicationException(string message, Exception innerException = null) : base(message, innerException)
        {
        }

        #endregion 构造函数
    }
}