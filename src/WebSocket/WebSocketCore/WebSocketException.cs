/*
 * WebSocketException.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2016 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

namespace WebSocketCore
{
    /// <summary>
    /// The exception that is thrown when a fatal error occurs in
    /// the WebSocket communication.
    /// </summary>
    public class WebSocketException : Exception
    {
        #region 字段

        /// <summary>
        /// Defines the _code
        /// </summary>
        private CloseStatusCode _code;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        internal WebSocketException()
      : this(CloseStatusCode.Abnormal, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="innerException">The innerException<see cref="Exception"/></param>
        internal WebSocketException(
      CloseStatusCode code, string message, Exception innerException
    )
      : base(message ?? code.GetMessage(), innerException)
        {
            _code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        internal WebSocketException(CloseStatusCode code)
      : this(code, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="innerException">The innerException<see cref="Exception"/></param>
        internal WebSocketException(CloseStatusCode code, Exception innerException)
      : this(code, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        internal WebSocketException(CloseStatusCode code, string message)
      : this(code, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="innerException">The innerException<see cref="Exception"/></param>
        internal WebSocketException(Exception innerException)
      : this(CloseStatusCode.Abnormal, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        internal WebSocketException(string message)
      : this(CloseStatusCode.Abnormal, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketException"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="innerException">The innerException<see cref="Exception"/></param>
        internal WebSocketException(string message, Exception innerException)
      : this(CloseStatusCode.Abnormal, message, innerException)
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the status code indicating the cause of the exception.
        /// </summary>
        public CloseStatusCode Code
        {
            get
            {
                return _code;
            }
        }

        #endregion 属性
    }
}