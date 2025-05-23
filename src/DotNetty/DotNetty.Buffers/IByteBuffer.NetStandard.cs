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

    partial interface IByteBuffer : IBufferWriter<byte>
    {
        void AdvanceReader(int count);

        ReadOnlyMemory<byte> UnreadMemory { get; }

        ReadOnlyMemory<byte> GetReadableMemory(int index, int count);

        ReadOnlySpan<byte> UnreadSpan { get; }

        ReadOnlySpan<byte> GetReadableSpan(int index, int count);

        ReadOnlySequence<byte> UnreadSequence { get; }

        ReadOnlySequence<byte> GetSequence(int index, int count);

        Memory<byte> FreeMemory { get; }

        Memory<byte> GetMemory(int index, int count);

        Span<byte> FreeSpan { get; }

        Span<byte> GetSpan(int index, int count);

        int GetBytes(int index, Span<byte> destination);
        int GetBytes(int index, Memory<byte> destination);

        int ReadBytes(Span<byte> destination);
        int ReadBytes(Memory<byte> destination);

        IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src);
        IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src);

        IByteBuffer WriteBytes(in ReadOnlySpan<byte> src);
        IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src);

        int FindIndex(int index, int count, Predicate<byte> match);

        int FindLastIndex(int index, int count, Predicate<byte> match);

        int IndexOf(int fromIndex, int toIndex, byte value);

        int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values);

        int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1);

        int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2);

        int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values);
    }
}
