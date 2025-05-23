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
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common;

    public class MixedFileUpload : IFileUpload
    {
        private readonly string _baseDir;
        private readonly bool _deleteOnExit;

        private IFileUpload _fileUpload;

        private readonly long _limitSize;
        private readonly long _definedSize;

        private long _maxSize = DefaultHttpDataFactory.MaxSize;

        public MixedFileUpload(string name, string fileName, string contentType,
            string contentTransferEncoding, Encoding charset, long size, long limitSize)
            : this(name, fileName, contentType, contentTransferEncoding,
                charset, size, limitSize, DiskFileUpload.FileBaseDirectory, DiskFileUpload.DeleteOnExitTemporaryFile)
        {
        }

        public MixedFileUpload(string name, string fileName, string contentType,
            string contentTransferEncoding, Encoding charset, long size,
            long limitSize, string baseDir, bool deleteOnExit)
        {
            _limitSize = limitSize;
            if (size > _limitSize)
            {
                _fileUpload = new DiskFileUpload(name, fileName, contentType,
                    contentTransferEncoding, charset, size);
            }
            else
            {
                _fileUpload = new MemoryFileUpload(name, fileName, contentType,
                    contentTransferEncoding, charset, size);
            }
            _definedSize = size;
            _baseDir = baseDir;
            _deleteOnExit = deleteOnExit;
        }

        public long MaxSize
        {
            get => _maxSize;
            set
            {
                _maxSize = value;
                _fileUpload.MaxSize = value;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public void CheckSize(long newSize)
        {
            if (_maxSize >= 0 && newSize > _maxSize)
            {
                ThrowHelper.ThrowIOException_CheckSize(DataType);
            }
        }

        public void AddContent(IByteBuffer buffer, bool last)
        {
            if (_fileUpload is MemoryFileUpload)
            {
                CheckSize(_fileUpload.Length + buffer.ReadableBytes);
                if (_fileUpload.Length + buffer.ReadableBytes > _limitSize)
                {
                    var diskFileUpload = new DiskFileUpload(
                        _fileUpload.Name, _fileUpload.FileName,
                        _fileUpload.ContentType,
                        _fileUpload.ContentTransferEncoding, _fileUpload.Charset,
                        _definedSize, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };
                    IByteBuffer data = _fileUpload.GetByteBuffer();
                    if (data is object && data.IsReadable())
                    {
                        diskFileUpload.AddContent((IByteBuffer)data.Retain(), false);
                    }
                    // release old upload
                    _ = _fileUpload.Release();

                    _fileUpload = diskFileUpload;
                }
            }
            _fileUpload.AddContent(buffer, last);
        }

        public void Delete() => _fileUpload.Delete();

        public byte[] GetBytes() => _fileUpload.GetBytes();

        public IByteBuffer GetByteBuffer() => _fileUpload.GetByteBuffer();

        public Encoding Charset
        {
            get => _fileUpload.Charset;
            set => _fileUpload.Charset = value;
        }

        public string ContentType
        {
            get => _fileUpload.ContentType;
            set => _fileUpload.ContentType = value;
        }

        public string ContentTransferEncoding
        {
            get => _fileUpload.ContentTransferEncoding;
            set => _fileUpload.ContentTransferEncoding = value;
        }

        public string FileName
        {
            get => _fileUpload.FileName;
            set => _fileUpload.FileName = value;
        }

        public string GetString() => _fileUpload.GetString();

        public string GetString(Encoding encoding) => _fileUpload.GetString(encoding);

        public bool IsCompleted => _fileUpload.IsCompleted;

        public bool IsInMemory => _fileUpload.IsInMemory;

        public long Length => _fileUpload.Length;

        public long DefinedLength => _fileUpload.DefinedLength;

        public bool RenameTo(FileStream destination) => _fileUpload.RenameTo(destination);

        public void SetContent(IByteBuffer buffer)
        {
            CheckSize(buffer.ReadableBytes);
            if (buffer.ReadableBytes > _limitSize)
            {
                if (_fileUpload is MemoryFileUpload memoryUpload)
                {
                    // change to Disk
                    _fileUpload = new DiskFileUpload(
                        memoryUpload.Name,
                        memoryUpload.FileName,
                        memoryUpload.ContentType,
                        memoryUpload.ContentTransferEncoding,
                        memoryUpload.Charset,
                        _definedSize, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };

                    // release old upload
                    _ = memoryUpload.Release();
                }
            }
            _fileUpload.SetContent(buffer);
        }

        public void SetContent(Stream inputStream)
        {
            if (_fileUpload is MemoryFileUpload)
            {
                IFileUpload memoryUpload = _fileUpload;
                // change to Disk
                _fileUpload = new DiskFileUpload(
                    _fileUpload.Name,
                    _fileUpload.FileName,
                    _fileUpload.ContentType,
                    _fileUpload.ContentTransferEncoding,
                    _fileUpload.Charset,
                    _definedSize, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };

                // release old upload
                _ = memoryUpload.Release();
            }
            _fileUpload.SetContent(inputStream);
        }

        public HttpDataType DataType => _fileUpload.DataType;

        public string Name => _fileUpload.Name;

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => _fileUpload.GetHashCode();

        public override bool Equals(object obj) => _fileUpload.Equals(obj);

        public int CompareTo(IInterfaceHttpData other) => _fileUpload.CompareTo(other);

        public override string ToString() => $"Mixed: {_fileUpload}";

        public IByteBuffer GetChunk(int length) => _fileUpload.GetChunk(length);

        public FileStream GetFile() => _fileUpload.GetFile();

        public IByteBufferHolder Copy() => _fileUpload.Copy();

        public IByteBufferHolder Duplicate() => _fileUpload.Duplicate();

        public IByteBufferHolder RetainedDuplicate() => _fileUpload.RetainedDuplicate();

        public IByteBufferHolder Replace(IByteBuffer content) => _fileUpload.Replace(content);

        public IByteBuffer Content => _fileUpload.Content;

        public int ReferenceCount => _fileUpload.ReferenceCount;

        public IReferenceCounted Retain()
        {
            _ = _fileUpload.Retain();
            return this;
        }

        public IReferenceCounted Retain(int increment)
        {
            _ = _fileUpload.Retain(increment);
            return this;
        }

        public IReferenceCounted Touch()
        {
            _ = _fileUpload.Touch();
            return this;
        }

        public IReferenceCounted Touch(object hint)
        {
            _ = _fileUpload.Touch(hint);
            return this;
        }

        public bool Release() => _fileUpload.Release();

        public bool Release(int decrement) => _fileUpload.Release(decrement);
    }
}
