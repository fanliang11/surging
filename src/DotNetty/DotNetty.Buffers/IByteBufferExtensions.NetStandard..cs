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
    using System.Runtime.CompilerServices;

    public static partial class IByteBufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<byte> GetReadableMemory(this IByteBuffer buf) => buf.UnreadMemory;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetReadableSpan(this IByteBuffer buf) => buf.UnreadSpan;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySequence<byte> GetSequence(this IByteBuffer buf) => buf.UnreadSequence;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindIndex(this IByteBuffer buf, Predicate<byte> match)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.FindIndex(match);
            }
            return buf.FindIndex(buf.ReaderIndex, buf.ReadableBytes, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindLastIndex(this IByteBuffer buf, Predicate<byte> match)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.FindLastIndex(match);
            }
            return buf.FindLastIndex(buf.ReaderIndex, buf.ReadableBytes, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this IByteBuffer buf, in ReadOnlySpan<byte> values)
        {
            return buf.IndexOf(buf.ReaderIndex, buf.WriterIndex, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this IByteBuffer buf, IByteBuffer value)
        {
            return buf.IndexOf(buf.ReaderIndex, buf.WriterIndex, value.GetReadableSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this IByteBuffer buf, IByteBuffer value, int valueIndex, int valueLength)
        {
            return buf.IndexOf(buf.ReaderIndex, buf.WriterIndex, value.GetReadableSpan(valueIndex, valueLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this IByteBuffer buf, byte value0, byte value1)
        {
            return buf.IndexOfAny(buf.ReaderIndex, buf.WriterIndex, value0, value1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this IByteBuffer buf, byte value0, byte value1, byte value2)
        {
            return buf.IndexOfAny(buf.ReaderIndex, buf.WriterIndex, value0, value1, value2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this IByteBuffer buf, in ReadOnlySpan<byte> values)
        {
            return buf.IndexOfAny(buf.ReaderIndex, buf.WriterIndex, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this IByteBuffer buf, IByteBuffer value)
        {
            return buf.IndexOfAny(buf.ReaderIndex, buf.WriterIndex, value.GetReadableSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this IByteBuffer buf, IByteBuffer value, int valueIndex, int valueLength)
        {
            return buf.IndexOfAny(buf.ReaderIndex, buf.WriterIndex, value.GetReadableSpan(valueIndex, valueLength));
        }
    }
}
