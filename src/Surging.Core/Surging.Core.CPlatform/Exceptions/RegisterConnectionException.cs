using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Exceptions
{
    /// <summary>
    /// Defines the <see cref="RegisterConnectionException" />
    /// </summary>
    public class RegisterConnectionException : CPlatformException
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="innerException">The innerException<see cref="Exception"/></param>
        public RegisterConnectionException(string message, Exception innerException = null) : base(message, innerException)
        {
        }

        #endregion 构造函数
    }
}