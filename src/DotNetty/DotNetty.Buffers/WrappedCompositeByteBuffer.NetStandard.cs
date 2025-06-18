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

    partial class WrappedCompositeByteBuffer
    {
        public override void AdvanceReader(int count) => _wrapped.AdvanceReader(count);

        public override ReadOnlyMemory<byte> UnreadMemory => _wrapped.UnreadMemory;
        public override ReadOnlyMemory<byte> GetReadableMemory(int index, int count) => _wrapped.GetReadableMemory(index, count);
        protected internal override ReadOnlyMemory<byte> _GetReadableMemory(int index, int count) => _wrapped._GetReadableMemory(index, count);

        public override ReadOnlySpan<byte> UnreadSpan => _wrapped.UnreadSpan;
        public override ReadOnlySpan<byte> GetReadableSpan(int index, int count) => _wrapped.GetReadableSpan(index, count);
        protected internal override ReadOnlySpan<byte> _GetReadableSpan(int index, int count) => _wrapped._GetReadableSpan(index, count);

        public override ReadOnlySequence<byte> UnreadSequence => _wrapped.UnreadSequence;
        public override ReadOnlySequence<byte> GetSequence(int index, int count) => _wrapped.GetSequence(index, count);
        protected internal override ReadOnlySequence<byte> _GetSequence(int index, int count) => _wrapped._GetSequence(index, count);

        public sealed override void Advance(int count) => _wrapped.Advance(count);

        public override Memory<byte> FreeMemory => _wrapped.FreeMemory;
        public override Memory<byte> GetMemory(int sizeHintt = 0) => _wrapped.GetMemory(sizeHintt);
        public override Memory<byte> GetMemory(int index, int count) => _wrapped.GetMemory(index, count);
        protected internal override Memory<byte> _GetMemory(int index, int count) => _wrapped._GetMemory(index, count);

        public override Span<byte> FreeSpan => _wrapped.FreeSpan;
        public override Span<byte> GetSpan(int sizeHintt = 0) => _wrapped.GetSpan(sizeHintt);
        public override Span<byte> GetSpan(int index, int count) => _wrapped.GetSpan(index, count);
        protected internal override Span<byte> _GetSpan(int index, int count) => _wrapped._GetSpan(index, count);

        public override int GetBytes(int index, Span<byte> destination) => _wrapped.GetBytes(index, destination);
        public override int GetBytes(int index, Memory<byte> destination) => _wrapped.GetBytes(index, destination);

        public override int ReadBytes(Span<byte> destination) => _wrapped.ReadBytes(destination);
        public override int ReadBytes(Memory<byte> destination) => _wrapped.ReadBytes(destination);

        public override IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src) { _ = _wrapped.SetBytes(index, src); return this; }
        public override IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src) { _ = _wrapped.SetBytes(index, src); return this; }

        public override IByteBuffer WriteBytes(in ReadOnlySpan<byte> src) { _ = _wrapped.WriteBytes(src); return this; }
        public override IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src) { _ = _wrapped.WriteBytes(src); return this; }

        public override int FindIndex(int index, int count, Predicate<byte> match)
        {
            return _wrapped.FindIndex(index, count, match);
        }

        public override int FindLastIndex(int index, int count, Predicate<byte> match)
        {
            return _wrapped.FindLastIndex(index, count, match);
        }

        public override int IndexOf(int fromIndex, int toIndex, byte value) => _wrapped.IndexOf(fromIndex, toIndex, value);

        public override int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values) => _wrapped.IndexOf(fromIndex, toIndex, values);

        public override int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1)
        {
            return _wrapped.IndexOfAny(fromIndex, toIndex, value0, value1);
        }

        public override int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2)
        {
            return _wrapped.IndexOfAny(fromIndex, toIndex, value0, value1, value2);
        }

        public override int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            return _wrapped.IndexOfAny(fromIndex, toIndex, values);
        }
    }
}
