/*
 * RequestStream.cs
 *
 * This code is derived from RequestStream.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2015 sta.blockhead
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
using System.IO;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="RequestStream" />
    /// </summary>
    internal class RequestStream : Stream
    {
        #region 字段

        /// <summary>
        /// Defines the _bodyLeft
        /// </summary>
        private long _bodyLeft;

        /// <summary>
        /// Defines the _buffer
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Defines the _count
        /// </summary>
        private int _count;

        /// <summary>
        /// Defines the _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Defines the _offset
        /// </summary>
        private int _offset;

        /// <summary>
        /// Defines the _stream
        /// </summary>
        private Stream _stream;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestStream"/> class.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <param name="contentLength">The contentLength<see cref="long"/></param>
        internal RequestStream(
      Stream stream, byte[] buffer, int offset, int count, long contentLength)
        {
            _stream = stream;
            _buffer = buffer;
            _offset = offset;
            _count = count;
            _bodyLeft = contentLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestStream"/> class.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        internal RequestStream(Stream stream, byte[] buffer, int offset, int count)
      : this(stream, buffer, offset, count, -1)
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether CanRead
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether CanSeek
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether CanWrite
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Length
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the Position
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The BeginRead
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <param name="callback">The callback<see cref="AsyncCallback"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        /// <returns>The <see cref="IAsyncResult"/></returns>
        public override IAsyncResult BeginRead(
      byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());

            var nread = fillFromBuffer(buffer, offset, count);
            if (nread > 0 || nread == -1)
            {
                var ares = new HttpStreamAsyncResult(callback, state);
                ares.Buffer = buffer;
                ares.Offset = offset;
                ares.Count = count;
                ares.SyncRead = nread > 0 ? nread : 0;
                ares.Complete();

                return ares;
            }

            // Avoid reading past the end of the request to allow for HTTP pipelining.
            if (_bodyLeft >= 0 && count > _bodyLeft)
                count = (int)_bodyLeft;

            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// The BeginWrite
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <param name="callback">The callback<see cref="AsyncCallback"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        /// <returns>The <see cref="IAsyncResult"/></returns>
        public override IAsyncResult BeginWrite(
      byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The Close
        /// </summary>
        public override void Close()
        {
            _disposed = true;
        }

        /// <summary>
        /// The EndRead
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="IAsyncResult"/></param>
        /// <returns>The <see cref="int"/></returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());

            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            if (asyncResult is HttpStreamAsyncResult)
            {
                var ares = (HttpStreamAsyncResult)asyncResult;
                if (!ares.IsCompleted)
                    ares.AsyncWaitHandle.WaitOne();

                return ares.SyncRead;
            }

            // Close on exception?
            var nread = _stream.EndRead(asyncResult);
            if (nread > 0 && _bodyLeft > 0)
                _bodyLeft -= nread;

            return nread;
        }

        /// <summary>
        /// The EndWrite
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="IAsyncResult"/></param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The Flush
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// The Read
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());

            // Call the fillFromBuffer method to check for buffer boundaries even when _bodyLeft is 0.
            var nread = fillFromBuffer(buffer, offset, count);
            if (nread == -1) // No more bytes available (Content-Length).
                return 0;

            if (nread > 0)
                return nread;

            nread = _stream.Read(buffer, offset, count);
            if (nread > 0 && _bodyLeft > 0)
                _bodyLeft -= nread;

            return nread;
        }

        /// <summary>
        /// The Seek
        /// </summary>
        /// <param name="offset">The offset<see cref="long"/></param>
        /// <param name="origin">The origin<see cref="SeekOrigin"/></param>
        /// <returns>The <see cref="long"/></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The SetLength
        /// </summary>
        /// <param name="value">The value<see cref="long"/></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The Write
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        // Returns 0 if we can keep reading from the base stream,
        // > 0 if we read something from the buffer,
        // -1 if we had a content length set and we finished reading that many bytes.
        /// <summary>
        /// The fillFromBuffer
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        private int fillFromBuffer(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "A negative value.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "A negative value.");

            var len = buffer.Length;
            if (offset + count > len)
                throw new ArgumentException(
                  "The sum of 'offset' and 'count' is greater than 'buffer' length.");

            if (_bodyLeft == 0)
                return -1;

            if (_count == 0 || count == 0)
                return 0;

            if (count > _count)
                count = _count;

            if (_bodyLeft > 0 && count > _bodyLeft)
                count = (int)_bodyLeft;

            Buffer.BlockCopy(_buffer, _offset, buffer, offset, count);
            _offset += count;
            _count -= count;
            if (_bodyLeft > 0)
                _bodyLeft -= count;

            return count;
        }

        #endregion 方法
    }
}