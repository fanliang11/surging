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

    partial class ReadOnlyByteBuffer
    {
        public override void AdvanceReader(int count) => Unwrap().AdvanceReader(count);
        public override ReadOnlyMemory<byte> UnreadMemory => Unwrap().UnreadMemory;
        public override ReadOnlyMemory<byte> GetReadableMemory(int index, int count)
        {
            return Unwrap().GetReadableMemory(index, count);
        }

        protected internal override ReadOnlyMemory<byte> _GetReadableMemory(int index, int count)
        {
            return Unwrap().GetReadableMemory(index, count);
        }


        public override ReadOnlySpan<byte> UnreadSpan => Unwrap().UnreadSpan;
        public override ReadOnlySpan<byte> GetReadableSpan(int index, int count)
        {
            return Unwrap().GetReadableSpan(index, count);
        }

        protected internal override ReadOnlySpan<byte> _GetReadableSpan(int index, int count)
        {
            return Unwrap().GetReadableSpan(index, count);
        }

        public override ReadOnlySequence<byte> UnreadSequence => Unwrap().UnreadSequence;
        public override ReadOnlySequence<byte> GetSequence(int index, int count)
        {
            return Unwrap().GetSequence(index, count);
        }

        protected internal override ReadOnlySequence<byte> _GetSequence(int index, int count)
        {
            return Unwrap().GetSequence(index, count);
        }


        public override int GetBytes(int index, Memory<byte> destination)
        {
            return Unwrap().GetBytes(index, destination);
        }

        public override int GetBytes(int index, Span<byte> destination)
        {
            return Unwrap().GetBytes(index, destination);
        }

        public override int FindIndex(int index, int count, Predicate<byte> match)
        {
            return Unwrap().FindIndex(index, count, match);
        }

        public override int FindLastIndex(int index, int count, Predicate<byte> match)
        {
            return Unwrap().FindLastIndex(index, count, match);
        }

        public override int IndexOf(int fromIndex, int toIndex, byte value)
        {
            return Unwrap().IndexOf(fromIndex, toIndex, value);
        }

        public override int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            return Unwrap().IndexOf(fromIndex, toIndex, values);
        }

        public override int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1)
        {
            return Unwrap().IndexOfAny(fromIndex, toIndex, value0, value1);
        }

        public override int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2)
        {
            return Unwrap().IndexOfAny(fromIndex, toIndex, value0, value1, value2);
        }

        public override int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            return Unwrap().IndexOfAny(fromIndex, toIndex, values);
        }

        public override void Advance(int count) => throw ThrowHelper.GetReadOnlyBufferException();


        public override Memory<byte> FreeMemory => throw ThrowHelper.GetReadOnlyBufferException();
        public override Memory<byte> GetMemory(int sizeHintt = 0)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
        public override Memory<byte> GetMemory(int index, int count)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
        protected internal override Memory<byte> _GetMemory(int index, int count)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }


        public override Span<byte> FreeSpan => throw ThrowHelper.GetReadOnlyBufferException();
        public override Span<byte> GetSpan(int sizeHintt = 0)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
        public override Span<byte> GetSpan(int index, int count)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
        protected internal override Span<byte> _GetSpan(int index, int count)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }


        public override IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
        public override IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer WriteBytes(in ReadOnlySpan<byte> src)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
        public override IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }
    }
}
