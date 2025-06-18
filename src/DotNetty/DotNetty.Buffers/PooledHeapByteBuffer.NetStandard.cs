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
    using System.Buffers;

    partial class PooledHeapByteBuffer
    {
        protected internal sealed override ReadOnlyMemory<byte> _GetReadableMemory(int index, int count)
        {
            return new ReadOnlyMemory<byte>(Memory, Idx(index), count);
        }

        protected internal sealed override ReadOnlySpan<byte> _GetReadableSpan(int index, int count)
        {
            return new ReadOnlySpan<byte>(Memory, Idx(index), count);
        }

        protected internal sealed override ReadOnlySequence<byte> _GetSequence(int index, int count)
        {
            return new ReadOnlySequence<byte>(Memory, Idx(index), count);
        }

        protected internal sealed override Memory<byte> _GetMemory(int index, int count)
        {
            return new Memory<byte>(Memory, Idx(index), count);
        }

        protected internal sealed override Span<byte> _GetSpan(int index, int count)
        {
            return new Span<byte>(Memory, Idx(index), count);
        }
    }
}
