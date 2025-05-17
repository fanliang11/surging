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

    partial class EmptyByteBuffer
    {
        public void AdvanceReader(int count) { }

        public ReadOnlyMemory<byte> UnreadMemory => ReadOnlyMemory<byte>.Empty;
        public ReadOnlyMemory<byte> GetReadableMemory(int index, int count)
        {
            _ = CheckIndex(index, count);
            return ReadOnlyMemory<byte>.Empty;
        }

        public ReadOnlySpan<byte> UnreadSpan => ReadOnlySpan<byte>.Empty;
        public ReadOnlySpan<byte> GetReadableSpan(int index, int count)
        {
            _ = CheckIndex(index, count);
            return ReadOnlySpan<byte>.Empty;
        }

        public ReadOnlySequence<byte> UnreadSequence => ReadOnlySequence<byte>.Empty;
        public ReadOnlySequence<byte> GetSequence(int index, int count)
        {
            _ = CheckIndex(index, count);
            return ReadOnlySequence<byte>.Empty;
        }

        public Memory<byte> FreeMemory => Memory<byte>.Empty;
        public Memory<byte> GetMemory(int sizeHintt = 0) => Memory<byte>.Empty;
        public Memory<byte> GetMemory(int index, int count)
        {
            _ = CheckIndex(index, count);
            return Memory<byte>.Empty;
        }

        public void Advance(int count)
        {
            _ = CheckLength(count);
        }

        public Span<byte> FreeSpan => Span<byte>.Empty;
        public Span<byte> GetSpan(int sizeHintt = 0) => Span<byte>.Empty;
        public Span<byte> GetSpan(int index, int count)
        {
            _ = CheckIndex(index, count);
            return Span<byte>.Empty;
        }

        public int GetBytes(int index, Span<byte> destination)
        {
            _ = CheckIndex(index);
            return 0;
        }
        public int GetBytes(int index, Memory<byte> destination)
        {
            _ = CheckIndex(index);
            return 0;
        }

        public int ReadBytes(Span<byte> destination) => 0;
        public int ReadBytes(Memory<byte> destination) => 0;

        public IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src) => CheckIndex(index, src.Length);
        public IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src) => CheckIndex(index, src.Length);

        public IByteBuffer WriteBytes(in ReadOnlySpan<byte> src) => CheckLength(src.Length);
        public IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src) => CheckLength(src.Length);

        public int FindIndex(int index, int count, Predicate<byte> match)
        {
            _ = CheckIndex(index, count);
            return IndexNotFound;
        }

        public int FindLastIndex(int index, int count, Predicate<byte> match)
        {
            _ = CheckIndex(index, count);
            return IndexNotFound;
        }

        public int IndexOf(int fromIndex, int toIndex, byte value)
        {
            _ = CheckIndex(fromIndex, toIndex - fromIndex);
            return IndexNotFound;
        }

        public int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            _ = CheckIndex(fromIndex, toIndex - fromIndex);
            return IndexNotFound;
        }

        public int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1)
        {
            _ = CheckIndex(fromIndex, toIndex - fromIndex);
            return IndexNotFound;
        }

        public int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2)
        {
            _ = CheckIndex(fromIndex, toIndex - fromIndex);
            return IndexNotFound;
        }

        public int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            _ = CheckIndex(fromIndex, toIndex - fromIndex);
            return IndexNotFound;
        }
    }
}
