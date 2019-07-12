/*
 * Chunk.cs
 *
 * This code is derived from ChunkStream.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2003 Ximian, Inc (http://www.ximian.com)
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
 * - Gonzalo Paniagua Javier <gonzalo@ximian.com>
 */

using System;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="Chunk" />
    /// </summary>
    internal class Chunk
    {
        #region 字段

        /// <summary>
        /// Defines the _data
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Defines the _offset
        /// </summary>
        private int _offset;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Chunk"/> class.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        public Chunk(byte[] data)
        {
            _data = data;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ReadLeft
        /// </summary>
        public int ReadLeft
        {
            get
            {
                return _data.Length - _offset;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Read
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            var left = _data.Length - _offset;
            if (left == 0)
                return left;

            if (count > left)
                count = left;

            Buffer.BlockCopy(_data, _offset, buffer, offset, count);
            _offset += count;

            return count;
        }

        #endregion 方法
    }
}