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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common.Utilities
{
    using System.Runtime.CompilerServices;
    using System.Threading;

    public abstract class AbstractReferenceCounted : IReferenceCounted
    {
        int referenceCount = 1;

        public int ReferenceCount => Volatile.Read(ref this.referenceCount);

        public IReferenceCounted Retain() => this.RetainCore(1);

        public IReferenceCounted Retain(int increment)
        {
            if ((uint)(increment - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(increment, ExceptionArgument.increment);
            }

            return this.RetainCore(increment);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected virtual IReferenceCounted RetainCore(int increment)
        {
            var currRefCnt = Volatile.Read(ref this.referenceCount);

            int nextCount = currRefCnt + increment;
            // Ensure we don't resurrect (which means the refCnt was 0) and also that we encountered an overflow.
            if (nextCount <= increment) { ThrowIllegalReferenceCountException(currRefCnt, increment); }

            var refCnt = Interlocked.CompareExchange(ref this.referenceCount, nextCount, currRefCnt);
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
                int nextCount = refCnt + increment;

                // Ensure we don't resurrect (which means the refCnt was 0) and also that we encountered an overflow.
                if (nextCount <= increment) { ThrowIllegalReferenceCountException(refCnt, increment); }

                refCnt = Interlocked.CompareExchange(ref this.referenceCount, nextCount, refCnt);
            } while (refCnt != oldRefCnt);
        }

        public IReferenceCounted Touch() => this.Touch(null);

        public abstract IReferenceCounted Touch(object hint);

        public bool Release() => this.ReleaseCore(1);

        public bool Release(int decrement)
        {
            if ((uint)(decrement - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(decrement, ExceptionArgument.decrement);
            }

            return this.ReleaseCore(decrement);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        bool ReleaseCore(int decrement)
        {
            var currRefCnt = Volatile.Read(ref this.referenceCount);
            if (currRefCnt < decrement) { ThrowIllegalReferenceCountException(currRefCnt, decrement); }

            var refCnt = Interlocked.CompareExchange(ref this.referenceCount, currRefCnt - decrement, currRefCnt);
            if (currRefCnt != refCnt) { refCnt = ReleaseSlow(decrement, refCnt); }

            if (refCnt == decrement)
            {
                this.Deallocate();
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

                if (refCnt < decrement) { ThrowIllegalReferenceCountException(refCnt, decrement); }

                refCnt = Interlocked.CompareExchange(ref this.referenceCount, refCnt - decrement, refCnt);
            } while (refCnt != oldRefCnt);

            return refCnt;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIllegalReferenceCountException(int count, int increment)
        {
            throw GetIllegalReferenceCountException();

            IllegalReferenceCountException GetIllegalReferenceCountException()
            {
                return new IllegalReferenceCountException(count, increment);
            }
        }

        protected abstract void Deallocate();
    }
}
