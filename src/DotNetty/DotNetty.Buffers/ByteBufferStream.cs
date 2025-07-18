/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;

namespace DotNetty.Buffers
{
    public class ByteBufferStream : Stream
    {
        #region @@ Fields @@

        private bool _isOpen;
        private bool _writable;
        private bool _releaseReferenceOnClosure;
        private readonly IByteBuffer _buffer;

        #endregion

        #region @@ Constructors @@

        public ByteBufferStream(IByteBuffer buffer) : this(buffer, true, false) { }

        public ByteBufferStream(IByteBuffer buffer, bool releaseReferenceOnClosure) : this(buffer, true, releaseReferenceOnClosure) { }

        public ByteBufferStream(IByteBuffer buffer, bool writable, bool releaseReferenceOnClosure)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }

            _buffer = buffer;
            _releaseReferenceOnClosure = releaseReferenceOnClosure;
            _isOpen = true;
            _writable = writable;
        }

        #endregion

        #region @@ Properties @@

        public IByteBuffer Buffer => _buffer;

        public override bool CanRead => _isOpen;

        public override bool CanSeek => _isOpen;

        public override bool CanWrite => _writable;

        public override long Length
        {
            get
            {
                EnsureNotClosed();
                return _buffer.WriterIndex;
            }
        }

        #endregion

        #region -- ReaderPosition --

        /// <summary>Only for reader position</summary>
        public override long Position
        {
            get
            {
                EnsureNotClosed();
                return _buffer.ReaderIndex;
            }
            set
            {
                EnsureNotClosed();
                _ = _buffer.SetReaderIndex((int)value);
            }
        }

        /// <summary>Only for reader position</summary>
        public void MarkPosition()
        {
            EnsureNotClosed();
            _ = _buffer.MarkReaderIndex();
        }

        /// <summary>Only for reader position</summary>
        public void ResetPosition()
        {
            EnsureNotClosed();
            _ = _buffer.ResetReaderIndex();
        }

        /// <summary>Only for reader position</summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotClosed();
            switch (origin)
            {
                case SeekOrigin.Current:
                    offset += Position;
                    break;
                case SeekOrigin.End:
                    offset += Length;
                    break;
                    //case SeekOrigin.Begin:
                    //default:
                    //    break;
            }
            Position = offset;
            return Position;
        }

        #endregion

        #region -- WriterPosition --

        public int WriterPosition
        {
            get
            {
                EnsureNotClosed();
                return _buffer.WriterIndex;
            }
            set
            {
                EnsureNotClosed();
                _ = _buffer.SetWriterIndex(value);
            }
        }

        public void MarkWriterPosition()
        {
            EnsureNotClosed();
            _ = _buffer.MarkWriterIndex();
        }

        public void ResetWriterPosition()
        {
            EnsureNotClosed();
            _ = _buffer.ResetWriterIndex();
        }

        #endregion

        #region -- SetLength --

        public override void SetLength(long value)
        {
            EnsureNotClosed();
            _ = _buffer.EnsureWritable((int)value);
        }

        #endregion

        #region -- CopyToAsync --

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureNotClosed();

            var remaining = _buffer.ReadableBytes;
            if ((uint)(remaining - 1) > SharedConstants.TooBigOrNegative) // remaining <= 0
            {
                return;
            }

            ValidateCopyToArgs(destination);

            var ioBuffers = _buffer.GetIoBuffers();
            foreach (var ioBuffer in ioBuffers)
            {
                destination.Write(ioBuffer.Array, ioBuffer.Offset, ioBuffer.Count);
            }
        }
