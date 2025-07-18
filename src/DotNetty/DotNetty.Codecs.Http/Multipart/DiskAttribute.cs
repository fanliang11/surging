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
    using System.Text;
    using DotNetty.Buffers;

    public class DiskAttribute : AbstractDiskHttpData, IAttribute
    {
        public static string DiskBaseDirectory;
        public static bool DeleteOnExitTemporaryFile = true;
        public const string FilePrefix = "Attr_";
        public const string FilePostfix = ".att";

        private readonly string _baseDir;
        private readonly bool _deleteOnExit;

        public DiskAttribute(string name)
            : this(name, HttpConstants.DefaultEncoding)
        {
        }

        public DiskAttribute(string name, string baseDir, bool deleteOnExit)
            : this(name, HttpConstants.DefaultEncoding, baseDir, deleteOnExit)
        {
        }

        public DiskAttribute(string name, long definedSize)
            : this(name, definedSize, HttpConstants.DefaultEncoding, DiskBaseDirectory, DeleteOnExitTemporaryFile)
        {
        }

        public DiskAttribute(string name, long definedSize, string baseDir, bool deleteOnExit)
            : this(name, definedSize, HttpConstants.DefaultEncoding, baseDir, deleteOnExit)
        {
        }

        public DiskAttribute(string name, Encoding charset)
            : this(name, charset, DiskBaseDirectory, DeleteOnExitTemporaryFile)
        {
        }

        public DiskAttribute(string name, Encoding charset, string baseDir, bool deleteOnExit)
            : base(name, charset, 0)
        {
            _baseDir = baseDir ?? DiskBaseDirectory;
            _deleteOnExit = deleteOnExit;
        }

        public DiskAttribute(string name, long definedSize, Encoding charset)
            : this(name, definedSize, charset, DiskBaseDirectory, DeleteOnExitTemporaryFile)
        {
        }

        public DiskAttribute(string name, long definedSize, Encoding charset, string baseDir, bool deleteOnExit)
            : base(name, charset, definedSize)
        {
            _baseDir = baseDir ?? DiskBaseDirectory;
            _deleteOnExit = deleteOnExit;
        }

        public DiskAttribute(string name, string value)
            : this(name, value, HttpConstants.DefaultEncoding)
        {
        }

        public DiskAttribute(string name, string value, Encoding charset)
            : this(name, value, charset, DiskBaseDirectory, DeleteOnExitTemporaryFile)
        {
        }

        public DiskAttribute(string name, string value, Encoding charset, string baseDir, bool deleteOnExit)
            : base(name, charset, 0) // Attribute have no default size
        {
            Value = value;
            _baseDir = baseDir ?? DiskBaseDirectory;
            _deleteOnExit = deleteOnExit;
        }

        public override HttpDataType DataType => HttpDataType.Attribute;

        public string Value
        {
            get
            {
                byte[] bytes = GetBytes();
                return Charset.GetString(bytes);
            }
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

                byte[] bytes = Charset.GetBytes(value);
                CheckSize(bytes.Length, MaxSize);
                IByteBuffer buffer = Unpooled.WrappedBuffer(bytes);
                if (DefinedSize > 0)
                {
                    DefinedSize = buffer.ReadableBytes;
                }
                SetContent(buffer);
            }
        }

        public override void AddContent(IByteBuffer buffer, bool last)
        {
            long newDefinedSize = Size + buffer.ReadableBytes;
            CheckSize(newDefinedSize, MaxSize);
            if (DefinedSize > 0 && DefinedSize < newDefinedSize)
            {
                DefinedSize = newDefinedSize;
            }
            base.AddContent(buffer, last);
        }

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is IAttribute attribute)
            {
                return string.Equals(Name, attribute.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int CompareTo(IInterfaceHttpData other)
        {
            if (other is IAttribute attr)
            {
                return CompareTo(attr);
            }

            return ThrowHelper.FromArgumentException_CompareToHttpData(DataType, other.DataType);
        }

        public int CompareTo(IAttribute attribute) => string.Compare(Name, attribute.Name, StringComparison.OrdinalIgnoreCase);

        public override string ToString()
        {
            try
            {
                return $"{Name}={Value}";
            }
            catch (IOException e)
            {
                return $"{Name}={e}";
            }
        }

        protected internal override bool DeleteOnExit => _deleteOnExit;

        protected internal override string BaseDirectory => _baseDir;

        protected override string DiskFilename => $"{Name}{Postfix}";

        protected override string Postfix => FilePostfix;

        protected override string Prefix => FilePrefix;

        public override IByteBufferHolder Copy() => Replace(Content?.Copy());

        public override IByteBufferHolder Duplicate() => Replace(Content?.Duplicate());

        public override IByteBufferHolder RetainedDuplicate()
        {
            IByteBuffer content = Content;
            if (content is object)
            {
                content = content.RetainedDuplicate();
                bool success = false;
                try
                {
                    var duplicate = (IAttribute)Replace(content);
                    success = true;
                    return duplicate;
                }
                finally
                {
                    if (!success)
                    {
                        _ = content.Release();
                    }
                }
            }
            else
            {
                return Replace(null);
            }
        }

        public override IByteBufferHolder Replace(IByteBuffer content)
        {
            var attr = new DiskAttribute(Name, _baseDir, _deleteOnExit);
            attr.Charset = Charset;
            if (content is object)
            {
                try
                {
                    attr.SetContent(content);
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowChannelException_IO(e);
                }
            }
            return attr;
        }
    }
}
