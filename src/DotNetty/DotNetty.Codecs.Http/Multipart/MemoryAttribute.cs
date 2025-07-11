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

    public class MemoryAttribute : AbstractMemoryHttpData, IAttribute
    {
        public MemoryAttribute(string name) 
            : this(name, HttpConstants.DefaultEncoding)
        {
        }

        public MemoryAttribute(string name, long definedSize) 
            : this(name, definedSize, HttpConstants.DefaultEncoding)
        {
        }

        public MemoryAttribute(string name, Encoding charset)
            : base(name, charset, 0)
        {
        }

        public MemoryAttribute(string name, long definedSize, Encoding charset)
            : base(name, charset, definedSize)
        {
        }

        public MemoryAttribute(string name, string value) 
            : this(name, value, HttpConstants.DefaultEncoding)
        {
        }

        public MemoryAttribute(string name, string value, Encoding contentEncoding)
            : base(name, contentEncoding, 0)
        {
            this.Value = value;
        }

        public override HttpDataType DataType => HttpDataType.Attribute;

        public string Value
        {
            get => this.GetByteBuffer().ToString(this.Charset);
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

                byte[] bytes = this.Charset.GetBytes(value);
                CheckSize(bytes.Length, this.MaxSize);
                IByteBuffer buffer = Unpooled.WrappedBuffer(bytes);
                if (this.DefinedSize > 0)
                {
                    this.DefinedSize = buffer.ReadableBytes;
                }
                this.SetContent(buffer);
            }
        }

        public override void AddContent(IByteBuffer buffer, bool last)
        {
            int localsize = buffer.ReadableBytes;
            CheckSize(this.Size + localsize, this.MaxSize);
            if (this.DefinedSize > 0 && this.DefinedSize < this.Size + localsize)
            {
                this.DefinedSize = this.Size + localsize;
            }
            base.AddContent(buffer, last);
        }

        public override int GetHashCode() => this.Name.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is IAttribute attribute)
            {
                return this.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int CompareTo(IInterfaceHttpData other)
        {
            if (other is IAttribute attr)
            {
                return this.CompareTo(attr);
            }

            return ThrowHelper.FromArgumentException_CompareToHttpData(this.DataType, other.DataType);
        }

        public int CompareTo(IAttribute attribute) => string.Compare(this.Name, attribute.Name, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => $"{this.Name} = {this.Value}";

        public override IByteBufferHolder Copy()
        {
            IByteBuffer content = this.Content;
            return this.Replace(content?.Copy());
        }

        public override IByteBufferHolder Duplicate()
        {
            IByteBuffer content = this.Content;
            return this.Replace(content?.Duplicate());
        }

        public override IByteBufferHolder RetainedDuplicate()
        {
            IByteBuffer content = this.Content;
            if (content is object)
            {
                content = content.RetainedDuplicate();
                bool success = false;
                try
                {
                    var duplicate = (IAttribute)this.Replace(content);
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
                return this.Replace(null);
            }
        }

        public override IByteBufferHolder Replace(IByteBuffer content)
        {
            var attr = new MemoryAttribute(this.Name);
            attr.Charset = this.Charset;
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