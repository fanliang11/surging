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

#pragma warning disable 420

namespace DotNetty.Buffers
{
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Common;

    public abstract class AbstractReferenceCountedByteBuffer : AbstractByteBuffer
    {
        private const int c_initialValue = 1;

        private int _referenceCount = c_initialValue;

        protected AbstractReferenceCountedByteBuffer(int maxCapacity)
            : base(maxCapacity)
        {
        }

        public override bool IsAccessible => 0u < (uint)Volatile.Read(ref _referenceCount);

        public override int ReferenceCount => Volatile.Read(ref _referenceCount);

        /// <summary>
        /// An unsafe operation intended for use by a subclass that sets the reference count of the buffer directly
        /// </summary>
        /// <param name="value"></param>
        protected internal void SetReferenceCount(int value) => Interlocked.Exchange(ref _referenceCount, value);

        /// <summary>
        /// An unsafe operation intended for use by a subclass that resets the reference count of the buffer to 1
        /// </summary>
        protected internal void ResetReferenceCount()
        {
            _ = Interlocked.Exchange(ref _referenceCount, c_initialValue);
        }

        public override IReferenceCounted Retain() => Retain0(1);

        public override IReferenceCounted Retain(int increment)
        {
            if ((uint)(increment - 1) > SharedConstants.TooBigOrNegative) // increment <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(increment, ExceptionArgument.increment);
            }

            return Retain0(increment);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        IReferenceCounted Retain0(int increment)
        {
            int currRefCnt = Volatile.Read(ref _referenceCount);

            int nextCnt = currRefCnt + increment;
            // Ensure we not resurrect (which means the refCnt was 0) and also that we encountered an overflow.
            if (nextCnt <= increment) { ThrowHelper.ThrowIllegalReferenceCountException(currRefCnt, increment); }

            var refCnt = Interlocked.CompareExchange(ref _referenceCount, nextCnt, currRefCnt);
            if (currRefCnt != refCnt) { RetainSlow(increment, refCnt); }

            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RetainSlow(int increment, int refCnt)
        {
            int oldRefCnt;
            do
            {
                oldRefCnt = refCnt;
                int nextCnt = refCnt + increment;

                // Ensure we not resurrect (which means the refCnt was 0) and also that we encountered an overflow.
                if (nextCnt <= increment) { ThrowHelper.ThrowIllegalReferenceCountException(refCnt, increment); }

                refCnt = Interlocked.CompareExchange(ref _referenceCount, nextCnt, refCnt);
            } while (refCnt != oldRefCnt);
        }

        public override IReferenceCounted Touch() => this;

        public override IReferenceCounted Touch(object hint) => this;

        public override bool Release() => Release0(1);

        public override bool Release(int decrement)
        {
            if ((uint)(decrement - 1) > SharedConstants.TooBigOrNegative) // decrement <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(decrement, ExceptionArgument.decrement);
            }

            return Release0(decrement);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        bool Release0(int decrement)
        {
            int currRefCnt = Volatile.Read(ref _referenceCount);
            if (currRefCnt < decrement) { ThrowHelper.ThrowIllegalReferenceCountException(currRefCnt, -decrement); }

            var refCnt = Interlocked.CompareExchange(ref _referenceCount, currRefCnt - decrement, currRefCnt);
            if (currRefCnt != refCnt) { refCnt = ReleaseSlow(decrement, refCnt); }

            if (refCnt == decrement)
            {
                Deallocate();
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        int ReleaseSlow(int decrement, int refCnt)
        {
            int oldRefCnt;
            do
            {
                oldRefCnt = refCnt;

                if (refCnt < decrement) { ThrowHelper.ThrowIllegalReferenceCountException(refCnt, -decrement); }

                refCnt = Interlocked.CompareExchange(ref _referenceCount, refCnt - decrement, refCnt);
            } while (refCnt != oldRefCnt);

            return refCnt;
        }

        protected internal abstract void Deallocate();
    }
}