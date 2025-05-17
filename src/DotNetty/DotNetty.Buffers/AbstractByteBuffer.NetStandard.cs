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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    partial class AbstractByteBuffer : IBufferWriter<byte>
    {
        private const int c_minimumGrowthSize = 256;

        public virtual void AdvanceReader(int count)
        {
            if (count < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);

            if (0u >= (uint)count) { return; }

            var readerIdx = _readerIndex + count;
            var writerIdx = _writerIndex;
            if (CheckBounds && readerIdx > writerIdx)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_ReaderIndex(readerIdx, writerIdx);
            }
            _readerIndex = readerIdx;
        }

        public virtual ReadOnlyMemory<byte> UnreadMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureAccessible();
                return _GetReadableMemory(_readerIndex, ReadableBytes);
            }
        }

        public virtual ReadOnlyMemory<byte> GetReadableMemory(int index, int count)
        {
            CheckIndex(index, count);
            return _GetReadableMemory(index, count);
        }
        protected internal abstract ReadOnlyMemory<byte> _GetReadableMemory(int index, int count);


        public virtual ReadOnlySpan<byte> UnreadSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureAccessible();
                return _GetReadableSpan(_readerIndex, ReadableBytes);
            }
        }

        public virtual ReadOnlySpan<byte> GetReadableSpan(int index, int count)
        {
            CheckIndex(index, count);
            return _GetReadableSpan(index, count);
        }
        protected internal abstract ReadOnlySpan<byte> _GetReadableSpan(int index, int count);


        public virtual ReadOnlySequence<byte> UnreadSequence
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureAccessible();
                return _GetSequence(_readerIndex, ReadableBytes);
            }
        }

        public virtual ReadOnlySequence<byte> GetSequence(int index, int count)
        {
            CheckIndex(index, count);
            return _GetSequence(index, count);
        }
        protected internal abstract ReadOnlySequence<byte> _GetSequence(int index, int count);


        public virtual void Advance(int count)
        {
            if (count < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);

            if (0u >= (uint)count) { return; }

            var capacity = Capacity;
            var writerIdx = _writerIndex + count;
            if (CheckBounds && writerIdx > capacity) { ThrowHelper.ThrowInvalidOperationException(capacity); }

            _writerIndex = writerIdx;
        }


        public virtual Memory<byte> FreeMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureAccessible();
                return _GetMemory(_writerIndex, WritableBytes);
            }
        }

        public virtual Memory<byte> GetMemory(int sizeHintt = 0)
        {
            EnsureAccessible();
            var writerIdx = _writerIndex;
            EnsureWritable0(writerIdx, sizeHintt);
            return _GetMemory(writerIdx, WritableBytes);
        }

        public virtual Memory<byte> GetMemory(int index, int count)
        {
            CheckIndex(index, count);
            return _GetMemory(index, count);
        }
        protected internal abstract Memory<byte> _GetMemory(int index, int count);


        public virtual Span<byte> FreeSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureAccessible();
                return _GetSpan(_writerIndex, WritableBytes);
            }
        }

        public virtual Span<byte> GetSpan(int sizeHintt = 0)
        {
            EnsureAccessible();
            var writerIdx = _writerIndex;
            EnsureWritable0(writerIdx, sizeHintt);
            return _GetSpan(writerIdx, WritableBytes);
        }

        public virtual Span<byte> GetSpan(int index, int count)
        {
            CheckIndex(index, count);
            return _GetSpan(index, count);
        }
        protected internal abstract Span<byte> _GetSpan(int index, int count);


        protected internal virtual void _GetBytes(int index, Span<byte> destination, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return; }

            var selfSpan = _GetReadableSpan(index, length);
            selfSpan.CopyTo(destination);
        }

        public virtual int GetBytes(int index, Span<byte> destination)
        {
            var length = Math.Min(Capacity - index, destination.Length);
            _GetBytes(index, destination, length);
            return length;
        }
        protected internal virtual void _GetBytes(int index, Memory<byte> destination, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return; }

            var selfMemory = _GetReadableMemory(index, length);
            selfMemory.CopyTo(destination);
        }

        public virtual int GetBytes(int index, Memory<byte> destination)
        {
            var length = Math.Min(Capacity - index, destination.Length);
            _GetBytes(index, destination, length);
            return length;
        }

        public virtual int ReadBytes(Span<byte> destination)
        {
            var readerIndex = _readerIndex;
            var readableBytes = Math.Min(_writerIndex - readerIndex, destination.Length);
            if (readableBytes > 0)
            {
                _GetBytes(readerIndex, destination, readableBytes);
                _readerIndex = readerIndex + readableBytes;
            }
            return readableBytes;
        }
        public virtual int ReadBytes(Memory<byte> destination)
        {
            var readerIndex = _readerIndex;
            var readableBytes = Math.Min(_writerIndex - readerIndex, destination.Length);
            if (readableBytes > 0)
            {
                _GetBytes(readerIndex, destination, readableBytes);
                _readerIndex = readerIndex + readableBytes;
            }
            return readableBytes;
        }


        public virtual IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src)
        {
            CheckIndex(index, src.Length);
            if (src.IsEmpty) { return this; }

            var length = src.Length;
            var selfSpan = _GetSpan(index, length);
            src.CopyTo(selfSpan);
            return this;
        }
        public virtual IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src)
        {
            CheckIndex(index, src.Length);
            if (src.IsEmpty) { return this; }

            var length = src.Length;
            var selfMemory = _GetMemory(index, length);
            src.CopyTo(selfMemory);
            return this;
        }

        public virtual IByteBuffer WriteBytes(in ReadOnlySpan<byte> src)
        {
            var writerIdx = _writerIndex;
            EnsureWritable0(writerIdx, src.Length);
            _ = SetBytes(writerIdx, src);
            _writerIndex = writerIdx + src.Length;
            return this;
        }
        public virtual IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src)
        {
            var writerIdx = _writerIndex;
            EnsureWritable0(writerIdx, src.Length);
            _ = SetBytes(writerIdx, src);
            _writerIndex = writerIdx + src.Length;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal void EnsureWritable0(int writerIdx, int sizeHint)
        {
            //EnsureAccessible();
            if ((uint)(sizeHint - 1) > SharedConstants.TooBigOrNegative)
            {
                sizeHint = c_minimumGrowthSize;
            }

            int targetCapacity = writerIdx + sizeHint;
            if ((uint)targetCapacity <= (uint)Capacity) { return; }

            EnsureWritableInternal(writerIdx, sizeHint, targetCapacity);
        }

        protected sealed class ReadOnlyBufferSegment : ReadOnlySequenceSegment<byte>
        {
            public static ReadOnlySequence<byte> Create(List<ReadOnlyMemory<byte>> buffers)
            {
                switch (buffers.Count)
                {
                    case 0:
                        return ReadOnlySequence<byte>.Empty;
                    case 1:
                        return new ReadOnlySequence<byte>(buffers[0]);
                }
                ReadOnlyBufferSegment segment = null;
                ReadOnlyBufferSegment first = null;
                foreach (var buffer in buffers)
                {
                    if (buffer.Length == 0)
                        continue;
                    var newSegment = new ReadOnlyBufferSegment()
                    {
                        Memory = buffer,
                    };

                    if (segment is object)
                    {
                        segment.Next = newSegment;
                        newSegment.RunningIndex = segment.RunningIndex + segment.Memory.Length;
                    }
                    else
                    {
                        first = newSegment;
                    }

                    segment = newSegment;
                }

                if (first is null)
                {
                    return ReadOnlySequence<byte>.Empty;
                }
                if (first == segment)
                {
                    return new ReadOnlySequence<byte>(first.Memory);
                }

                return new ReadOnlySequence<byte>(first, 0, segment, segment.Memory.Length);
            }
        }


        internal protected virtual int ForEachByteAsc0(int index, int count, IByteProcessor processor)
        {
            var span = GetReadableSpan(index, count);

            var result = SpanHelpers.ForEachByte(ref MemoryMarshal.GetReference(span), processor, span.Length);

            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        internal protected virtual int ForEachByteDesc0(int index, int count, IByteProcessor processor)
        {
            var span = GetReadableSpan(index, count);

            var result = SpanHelpers.ForEachByteDesc(ref MemoryMarshal.GetReference(span), processor, span.Length);

            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }


        public virtual int FindIndex(int index, int length, Predicate<byte> match)
        {
            if (0u >= (uint)Capacity) { return IndexNotFound; }

            return FindIndex0(index, length, match);
        }
        internal protected virtual int FindIndex0(int index, int count, Predicate<byte> match)
        {
            var span = GetReadableSpan(index, count);

            var result = SpanHelpers.FindIndex(ref MemoryMarshal.GetReference(span), match, span.Length);

            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        public virtual int FindLastIndex(int index, int count, Predicate<byte> match)
        {
            if (0u >= (uint)Capacity) { return IndexNotFound; }

            return FindLastIndex0(index, count, match);
        }
        internal protected virtual int FindLastIndex0(int index, int count, Predicate<byte> match)
        {
            var span = GetReadableSpan(index, count);

            var result = SpanHelpers.FindLastIndex(ref MemoryMarshal.GetReference(span), match, span.Length);

            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }


        public virtual int IndexOf(int fromIndex, int toIndex, byte value)
        {
            if (fromIndex <= toIndex)
            {
                fromIndex = Math.Max(fromIndex, 0);
                if (fromIndex >= toIndex || 0u >= (uint)Capacity) { return IndexNotFound; }

                return IndexOf0(fromIndex, toIndex - fromIndex, value);
            }
            else
            {
                int capacity = Capacity;
                fromIndex = Math.Min(fromIndex, capacity);
                if (fromIndex < 0 || 0u >= (uint)capacity) { return IndexNotFound; }

                return LastIndexOf0(toIndex, fromIndex - toIndex, value);
            }
        }

        internal protected virtual int IndexOf0(int index, int count, byte value)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.IndexOf(value);
#else
            var result = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        internal protected virtual int LastIndexOf0(int index, int count, byte value)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.LastIndexOf(value);
#else
            var result = SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        public virtual int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            if (fromIndex <= toIndex)
            {
                fromIndex = Math.Max(fromIndex, 0);
                if (fromIndex >= toIndex || 0u >= (uint)Capacity) { return IndexNotFound; }

                return IndexOf0(fromIndex, toIndex - fromIndex, values);
            }
            else
            {
                int capacity = Capacity;
                fromIndex = Math.Min(fromIndex, capacity);
                if (fromIndex < 0 || 0u >= (uint)capacity) { return IndexNotFound; }

                return LastIndexOf0(toIndex, fromIndex - toIndex, values);
            }
        }

        internal protected virtual int IndexOf0(int index, int count, in ReadOnlySpan<byte> values)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.IndexOf(values);
#else
            var result = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        internal protected virtual int LastIndexOf0(int index, int count, in ReadOnlySpan<byte> values)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.LastIndexOf(values);
#else
            var result = SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        public virtual int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1)
        {
            if (fromIndex <= toIndex)
            {
                fromIndex = Math.Max(fromIndex, 0);
                if (fromIndex >= toIndex || 0u >= (uint)Capacity) { return IndexNotFound; }

                return IndexOfAny0(fromIndex, toIndex - fromIndex, value0, value1);
            }
            else
            {
                int capacity = Capacity;
                fromIndex = Math.Min(fromIndex, capacity);
                if (fromIndex < 0 || 0u >= (uint)capacity) { return IndexNotFound; }

                return LastIndexOfAny0(toIndex, fromIndex - toIndex, value0, value1);
            }
        }

        internal protected virtual int IndexOfAny0(int index, int count, byte value0, byte value1)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.IndexOfAny(value0, value1);
#else
            var result = SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        internal protected virtual int LastIndexOfAny0(int index, int count, byte value0, byte value1)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.LastIndexOfAny(value0, value1);
#else
            var result = SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        public virtual int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2)
        {
            if (fromIndex <= toIndex)
            {
                fromIndex = Math.Max(fromIndex, 0);
                if (fromIndex >= toIndex || 0u >= (uint)Capacity) { return IndexNotFound; }

                return IndexOfAny0(fromIndex, toIndex - fromIndex, value0, value1, value2);
            }
            else
            {
                int capacity = Capacity;
                fromIndex = Math.Min(fromIndex, capacity);
                if (fromIndex < 0 || 0u >= (uint)capacity) { return IndexNotFound; }

                return LastIndexOfAny0(toIndex, fromIndex - toIndex, value0, value1, value2);
            }
        }

        internal protected virtual int IndexOfAny0(int index, int count, byte value0, byte value1, byte value2)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.IndexOfAny(value0, value1, value2);
