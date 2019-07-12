/*
 * PayloadData.cs
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
using System.Collections;
using System.Collections.Generic;

namespace WebSocketCore
{
    /// <summary>
    /// Defines the <see cref="PayloadData" />
    /// </summary>
    internal class PayloadData : IEnumerable<byte>
    {
        #region 字段

        /// <summary>
        /// Represents the empty payload data.
        /// </summary>
        public static readonly PayloadData Empty;

        /// <summary>
        /// Represents the allowable max length.
        /// </summary>
        public static readonly ulong MaxLength;

        /// <summary>
        /// Defines the _code
        /// </summary>
        private ushort _code;

        /// <summary>
        /// Defines the _codeSet
        /// </summary>
        private bool _codeSet;

        /// <summary>
        /// Defines the _data
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Defines the _extDataLength
        /// </summary>
        private long _extDataLength;

        /// <summary>
        /// Defines the _length
        /// </summary>
        private long _length;

        /// <summary>
        /// Defines the _reason
        /// </summary>
        private string _reason;

        /// <summary>
        /// Defines the _reasonSet
        /// </summary>
        private bool _reasonSet;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadData"/> class.
        /// </summary>
        internal PayloadData()
        {
            _code = 1005;
            _reason = String.Empty;

            _data = WebSocket.EmptyBytes;

            _codeSet = true;
            _reasonSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadData"/> class.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        internal PayloadData(byte[] data)
      : this(data, data.LongLength)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadData"/> class.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="length">The length<see cref="long"/></param>
        internal PayloadData(byte[] data, long length)
        {
            _data = data;
            _length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadData"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        internal PayloadData(ushort code, string reason)
        {
            _code = code;
            _reason = reason ?? String.Empty;

            _data = code.Append(reason);
            _length = _data.LongLength;

            _codeSet = true;
            _reasonSet = true;
        }

        /// <summary>
        /// Initializes static members of the <see cref="PayloadData"/> class.
        /// </summary>
        static PayloadData()
        {
            Empty = new PayloadData();
            MaxLength = Int64.MaxValue;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ApplicationData
        /// </summary>
        public byte[] ApplicationData
        {
            get
            {
                return _extDataLength > 0
                       ? _data.SubArray(_extDataLength, _length - _extDataLength)
                       : _data;
            }
        }

        /// <summary>
        /// Gets the ExtensionData
        /// </summary>
        public byte[] ExtensionData
        {
            get
            {
                return _extDataLength > 0
                       ? _data.SubArray(0, _extDataLength)
                       : WebSocket.EmptyBytes;
            }
        }

        /// <summary>
        /// Gets the Length
        /// </summary>
        public ulong Length
        {
            get
            {
                return (ulong)_length;
            }
        }

        /// <summary>
        /// Gets the Code
        /// </summary>
        internal ushort Code
        {
            get
            {
                if (!_codeSet)
                {
                    _code = _length > 1
                            ? _data.SubArray(0, 2).ToUInt16(ByteOrder.Big)
                            : (ushort)1005;

                    _codeSet = true;
                }

                return _code;
            }
        }

        /// <summary>
        /// Gets or sets the ExtensionDataLength
        /// </summary>
        internal long ExtensionDataLength
        {
            get
            {
                return _extDataLength;
            }

            set
            {
                _extDataLength = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether HasReservedCode
        /// </summary>
        internal bool HasReservedCode
        {
            get
            {
                return _length > 1 && Code.IsReserved();
            }
        }

        /// <summary>
        /// Gets the Reason
        /// </summary>
        internal string Reason
        {
            get
            {
                if (!_reasonSet)
                {
                    _reason = _length > 2
                              ? _data.SubArray(2, _length - 2).UTF8Decode()
                              : String.Empty;

                    _reasonSet = true;
                }

                return _reason;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator{byte}"/></returns>
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in _data)
                yield return b;
        }

        /// <summary>
        /// The ToArray
        /// </summary>
        /// <returns>The <see cref="byte[]"/></returns>
        public byte[] ToArray()
        {
            return _data;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return BitConverter.ToString(_data);
        }

        /// <summary>
        /// The Mask
        /// </summary>
        /// <param name="key">The key<see cref="byte[]"/></param>
        internal void Mask(byte[] key)
        {
            for (long i = 0; i < _length; i++)
                _data[i] = (byte)(_data[i] ^ key[i % 4]);
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion 方法
    }
}