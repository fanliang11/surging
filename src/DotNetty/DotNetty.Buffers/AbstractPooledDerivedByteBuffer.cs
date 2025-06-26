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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using DotNetty.Common;

    abstract class AbstractPooledDerivedByteBuffer : AbstractReferenceCountedByteBuffer
    {
        private readonly ThreadLocalPool.Handle _recyclerHandle;
        private AbstractByteBuffer _rootParent;

        // Deallocations of a pooled derived buffer should always propagate through the entire chain of derived buffers.
        // This is because each pooled derived buffer maintains its own reference count and we should respect each one.
        // If deallocations cause a release of the "root parent" then then we may prematurely release the underlying
        // content before all the derived buffers have been released.
        //
        private IByteBuffer _parent;

        protected AbstractPooledDerivedByteBuffer(ThreadLocalPool.Handle recyclerHandle) 
            : base(0)
        {
            _recyclerHandle = recyclerHandle;
        }

        // Called from within SimpleLeakAwareByteBuf and AdvancedLeakAwareByteBuffer.
        internal void Parent(IByteBuffer newParent)
        {
            Debug.Assert(newParent is SimpleLeakAwareByteBuffer);
            _parent = newParent;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public sealed override IByteBuffer Unwrap() => _rootParent;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected AbstractByteBuffer UnwrapCore() => _rootParent;

        internal T Init<T>(
            AbstractByteBuffer unwrapped, IByteBuffer wrapped, int readerIndex, int writerIndex, int maxCapacity)
            where T : AbstractPooledDerivedByteBuffer
        {
            _ = wrapped.Retain(); // Retain up front to ensure the parent is accessible before doing more work.
            _parent = wrapped;
            _rootParent = unwrapped;

            try
            {
                SetMaxCapacity(maxCapacity);
                SetIndex0(readerIndex, writerIndex); // It is assumed the bounds checking is done by the caller.
                ResetReferenceCount();
                
                wrapped = null;
                return (T)this;
            }
            finally
            {
                if (wrapped is object)
                {
                    _parent = _rootParent = null;
                    _ = wrapped.Release();
                }
            }
        }

        protected internal sealed override void Deallocate()
        {
            // We need to first store a reference to the parent before recycle this instance. This is needed as
            // otherwise it is possible that the same AbstractPooledDerivedByteBuf is again obtained and init(...) is
            // called before we actually have a chance to call release(). This leads to call release() on the wrong parent.
            IByteBuffer parentBuf = _parent;
            _recyclerHandle.Release(this);
            _ = parentBuf.Release();
        }

        public sealed override IByteBufferAllocator Allocator => Unwrap().Allocator;

        public sealed override bool IsDirect => Unwrap().IsDirect;

        public override bool IsReadOnly => Unwrap().IsReadOnly;

        public override bool HasArray => Unwrap().HasArray;

        public override byte[] Array => Unwrap().Array;

        public override bool HasMemoryAddress => Unwrap().HasMemoryAddress;

        public sealed override bool IsSingleIoBuffer => Unwrap().IsSingleIoBuffer;

        public sealed override int IoBufferCount => Unwrap().IoBufferCount;

        public override bool IsContiguous => Unwrap().IsContiguous;

        public sealed override IByteBuffer RetainedSlice()
        {
            int index = ReaderIndex;
            return base.RetainedSlice(index, WriterIndex - index);
        }

        public override IByteBuffer Slice(int index, int length)
        {
            EnsureAccessible();
            // All reference count methods should be inherited from this object (this is the "parent").
            return new PooledNonRetainedSlicedByteBuffer(this, (AbstractByteBuffer)Unwrap(), index, length);
        }

        protected IByteBuffer Duplicate0()
        {
            EnsureAccessible();
            // All reference count methods should be inherited from this object (this is the "parent").
            return new PooledNonRetainedDuplicateByteBuffer(this, (AbstractByteBuffer)Unwrap());
        }

        sealed class PooledNonRetainedDuplicateByteBuffer : UnpooledDuplicatedByteBuffer
        {
            readonly IReferenceCounted _referenceCountDelegate;

            internal PooledNonRetainedDuplicateByteBuffer(IReferenceCounted referenceCountDelegate, AbstractByteBuffer buffer)
                : base(buffer)
            {
                _referenceCountDelegate = referenceCountDelegate;
            }

            protected override int ReferenceCount0() => _referenceCountDelegate.ReferenceCount;

            protected override IByteBuffer Retain0()
            {
                _ = _referenceCountDelegate.Retain();
                return this;
            }

            protected override IByteBuffer Retain0(int increment)
            {
                _ = _referenceCountDelegate.Retain(increment);
                return this;
            }

            protected override IByteBuffer Touch0()
            {
                _ = _referenceCountDelegate.Touch();
                return this;
            }

            protected override IByteBuffer Touch0(object hint)
            {
                _ = _referenceCountDelegate.Touch(hint);
                return this;
            }

            protected override bool Release0() => _referenceCountDelegate.Release();

            protected override bool Release0(int decrement) => _referenceCountDelegate.Release(decrement);

            public override IByteBuffer Duplicate()
            {
                EnsureAccessible();
                return new PooledNonRetainedDuplicateByteBuffer(_referenceCountDelegate, this);
            }

            public override IByteBuffer RetainedDuplicate() => PooledDuplicatedByteBuffer.NewInstance(UnwrapCore(), this, ReaderIndex, WriterIndex);

            public override IByteBuffer Slice(int index, int length)
            {
                CheckIndex(index, length);
                return new PooledNonRetainedSlicedByteBuffer(_referenceCountDelegate, (AbstractByteBuffer)Unwrap(), index, length);
            }

            // Capacity is not allowed to change for a sliced ByteBuf, so length == capacity()
            public override IByteBuffer RetainedSlice() => RetainedSlice(ReaderIndex, Capacity);

            public override IByteBuffer RetainedSlice(int index, int length) => PooledSlicedByteBuffer.NewInstance(UnwrapCore(), this, index, length);
        }

        sealed class PooledNonRetainedSlicedByteBuffer : UnpooledSlicedByteBuffer
        {
            readonly IReferenceCounted _referenceCountDelegate;

            public PooledNonRetainedSlicedByteBuffer(IReferenceCounted referenceCountDelegate, AbstractByteBuffer buffer, int index, int length)
                : base(buffer, index, length)
            {
                _referenceCountDelegate = referenceCountDelegate;
            }

            protected override int ReferenceCount0() => _referenceCountDelegate.ReferenceCount;

            protected override IByteBuffer Retain0()
            {
                _ = _referenceCountDelegate.Retain();
                return this;
            }

            protected override IByteBuffer Retain0(int increment)
            {
                _ = _referenceCountDelegate.Retain(increment);
                return this;
            }

            protected override IByteBuffer Touch0()
            {
                _ = _referenceCountDelegate.Touch();
                return this;
            }

            protected override IByteBuffer Touch0(object hint)
            {
                _ = _referenceCountDelegate.Touch(hint);
                return this;
            }

            protected override bool Release0() => _referenceCountDelegate.Release();

            protected override bool Release0(int decrement) => _referenceCountDelegate.Release(decrement);

            public override IByteBuffer Duplicate()
            {
                EnsureAccessible();
                return new PooledNonRetainedDuplicateByteBuffer(_referenceCountDelegate, UnwrapCore())
                    .SetIndex(Idx(ReaderIndex), Idx(WriterIndex));
            }

            public override IByteBuffer RetainedDuplicate() => PooledDuplicatedByteBuffer.NewInstance(UnwrapCore(), this, Idx(ReaderIndex), Idx(WriterIndex));
            
            public override IByteBuffer Slice(int index, int length)
            {
                CheckIndex(index, length);
                return new PooledNonRetainedSlicedByteBuffer(_referenceCountDelegate, UnwrapCore(), Idx(index), length);
            }

            public override IByteBuffer RetainedSlice(int index, int length) => PooledSlicedByteBuffer.NewInstance(UnwrapCore(), this, Idx(index), length);
        }
    }
}
