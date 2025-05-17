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
    /// <summary>
    ///     Thread-safe interface for allocating <see cref="IByteBuffer" />/.
    /// </summary>
    public interface IByteBufferAllocator
    {
        IByteBuffer Buffer();

        IByteBuffer Buffer(int initialCapacity);

        IByteBuffer Buffer(int initialCapacity, int maxCapacity);

        IByteBuffer HeapBuffer();

        IByteBuffer HeapBuffer(int initialCapacity);

        IByteBuffer HeapBuffer(int initialCapacity, int maxCapacity);

        IByteBuffer DirectBuffer();

        IByteBuffer DirectBuffer(int initialCapacity);

        IByteBuffer DirectBuffer(int initialCapacity, int maxCapacity);

        CompositeByteBuffer CompositeBuffer();

        CompositeByteBuffer CompositeBuffer(int maxComponents);

        CompositeByteBuffer CompositeHeapBuffer();

        CompositeByteBuffer CompositeHeapBuffer(int maxComponents);

        CompositeByteBuffer CompositeDirectBuffer();

        CompositeByteBuffer CompositeDirectBuffer(int maxComponents);

        bool IsDirectBufferPooled { get; }

        int CalculateNewCapacity(int minNewCapacity, int maxCapacity);
    }
}