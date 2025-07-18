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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.Multipart
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common;

    /// <summary>
    /// Abstract Memory HttpData implementation
    /// </summary>
    public abstract class AbstractMemoryHttpData : AbstractHttpData
    {
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The SetContent buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        const int c_defaultCopyBufferSize = 81920;

        private IByteBuffer _byteBuf;
        private int _chunkPosition;

        protected AbstractMemoryHttpData(string name, Encoding charset, long size)
            : base(name, charset, size)
        {
        }

        public override void SetContent(IByteBuffer buffer)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }

            long localsize = buffer.ReadableBytes;
            CheckSize(localsize, MaxSize);
            if (DefinedSize > 0 && DefinedSize < localsize)
            {
                ThrowHelper.ThrowIOException_OutOfSize(localsize, DefinedSize);
            }
            _ = (_byteBuf?.Release());

            _byteBuf = buffer;
            Size = localsize;
            SetCompleted();
        }

        public override void SetContent(Stream inputStream)
        {
            if (inputStream is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inputStream); }

            if (!inputStream.CanRead)
            {
                ThrowHelper.ThrowArgumentException_Stream_NotReadable();
            }

            var bytes = ArrayPool<byte>.Shared.Rent(c_defaultCopyBufferSize);
            IByteBuffer buffer = ArrayPooled.Buffer();
            int written = 0;
            try
            {
                while (true)
                {
                    int read = inputStream.Read(bytes, 0, bytes.Length);
                    if (read <= 0)
                    {
                        break;
                    }

                    _ = buffer.WriteBytes(bytes, 0, read);
                    written += read;
                    CheckSize(written, MaxSize);
                }
            }
            catch (IOException)
            {
                buffer.Release();
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
            Size = written;
            if (DefinedSize > 0 && DefinedSize < Size)
            {
                buffer.Release();
                ThrowHelper.ThrowIOException_OutOfSize(Size, DefinedSize);
            }

            _ = (_byteBuf?.Release());
            _byteBuf = buffer;
            SetCompleted();
        }

        public override void AddContent(IByteBuffer buffer, bool last)
        {
            if (buffer is object)
            {
                long localsize = buffer.ReadableBytes;
                CheckSize(Size + localsize, MaxSize);
                if (DefinedSize > 0 && DefinedSize < Size + localsize)
                {
                    ThrowHelper.ThrowIOException_OutOfSize(Size + localsize, DefinedSize);
                }

                Size += localsize;
                if (_byteBuf is null)
                {
                    _byteBuf = buffer;
                }
                else if (_byteBuf is CompositeByteBuffer buf)
                {
                    _ = buf.AddComponent(true, buffer);
                    _ = buf.SetWriterIndex((int)Size);
                }
                else
                {
                    CompositeByteBuffer compositeBuffer = ArrayPooled.CompositeBuffer(int.MaxValue);
                    _ = compositeBuffer.AddComponents(true, _byteBuf, buffer);
                    _ = compositeBuffer.SetWriterIndex((int)Size);
                    _byteBuf = compositeBuffer;
                }
            }
            if (last)
            {
                SetCompleted();
            }
            else
            {
                if (buffer is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer);
                }
            }
        }

        public override void Delete()
        {
            if (_byteBuf is object)
            {
                _ = _byteBuf.Release();
                _byteBuf = null;
            }
        }

        public override byte[] GetBytes()
        {
            if (_byteBuf is null)
            {
                return Unpooled.Empty.Array;
            }

            var array = new byte[_byteBuf.ReadableBytes];
            _ = _byteBuf.GetBytes(_byteBuf.ReaderIndex, array);
            return array;
        }

        public override string GetString() => GetString(HttpConstants.DefaultEncoding);

        public override string GetString(Encoding encoding)
        {
            if (_byteBuf is null)
            {
                return string.Empty;
            }
            if (encoding is null)
            {
                encoding = HttpConstants.DefaultEncoding;
            }
            return _byteBuf.ToString(encoding);
        }

        /// <summary>
        /// Utility to go from a In Memory FileUpload
        /// to a Disk (or another implementation) FileUpload
        /// </summary>
        /// <returns>the attached ByteBuf containing the actual bytes</returns>
        public override IByteBuffer GetByteBuffer() => _byteBuf;

        public override IByteBuffer GetChunk(int length)
        {
            if (_byteBuf is null || 0u >= (uint)length || 0u >= (uint)_byteBuf.ReadableBytes)
            {
                _chunkPosition = 0;
                return Unpooled.Empty;
            }
            int sizeLeft = _byteBuf.ReadableBytes - _chunkPosition;
            if (0u >= (uint)sizeLeft)
            {
                _chunkPosition = 0;
                return Unpooled.Empty;
            }
            int sliceLength = length;
            if ((uint)sizeLeft < (uint)length)
            {
                sliceLength = sizeLeft;
            }

            IByteBuffer chunk = _byteBuf.RetainedSlice(_chunkPosition, sliceLength);
            _chunkPosition += sliceLength;
            return chunk;
        }

        public override bool IsInMemory => true;

        public override bool RenameTo(FileStream destination)
        {
            if (destination is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destination); }

            if (!destination.CanWrite)
            {
                ThrowHelper.ThrowArgumentException_Stream_NotWritable();
            }
            if (_byteBuf is null)
            {
                // empty file
                return true;
            }

            _ = _byteBuf.GetBytes(_byteBuf.ReaderIndex, destination, _byteBuf.ReadableBytes);
            destination.Flush();
            return true;
        }

        public override FileStream GetFile() => throw new IOException("Not represented by a stream");

        public override IReferenceCounted Touch(object hint)
        {
            _ = (_byteBuf?.Touch(hint));
            return this;
        }
    }
}
