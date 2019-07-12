/*
 * CloseEventArgs.cs
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
    /// Represents the event data for the <see cref="WebSocket.OnClose"/> event.
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        #region 字段

        /// <summary>
        /// Defines the _clean
        /// </summary>
        private bool _clean;

        /// <summary>
        /// Defines the _payloadData
        /// </summary>
        private PayloadData _payloadData;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class.
        /// </summary>
        internal CloseEventArgs()
        {
            _payloadData = PayloadData.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        internal CloseEventArgs(CloseStatusCode code)
      : this((ushort)code, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        internal CloseEventArgs(CloseStatusCode code, string reason)
      : this((ushort)code, reason)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class.
        /// </summary>
        /// <param name="payloadData">The payloadData<see cref="PayloadData"/></param>
        internal CloseEventArgs(PayloadData payloadData)
        {
            _payloadData = payloadData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        internal CloseEventArgs(ushort code)
      : this(code, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        internal CloseEventArgs(ushort code, string reason)
        {
            _payloadData = new PayloadData(code, reason);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the status code for the close.
        /// </summary>
        public ushort Code
        {
            get
            {
                return _payloadData.Code;
            }
        }

        /// <summary>
        /// Gets the reason for the close.
        /// </summary>
        public string Reason
        {
            get
            {
                return _payloadData.Reason ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether WasClean
        /// Gets a value indicating whether the connection has been closed cleanly.
        /// </summary>
        public bool WasClean
        {
            get
            {
                return _clean;
            }

            internal set
            {
                _clean = value;
            }
        }

        /// <summary>
        /// Gets the PayloadData
        /// </summary>
        internal PayloadData PayloadData
        {
            get
            {
                return _payloadData;
            }
        }

        #endregion 属性
    }
}