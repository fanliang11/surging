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
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Buffers
{
    using System;
    using DotNetty.Common;

    /// <inheritdoc />
    /// <summary>
    ///     Abstract base class for <see cref="T:DotNetty.Buffers.IByteBuffer" /> implementations that wrap another
    ///     <see cref="T:DotNetty.Buffers.IByteBuffer" />.
    /// </summary>
    public abstract partial class AbstractDerivedByteBuffer : AbstractByteBuffer
    {
        protected AbstractDerivedByteBuffer(int maxCapacity)
            : base(maxCapacity)
        {
        }

        public sealed override bool IsAccessible => Unwrap().IsAccessible;

        public sealed override int ReferenceCount => ReferenceCount0();

        protected virtual int ReferenceCount0() => Unwrap().ReferenceCount;

        public sealed override IReferenceCounted Retain() => Retain0();

        protected virtual IByteBuffer Retain0()
        {
            _ = Unwrap().Retain();
            return this;
        }

        public sealed override IReferenceCounted Retain(int increment) => Retain0(increment);

        protected virtual IByteBuffer Retain0(int increment)
        {
            _ = Unwrap().Retain(increment);
            return this;
        }

        public sealed override IReferenceCounted Touch() => Touch0();

        protected virtual IByteBuffer Touch0()
        {
            _ = Unwrap().Touch();
            return this;
        }

        public sealed override IReferenceCounted Touch(object hint) => Touch0(hint);

        protected virtual IByteBuffer Touch0(object hint)
        {
            _ = Unwrap().Touch(hint);
            return this;
        }

        public sealed override bool Release() => Release0();

        protected virtual bool Release0() => Unwrap().Release();

        public sealed override bool Release(int decrement) => Release0(decrement);

        protected virtual bool Release0(int decrement) => Unwrap().Release(decrement);

        public override bool IsReadOnly => Unwrap().IsReadOnly;

        public override ArraySegment<byte> GetIoBuffer(int index, int length) => Unwrap().GetIoBuffer(index, length);

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length) => Unwrap().GetIoBuffers(index, length);

        public override bool IsContiguous => Unwrap().IsContiguous;
    }
}