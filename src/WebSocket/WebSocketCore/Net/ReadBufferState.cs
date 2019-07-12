/*
 * ReadBufferState.cs
 *
 * This code is derived from ChunkedInputStream.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2014-2015 sta.blockhead
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

/*
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@novell.com>
 */

using System;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="ReadBufferState" />
    /// </summary>
    internal class ReadBufferState
    {
        #region 字段

        /// <summary>
        /// Defines the _asyncResult
        /// </summary>
        private HttpStreamAsyncResult _asyncResult;

        /// <summary>
        /// Defines the _buffer
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Defines the _count
        /// </summary>
        private int _count;

        /// <summary>
        /// Defines the _initialCount
        /// </summary>
        private int _initialCount;

        /// <summary>
        /// Defines the _offset
        /// </summary>
        private int _offset;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadBufferState"/> class.
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <param name="asyncResult">The asyncResult<see cref="HttpStreamAsyncResult"/></param>
        public ReadBufferState(
      byte[] buffer, int offset, int count, HttpStreamAsyncResult asyncResult)
        {
            _buffer = buffer;
            _offset = offset;
            _count = count;
            _initialCount = count;
            _asyncResult = asyncResult;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the AsyncResult
        /// </summary>
        public HttpStreamAsyncResult AsyncResult
        {
            get
            {
                return _asyncResult;
            }

            set
            {
                _asyncResult = value;
            }
        }

        /// <summary>
        /// Gets or sets the Buffer
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                return _buffer;
            }

            set
            {
                _buffer = value;
            }
        }

        /// <summary>
        /// Gets or sets the Count
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }

            set
            {
                _count = value;
            }
        }

        /// <summary>
        /// Gets or sets the InitialCount
        /// </summary>
        public int InitialCount
        {
            get
            {
                return _initialCount;
            }

            set
            {
                _initialCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the Offset
        /// </summary>
        public int Offset
        {
            get
            {
                return _offset;
            }

            set
            {
                _offset = value;
            }
        }

        #endregion 属性
    }
}