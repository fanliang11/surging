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
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    sealed class InternalAttribute : AbstractReferenceCounted, IInterfaceHttpData
    {
        readonly List<IByteBuffer> value = new List<IByteBuffer>();
        readonly Encoding charset;
        int size;

        internal InternalAttribute(Encoding charset)
        {
            this.charset = charset;
        }

        public HttpDataType DataType => HttpDataType.InternalAttribute;

        public void AddValue(string stringValue)
        {
            if (stringValue is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.stringValue); }

            IByteBuffer buf = ArrayPooled.CopiedBuffer(stringValue, this.charset);
            this.value.Add(buf);
            this.size += buf.ReadableBytes;
        }

        public void AddValue(string stringValue, int rank)
        {
            if (stringValue is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.stringValue); }

            IByteBuffer buf = ArrayPooled.CopiedBuffer(stringValue, this.charset);
            this.value[rank] = buf;
            this.size += buf.ReadableBytes;
        }

        public void SetValue(string stringValue, int rank)
        {
            if (stringValue is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.stringValue); }

            IByteBuffer buf = ArrayPooled.CopiedBuffer(stringValue, this.charset);
            IByteBuffer old = this.value[rank];
            this.value[rank] = buf;
            if (old is object)
            {
                this.size -= old.ReadableBytes;
                _ = old.Release();
            }
            this.size += buf.ReadableBytes;
        }

        public override int GetHashCode() => this.Name.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is InternalAttribute attribute)
            {
                return this.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public int CompareTo(IInterfaceHttpData other)
        {
            if (other is InternalAttribute attr)
            {
                return this.CompareTo(attr);
            }

            return ThrowHelper.FromArgumentException_CompareToHttpData(this.DataType, other.DataType);
        }

        public int CompareTo(InternalAttribute other) => string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override string ToString()
        {
            var result = StringBuilderManager.Allocate();
            foreach (IByteBuffer buf in this.value)
            {
                _ = result.Append(buf.ToString(this.charset));
            }

            return StringBuilderManager.ReturnAndFree(result);
        }

        public int Size => this.size;

        public IByteBuffer ToByteBuffer()
        {
            CompositeByteBuffer compositeBuffer = ArrayPooled.CompositeBuffer();
            _ = compositeBuffer.AddComponents(this.value);
            _ = compositeBuffer.SetWriterIndex(this.size);
            _ = compositeBuffer.SetReaderIndex(0);

            return compositeBuffer;
        }

        public string Name => nameof(InternalAttribute);

        protected override void Deallocate()
        {
            // Do nothing
        }

        protected override IReferenceCounted RetainCore(int increment)
        {
            foreach (IByteBuffer buf in this.value)
            {
                _ = buf.Retain(increment);
            }
            return this;
        }

        public override IReferenceCounted Touch(object hint)
        {
            foreach (IByteBuffer buf in this.value)
            {
                _ = buf.Touch(hint);
            }
            return this;
        }
    }
}