#else
            var result = SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        internal protected virtual int LastIndexOfAny0(int index, int count, byte value0, byte value1, byte value2)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.LastIndexOfAny(value0, value1, value2);
#else
            var result = SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        public virtual int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            if (fromIndex <= toIndex)
            {
                fromIndex = Math.Max(fromIndex, 0);
                if (fromIndex >= toIndex || 0u >= (uint)Capacity) { return IndexNotFound; }

                return IndexOfAny0(fromIndex, toIndex - fromIndex, values);
            }
            else
            {
                int capacity = Capacity;
                fromIndex = Math.Min(fromIndex, capacity);
                if (fromIndex < 0 || 0u >= (uint)capacity) { return IndexNotFound; }

                return LastIndexOfAny0(toIndex, fromIndex - toIndex, values);
            }
        }

        internal protected virtual int IndexOfAny0(int index, int count, in ReadOnlySpan<byte> values)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.IndexOfAny(values);
#else
            var result = SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }

        internal protected virtual int LastIndexOfAny0(int index, int count, in ReadOnlySpan<byte> values)
        {
            var span = GetReadableSpan(index, count);
#if NET
            var result = span.LastIndexOfAny(values);
#else
            var result = SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
#endif
            return (uint)result < SharedConstants.uIndexNotFound ? index + result : result;
        }
    }
}
