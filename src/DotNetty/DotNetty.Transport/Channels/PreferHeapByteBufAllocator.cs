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

namespace DotNetty.Transport.Channels
{
    using DotNetty.Buffers;

    public sealed class PreferHeapByteBufAllocator : IByteBufferAllocator
    {
        private readonly IByteBufferAllocator _allocator;

        public PreferHeapByteBufAllocator(IByteBufferAllocator allocator)
        {
            if (allocator is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.allocator); }
            _allocator = allocator;
        }

        public bool IsDirectBufferPooled => _allocator.IsDirectBufferPooled;

        public IByteBuffer Buffer() => _allocator.HeapBuffer();

        public IByteBuffer Buffer(int initialCapacity) => _allocator.HeapBuffer(initialCapacity);

        public IByteBuffer Buffer(int initialCapacity, int maxCapacity) => _allocator.HeapBuffer(initialCapacity, maxCapacity);

        public int CalculateNewCapacity(int minNewCapacity, int maxCapacity) => _allocator.CalculateNewCapacity(minNewCapacity, maxCapacity);

        public CompositeByteBuffer CompositeBuffer() => _allocator.CompositeHeapBuffer();

        public CompositeByteBuffer CompositeBuffer(int maxComponents) => _allocator.CompositeHeapBuffer(maxComponents);

        public CompositeByteBuffer CompositeDirectBuffer() => _allocator.CompositeDirectBuffer();

        public CompositeByteBuffer CompositeDirectBuffer(int maxComponents) => _allocator.CompositeDirectBuffer(maxComponents);

        public CompositeByteBuffer CompositeHeapBuffer() => _allocator.CompositeHeapBuffer();

        public CompositeByteBuffer CompositeHeapBuffer(int maxComponents) => _allocator.CompositeHeapBuffer();

        public IByteBuffer DirectBuffer() => _allocator.DirectBuffer();

        public IByteBuffer DirectBuffer(int initialCapacity) => _allocator.DirectBuffer(initialCapacity);

        public IByteBuffer DirectBuffer(int initialCapacity, int maxCapacity) => _allocator.DirectBuffer(initialCapacity, maxCapacity);

        public IByteBuffer HeapBuffer() => _allocator.HeapBuffer();

        public IByteBuffer HeapBuffer(int initialCapacity) => _allocator.HeapBuffer(initialCapacity);

        public IByteBuffer HeapBuffer(int initialCapacity, int maxCapacity) => _allocator.HeapBuffer(initialCapacity, maxCapacity);
    }
}
