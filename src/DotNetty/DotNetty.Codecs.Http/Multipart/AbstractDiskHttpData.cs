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
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    public abstract class AbstractDiskHttpData : AbstractHttpData
    {
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The SetContent/RenameTo buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        const int c_defaultCopyBufferSize = 81920;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<AbstractDiskHttpData>();

        private FileStream _fileStream;
        private long _chunkPosition;

        protected AbstractDiskHttpData(string name, Encoding charset, long size) : base(name, charset, size)
        {
        }

        protected abstract string DiskFilename { get; }

        protected abstract string Prefix { get; }

        protected internal abstract string BaseDirectory { get; }

        protected abstract string Postfix { get; }

        protected internal abstract bool DeleteOnExit { get; }

        FileStream TempFile()
        {
            string newpostfix;
            string diskFilename = DiskFilename;
            if (diskFilename is object)
            {
                newpostfix = '_' + diskFilename;
            }
            else
            {
                newpostfix = Postfix;
            }
            string directory = string.IsNullOrWhiteSpace(BaseDirectory)
                ? Path.GetTempPath()
                : Path.IsPathRooted(BaseDirectory) ? BaseDirectory : Path.Combine(Path.GetTempPath(), BaseDirectory);
            // File.createTempFile
            string fileName = Path.Combine(directory, $"{Prefix}{Path.GetRandomFileName()}{newpostfix}");
            FileStream tmpFile = File.Create(fileName, 4096, // DefaultBufferSize
                DeleteOnExit ? FileOptions.DeleteOnClose : FileOptions.None);
            return tmpFile;
        }

        public override void SetContent(IByteBuffer buffer)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }
            try
            {
                if (_fileStream is object)
                {
                    Delete();
                }

                _fileStream = TempFile();

                Size = buffer.ReadableBytes;
                CheckSize(Size, MaxSize);
                if (DefinedSize > 0 && DefinedSize < Size)
                {
                    ThrowHelper.ThrowIOException_OutOfSize(Size, DefinedSize);
                }
                if (0u >= (uint)buffer.ReadableBytes)
                {
                    // empty file
                    return;
                }

                _ = buffer.GetBytes(buffer.ReaderIndex, _fileStream, buffer.ReadableBytes);
                _ = buffer.SetReaderIndex(buffer.ReaderIndex + buffer.ReadableBytes);
                _fileStream.Flush();
                SetCompleted();
            }
            finally
            {
                // Release the buffer as it was retained before and we not need a reference to it at all
                // See https://github.com/netty/netty/issues/1516
                _ = buffer.Release();
            }
        }

        public override void AddContent(IByteBuffer buffer, bool last)
        {
            if (buffer is object)
            {
                try
                {
                    int localsize = buffer.ReadableBytes;
                    CheckSize(Size + localsize, MaxSize);
                    if (DefinedSize > 0 && DefinedSize < Size + localsize)
                    {
                        ThrowHelper.ThrowIOException_OutOfSize(Size, DefinedSize);
                    }
                    if (_fileStream is null)
                    {
                        _fileStream = TempFile();
                    }
                    _ = buffer.GetBytes(buffer.ReaderIndex, _fileStream, localsize);
                    _ = buffer.SetReaderIndex(buffer.ReaderIndex + localsize);
                    _fileStream.Flush();

                    Size += localsize;
                }
                finally
                {
                    // Release the buffer as it was retained before and we not need a reference to it at all
                    // See https://github.com/netty/netty/issues/1516
                    _ = buffer.Release();
                }
            }
            if (last)
            {
                if (_fileStream is null)
                {
                    _fileStream = TempFile();
                }
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

        public override void SetContent(Stream source)
        {
            if (source is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source); }

            if (_fileStream is object)
            {
                Delete();
            }

            _fileStream = TempFile();
            int written = 0;
            var bytes = ArrayPool<byte>.Shared.Rent(c_defaultCopyBufferSize);
            try
            {
                while (true)
                {
                    int read = source.Read(bytes, 0, bytes.Length);
                    if (read <= 0)
                    {
                        break;
                    }

                    written += read;
                    CheckSize(written, MaxSize);
                    _fileStream.Write(bytes, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
            _fileStream.Flush();
            // Reset the position to start for reads
            _fileStream.Position -= written;

            Size = written;
            if (DefinedSize > 0 && DefinedSize < Size)
            {
                try
                {
                    Delete(_fileStream);
                }
                catch (Exception error)
                {
                    if (Logger.WarnEnabled) Logger.FailedToDelete(_fileStream, error);
                }
                _fileStream = null;
                ThrowHelper.ThrowIOException_OutOfSize(Size, DefinedSize);
            }
            //isRenamed = true;
            SetCompleted();
        }

        public override void Delete()
        {
            if (_fileStream is object)
            {
                try
                {
                    Delete(_fileStream);
                }
                catch (IOException error)
                {
                    if (Logger.WarnEnabled) Logger.FailedToDeleteFile(error);
                }

                _fileStream = null;
            }
        }

        public override byte[] GetBytes() => _fileStream is null
            ? ArrayExtensions.ZeroBytes : ReadFrom(_fileStream);

        public override IByteBuffer GetByteBuffer()
        {
            if (_fileStream is null)
            {
                return Unpooled.Empty;
            }

            var array = ReadFrom(_fileStream);
            return Unpooled.WrappedBuffer(array);
        }

        public override IByteBuffer GetChunk(int length)
        {
            if (_fileStream is null || 0u >= (uint)length)
            {
                _chunkPosition = 0L;
                return Unpooled.Empty;
            }
            var sizeLeft = _fileStream.Length - _chunkPosition;
            if (0ul >= (ulong)sizeLeft)
            {
                _chunkPosition = 0L;
                return Unpooled.Empty;
            }
            int sliceLength = length;
            if ((uint)sizeLeft < (uint)length)
            {
                sliceLength = (int)sizeLeft;
            }

            var lastPosition = _fileStream.Position;
            _ = _fileStream.Seek(_chunkPosition, SeekOrigin.Begin);
            int read = 0;
            var bytes = new byte[sliceLength];
            while (read < sliceLength)
            {
                int readnow = _fileStream.Read(bytes, read, sliceLength - read);
                if (readnow <= 0)
                {
                    break;
                }

                read += readnow;
            }
            _ = _fileStream.Seek(lastPosition, SeekOrigin.Begin);
            if (0u >= (uint)read)
            {
                return Unpooled.Empty;
            }
            else
            {
                _chunkPosition += read;
            }
            var buffer = Unpooled.WrappedBuffer(bytes);
            _ = buffer.SetReaderIndex(0);
            _ = buffer.SetWriterIndex(read);
            return buffer;
        }

        public override string GetString() => GetString(HttpConstants.DefaultEncoding);

        public override string GetString(Encoding encoding)
        {
            if (_fileStream is null)
            {
                return string.Empty;
            }
            byte[] array = ReadFrom(_fileStream);
            if (encoding is null)
            {
                encoding = HttpConstants.DefaultEncoding;
            }

            return encoding.GetString(array);
        }

        public override bool IsInMemory => false;

        public override bool RenameTo(FileStream destination)
        {
            if (destination is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destination); }
            if (_fileStream is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NoFileDefined();
            }

            // must copy
            var buffer = ArrayPool<byte>.Shared.Rent(c_defaultCopyBufferSize);
            int position = 0;
            var lastPosition = _fileStream.Position;
            _ = _fileStream.Seek(0, SeekOrigin.Begin);

            try
            {
                while (position < Size)
                {
                    int read = _fileStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }

                    destination.Write(buffer, 0, read);
                    position += read;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            if (position == Size)
            {
                try
                {
                    Delete(_fileStream);
                }
                catch (IOException exception)
                {
                    if (Logger.WarnEnabled) Logger.FailedToDeleteFile(exception);
                }
                _fileStream = destination;
                _ = _fileStream.Seek(lastPosition, SeekOrigin.Begin);
                return true;
            }
            else
            {
                try
                {
                    Delete(destination);
                }
                catch (IOException exception)
                {
                    if (Logger.WarnEnabled) Logger.FailedToDeleteFile(exception);
                }
                _ = _fileStream.Seek(lastPosition, SeekOrigin.Begin);
                return false;
            }
        }

        static void Delete(FileStream fileStream)
        {
            string fileName = fileStream.Name;
            fileStream.Dispose();
            File.Delete(fileName);
        }

        static byte[] ReadFrom(Stream fileStream)
        {
            long srcsize = fileStream.Length;
            if (srcsize > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentException_FileTooBig();
            }

            var array = new byte[(int)srcsize];
            var lastPosition = fileStream.Position;
            _ = fileStream.Seek(0, SeekOrigin.Begin);
            _ = fileStream.Read(array, 0, array.Length);
            _ = fileStream.Seek(lastPosition, SeekOrigin.Begin);
            return array;
        }

        public override FileStream GetFile() => _fileStream;

        public override IReferenceCounted Touch(object hint) => this;
    }
}
