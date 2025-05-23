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
    using System.Runtime.CompilerServices;
    using DotNetty.Common;

    /// <inheritdoc />
    /// <summary>
    ///     Abstract base class for <see cref="T:DotNetty.Buffers.IByteBufferAllocator" /> instances
    /// </summary>
    public abstract class AbstractByteBufferAllocator : IByteBufferAllocator
    {
        public const int DefaultInitialCapacity = 256;
        public const int DefaultMaxComponents = 16;
        public const int DefaultMaxCapacity = int.MaxValue;
        const int CalculateThreshold = 1048576 * 4; // 4 MiB page

        protected static IByteBuffer ToLeakAwareBuffer(IByteBuffer buf)
        {
            IResourceLeakTracker leak;
            switch (ResourceLeakDetector.Level)
            {
                case ResourceLeakDetector.DetectionLevel.Simple:
                    leak = AbstractByteBuffer.LeakDetector.Track(buf);
                    if (leak is object)
                    {
                        buf = new SimpleLeakAwareByteBuffer(buf, leak);
                    }
                    break;
                case ResourceLeakDetector.DetectionLevel.Advanced:
                case ResourceLeakDetector.DetectionLevel.Paranoid:
                    leak = AbstractByteBuffer.LeakDetector.Track(buf);
                    if (leak is object)
                    {
                        buf = new AdvancedLeakAwareByteBuffer(buf, leak);
                    }
                    break;
                case ResourceLeakDetector.DetectionLevel.Disabled:
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(); break;
            }

            return buf;
        }

        protected static CompositeByteBuffer ToLeakAwareBuffer(CompositeByteBuffer buf)
        {
            IResourceLeakTracker leak;
            switch (ResourceLeakDetector.Level)
            {
                case ResourceLeakDetector.DetectionLevel.Simple:
                    leak = AbstractByteBuffer.LeakDetector.Track(buf);
                    if (leak is object)
                    {
                        buf = new SimpleLeakAwareCompositeByteBuffer(buf, leak);
                    }
                    break;
                case ResourceLeakDetector.DetectionLevel.Advanced:
                case ResourceLeakDetector.DetectionLevel.Paranoid:
                    leak = AbstractByteBuffer.LeakDetector.Track(buf);
                    if (leak is object)
                    {
                        buf = new AdvancedLeakAwareCompositeByteBuffer(buf, leak);
                    }
                    break;
                case ResourceLeakDetector.DetectionLevel.Disabled:
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(); break;
            }

            return buf;
        }

        private readonly bool _directByDefault;
        private readonly IByteBuffer _emptyBuffer;

        protected AbstractByteBufferAllocator()
        {
            _emptyBuffer = new EmptyByteBuffer(this);
        }

        protected AbstractByteBufferAllocator(bool preferDirect)
        {
            _directByDefault = preferDirect;
            _emptyBuffer = new EmptyByteBuffer(this);
        }

        public IByteBuffer Buffer() => _directByDefault ? DirectBuffer() : HeapBuffer();

        public IByteBuffer Buffer(int initialCapacity) =>
            _directByDefault ? DirectBuffer(initialCapacity) : HeapBuffer(initialCapacity);

        public IByteBuffer Buffer(int initialCapacity, int maxCapacity) =>
            _directByDefault ? DirectBuffer(initialCapacity, maxCapacity) : HeapBuffer(initialCapacity, maxCapacity);

        public IByteBuffer HeapBuffer() => HeapBuffer(DefaultInitialCapacity, DefaultMaxCapacity);

        public IByteBuffer HeapBuffer(int initialCapacity) => HeapBuffer(initialCapacity, DefaultMaxCapacity);

        public IByteBuffer HeapBuffer(int initialCapacity, int maxCapacity)
        {
            if (0u >= (uint)initialCapacity && 0u >= (uint)maxCapacity)
            {
                return _emptyBuffer;
            }

            Validate(initialCapacity, maxCapacity);
            return NewHeapBuffer(initialCapacity, maxCapacity);
        }

        public unsafe IByteBuffer DirectBuffer() => DirectBuffer(DefaultInitialCapacity, DefaultMaxCapacity);

        public unsafe IByteBuffer DirectBuffer(int initialCapacity) => DirectBuffer(initialCapacity, DefaultMaxCapacity);

        public unsafe IByteBuffer DirectBuffer(int initialCapacity, int maxCapacity)
        {
            if (0u >= (uint)initialCapacity && 0u >= (uint)maxCapacity)
            {
                return _emptyBuffer;
            }
            Validate(initialCapacity, maxCapacity);
            return NewDirectBuffer(initialCapacity, maxCapacity);
        }

        public CompositeByteBuffer CompositeBuffer() =>
            _directByDefault ? CompositeDirectBuffer() : CompositeHeapBuffer();

        public CompositeByteBuffer CompositeBuffer(int maxComponents) =>
            _directByDefault ? CompositeDirectBuffer(maxComponents) : CompositeHeapBuffer(maxComponents);

        public CompositeByteBuffer CompositeHeapBuffer() => CompositeHeapBuffer(DefaultMaxComponents);

        public virtual CompositeByteBuffer CompositeHeapBuffer(int maxNumComponents) =>
            ToLeakAwareBuffer(new CompositeByteBuffer(this, false, maxNumComponents));

        public unsafe CompositeByteBuffer CompositeDirectBuffer() => CompositeDirectBuffer(DefaultMaxComponents);

        public unsafe virtual CompositeByteBuffer CompositeDirectBuffer(int maxNumComponents) =>
            ToLeakAwareBuffer(new CompositeByteBuffer(this, true, maxNumComponents));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void Validate(int initialCapacity, int maxCapacity)
        {
            if ((uint)initialCapacity > (uint)maxCapacity)
            {
                ThrowInvalidInitialCapacity(initialCapacity, maxCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowInvalidInitialCapacity(int initialCapacity, int maxCapacity)
        {
            if ((uint)initialCapacity > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(initialCapacity, ExceptionArgument.initialCapacity);
            }

            if (initialCapacity > maxCapacity)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_InitialCapacity(initialCapacity, maxCapacity);
            }
        }

        protected abstract IByteBuffer NewHeapBuffer(int initialCapacity, int maxCapacity);

        protected unsafe abstract IByteBuffer NewDirectBuffer(int initialCapacity, int maxCapacity);

        public abstract bool IsDirectBufferPooled { get; }

        public int CalculateNewCapacity(int minNewCapacity, int maxCapacity)
        {
            if ((uint)minNewCapacity > (uint)maxCapacity)
            {
                ThrowInvalidNewCapacity(minNewCapacity, maxCapacity);
            }

            const int Threshold = CalculateThreshold; // 4 MiB page
            if (minNewCapacity == CalculateThreshold)
            {
                return Threshold;
            }

            int newCapacity;
            // If over threshold, do not double but just increase by threshold.
            if (minNewCapacity > Threshold)
            {
                newCapacity = minNewCapacity / Threshold * Threshold;
                if (newCapacity > maxCapacity - Threshold)
                {
                    newCapacity = maxCapacity;
                }
                else
                {
                    newCapacity += Threshold;
                }

                return newCapacity;
            }

            // Not over threshold. Double up to 4 MiB, starting from 64.
            newCapacity = 64;
            while (newCapacity < minNewCapacity)
            {
                newCapacity <<= 1;
            }

            return Math.Min(newCapacity, maxCapacity);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidNewCapacity(int minNewCapacity, int maxCapacity)
        {
            if ((uint)minNewCapacity > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(minNewCapacity, ExceptionArgument.minNewCapacity);
            }
            if (minNewCapacity > maxCapacity)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MaxCapacity(minNewCapacity, maxCapacity);
            }
        }
    }
}