#endif

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureNotClosed();

            var remaining = _buffer.ReadableBytes;
            if ((uint)(remaining - 1) > SharedConstants.TooBigOrNegative) // remaining <= 0
            {
                return;
            }

            ValidateCopyToArgs(destination);

            // If cancelled - return fast:
            if (cancellationToken.IsCancellationRequested) { return; }

            var ioBuffers = _buffer.GetIoBuffers();
            if (destination is MemoryStream)
            {
                try
                {
                    // If destination is a MemoryStream, CopyTo synchronously:
                    foreach (var ioBuffer in ioBuffers)
                    {
                        if (cancellationToken.IsCancellationRequested) { return; }
                        destination.Write(ioBuffer.Array, ioBuffer.Offset, ioBuffer.Count);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                // If destination is not a memory stream, write there asynchronously:
                foreach (var ioBuffer in ioBuffers)
                {
                    if (cancellationToken.IsCancellationRequested) { return; }
                    await destination.WriteAsync(ioBuffer.Array, ioBuffer.Offset, ioBuffer.Count);
                }
            }
        }

        #endregion

        #region -- Read --

        public override int ReadByte()
        {
            EnsureNotClosed();
            try
            {
                return _buffer.ReadByte();
            }
            catch
            {
                return -1;
            }
        }

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        public override int Read(Span<byte> buffer)
        {
            EnsureNotClosed();

            return _buffer.ReadBytes(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureNotClosed();

            return new ValueTask<int>(_buffer.ReadBytes(buffer));
        }
#endif

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }
            if ((uint)offset > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument.offset); }
            if ((uint)count > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument.count); }
            if ((uint)buffer.Length < (uint)(offset + count)) { ThrowHelper.ThrowArgumentException_InvalidOffLen(); }

            EnsureNotClosed();

            int read = Math.Min(count, _buffer.ReadableBytes);
            if (0u >= (uint)read) { return 0; }
            _ = _buffer.ReadBytes(buffer, offset, read);
            return read;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.FromCanceled<int>(cancellationToken);
            }
            try
            {
                var readNum = Read(buffer, offset, count);
                return Task.FromResult(readNum);
            }
            //catch (OperationCanceledException oce)
            //{
            //    return Task.FromCancellation<int>(oce);
            //}
            catch (Exception ex2)
            {
                return TaskUtil.FromException<int>(ex2);
            }
        }

        #endregion

        #region -- Write --

        public override void WriteByte(byte value)
        {
            EnsureNotClosed();
            EnsureWriteable();

            _ = _buffer.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }
            if ((uint)offset > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument.offset); }
            if ((uint)count > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument.count); }
            if ((uint)buffer.Length < (uint)(offset + count)) { ThrowHelper.ThrowArgumentException_InvalidOffLen(); }

            EnsureNotClosed();
            EnsureWriteable();

            _ = _buffer.WriteBytes(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }
            if ((uint)offset > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument.offset); }
            if ((uint)count > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument.count); }
            if ((uint)buffer.Length < (uint)(offset + count)) { ThrowHelper.ThrowArgumentException_InvalidOffLen(); }

            EnsureNotClosed();
            EnsureWriteable();

            // If cancellation is already requested, bail early
            if (cancellationToken.IsCancellationRequested) { return TaskUtil.FromCanceled(cancellationToken); }

            try
            {
                _ = _buffer.WriteBytes(buffer, offset, count);
                return TaskUtil.Completed;
            }
            //catch (OperationCanceledException oce)
            //{
            //    return Task.FromCancellation<VoidTaskResult>(oce);
            //}
            catch (Exception exception)
            {
                return TaskUtil.FromException(exception);
            }
        }

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureNotClosed();
            EnsureWriteable();

            _buffer.WriteBytes(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureNotClosed();
            EnsureWriteable();

            _buffer.WriteBytes(buffer);
            return default;
        }
#endif

        #endregion

        #region -- Flush --

        public override void Flush() { }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) { return TaskUtil.FromCanceled(cancellationToken); }

            try
            {
                Flush();
                return TaskUtil.Completed;
            }
            catch (Exception ex)
            {
                return TaskUtil.FromException(ex);
            }
        }

        #endregion

        #region ++ Dispose ++

        protected override void Dispose(bool disposing)
        {
            _isOpen = false;
            _writable = false;
            if (_releaseReferenceOnClosure)
            {
                _releaseReferenceOnClosure = false;
                if (disposing)
                {
                    _ = _buffer.Release();
                }
                else
                {
                    _buffer.SafeRelease();
                }
            }
        }

        #endregion

        #region ** Helper **

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void EnsureNotClosed()
        {
            if (!_isOpen) { ThrowHelper.ThrowObjectDisposedException_StreamIsClosed(); }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void EnsureWriteable()
        {
            if (!_writable) { ThrowHelper.ThrowNotSupportedException_UnwritableStream(); }
        }

        /// <summary>Validate the arguments to CopyTo, as would Stream.CopyTo.</summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static void ValidateCopyToArgs(Stream destination)
        {
            if (destination is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destination); }

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite)
            {
                if (destination.CanRead)
                {
                    ThrowHelper.ThrowNotSupportedException_UnwritableStream();
                }
                else
                {
                    ThrowHelper.ThrowObjectDisposedException_StreamIsClosed(ExceptionArgument.destination);
                }
            }
        }

        #endregion
    }
}
