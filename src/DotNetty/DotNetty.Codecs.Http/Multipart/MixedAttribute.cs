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
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common;

    public class MixedAttribute : IAttribute
    {
        private readonly string _baseDir;
        private readonly bool _deleteOnExit;
        private IAttribute _attribute;

        private readonly long _limitSize;
        private long _maxSize = DefaultHttpDataFactory.MaxSize;

        public MixedAttribute(string name, long limitSize)
            : this(name, limitSize, HttpConstants.DefaultEncoding)
        {
        }

        public MixedAttribute(string name, long definedSize, long limitSize)
            : this(name, definedSize, limitSize, HttpConstants.DefaultEncoding)
        {
        }

        public MixedAttribute(string name, long limitSize, Encoding contentEncoding)
            : this(name, limitSize, contentEncoding, DiskAttribute.DiskBaseDirectory, DiskAttribute.DeleteOnExitTemporaryFile)
        {
        }

        public MixedAttribute(string name, long limitSize, Encoding contentEncoding, string baseDir, bool deleteOnExit)
        {
            _limitSize = limitSize;
            _attribute = new MemoryAttribute(name, contentEncoding);
            _baseDir = baseDir;
            _deleteOnExit = deleteOnExit;
        }

        public MixedAttribute(string name, long definedSize, long limitSize, Encoding contentEncoding)
            : this(name, definedSize, limitSize, contentEncoding,
                DiskAttribute.DiskBaseDirectory, DiskAttribute.DeleteOnExitTemporaryFile)
        {
        }

        public MixedAttribute(string name, long definedSize, long limitSize, Encoding contentEncoding, string baseDir, bool deleteOnExit)
        {
            _limitSize = limitSize;
            _attribute = new MemoryAttribute(name, definedSize, contentEncoding);
            _baseDir = baseDir;
            _deleteOnExit = deleteOnExit;
        }

        public MixedAttribute(string name, string value, long limitSize)
            : this(name, value, limitSize, HttpConstants.DefaultEncoding,
                  DiskAttribute.DiskBaseDirectory, DiskFileUpload.DeleteOnExitTemporaryFile)
        {
        }

        public MixedAttribute(string name, string value, long limitSize, Encoding charset)
            : this(name, value, limitSize, charset,
                DiskAttribute.DiskBaseDirectory, DiskFileUpload.DeleteOnExitTemporaryFile)
        {
        }

        public MixedAttribute(string name, string value, long limitSize, Encoding charset, string baseDir, bool deleteOnExit)
        {
            _limitSize = limitSize;
            if (value.Length > _limitSize)
            {
                try
                {
                    _attribute = new DiskAttribute(name, value, charset, baseDir, deleteOnExit);
                }
                catch (IOException e)
                {
                    // revert to Memory mode
                    try
                    {
                        _attribute = new MemoryAttribute(name, value, charset);
                    }
                    catch (IOException)
                    {
                        throw new ArgumentException($"{name}", e);
                    }
                }
            }
            else
            {
                try
                {
                    _attribute = new MemoryAttribute(name, value, charset);
                }
                catch (IOException e)
                {
                    throw new ArgumentException($"{name}", e);
                }
            }
            _baseDir = baseDir;
            _deleteOnExit = deleteOnExit;
        }


        public long MaxSize
        {
            get => _maxSize;
            set
            {
                _maxSize = value;
                _attribute.MaxSize = _maxSize;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public void CheckSize(long newSize)
        {
            if (_maxSize >= 0 && newSize > _maxSize)
            {
                ThrowHelper.ThrowIOException_CheckSize(_maxSize);
            }
        }

        public void AddContent(IByteBuffer buffer, bool last)
        {
            if (_attribute is MemoryAttribute memoryAttribute)
            {
                CheckSize(_attribute.Length + buffer.ReadableBytes);
                if (_attribute.Length + buffer.ReadableBytes > _limitSize)
                {
                    var diskAttribute = new DiskAttribute(_attribute.Name, _attribute.DefinedLength, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };
                    var byteBuf = memoryAttribute.GetByteBuffer();
                    if (byteBuf is object)
                    {
                        diskAttribute.AddContent(byteBuf, false);
                    }
                    _attribute = diskAttribute;
                }
            }
            _attribute.AddContent(buffer, last);
        }

        public void Delete() => _attribute.Delete();

        public byte[] GetBytes() => _attribute.GetBytes();

        public IByteBuffer GetByteBuffer() => _attribute.GetByteBuffer();

        public Encoding Charset
        {
            get => _attribute.Charset;
            set => _attribute.Charset = value;
        }

        public string GetString() => _attribute.GetString();

        public string GetString(Encoding charset) => _attribute.GetString(charset);

        public bool IsCompleted => _attribute.IsCompleted;

        public bool IsInMemory => _attribute.IsInMemory;

        public long Length => _attribute.Length;

        public long DefinedLength => _attribute.DefinedLength;

        public bool RenameTo(FileStream destination) => _attribute.RenameTo(destination);

        public void SetContent(IByteBuffer buffer)
        {
            CheckSize(buffer.ReadableBytes);
            if (buffer.ReadableBytes > _limitSize)
            {
                if (_attribute is MemoryAttribute)
                {
                    // change to Disk
                    _attribute = new DiskAttribute(_attribute.Name, _attribute.DefinedLength, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };
                }
            }
            _attribute.SetContent(buffer);
        }

        public void SetContent(Stream source)
        {
            CheckSize(source.Length);
            if (source.Length > _limitSize)
            {
                if (_attribute is MemoryAttribute)
                {
                    // change to Disk
                    _attribute = new DiskAttribute(_attribute.Name, _attribute.DefinedLength, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };
                }
            }
            _attribute.SetContent(source);
        }

        public HttpDataType DataType => _attribute.DataType;

        public string Name => _attribute.Name;

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => _attribute.GetHashCode();

        public override bool Equals(object obj) => _attribute.Equals(obj);

        public int CompareTo(IInterfaceHttpData other) => _attribute.CompareTo(other);

        public override string ToString() => $"Mixed: {_attribute}";

        public string Value
        {
            get => _attribute.Value;
            set
            {
                if (value is object)
                {
                    byte[] bytes = Charset is object
                        ? Charset.GetBytes(value)
                        : HttpConstants.DefaultEncoding.GetBytes(value);
                    CheckSize(bytes.Length);
                }

                _attribute.Value = value;
            }
        }

        public IByteBuffer GetChunk(int length) => _attribute.GetChunk(length);

        public FileStream GetFile() => _attribute.GetFile();

        public IByteBufferHolder Copy() => _attribute.Copy();

        public IByteBufferHolder Duplicate() => _attribute.Duplicate();

        public IByteBufferHolder RetainedDuplicate() => _attribute.RetainedDuplicate();

        public IByteBufferHolder Replace(IByteBuffer content) => _attribute.Replace(content);

        public IByteBuffer Content => _attribute.Content;

        public int ReferenceCount => _attribute.ReferenceCount;

        public IReferenceCounted Retain()
        {
            _ = _attribute.Retain();
            return this;
        }

        public IReferenceCounted Retain(int increment)
        {
            _ = _attribute.Retain(increment);
            return this;
        }

        public IReferenceCounted Touch()
        {
            _ = _attribute.Touch();
            return this;
        }

        public IReferenceCounted Touch(object hint)
        {
            _ = _attribute.Touch(hint);
            return this;
        }

        public bool Release() => _attribute.Release();

        public bool Release(int decrement) => _attribute.Release(decrement);
    }
}
