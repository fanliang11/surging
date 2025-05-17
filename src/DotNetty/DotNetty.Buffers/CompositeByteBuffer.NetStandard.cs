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
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    public partial class CompositeByteBuffer
    {
        protected internal override ReadOnlyMemory<byte> _GetReadableMemory(int index, int count)
        {
            if (0u >= (uint)count || _componentCount == 0) { return ReadOnlyMemory<byte>.Empty; }

            var buffers = GetSequence(index, count);
            if (buffers.IsSingleSegment) { return buffers.First; }

            var merged = buffers.ToArray();
            return merged;
        }

        protected internal override ReadOnlySpan<byte> _GetReadableSpan(int index, int count)
        {
            if (0u >= (uint)count || _componentCount == 0) { return ReadOnlySpan<byte>.Empty; }

            var buffers = GetSequence(index, count);
            if (buffers.IsSingleSegment) { return buffers.First.Span; }

            var merged = buffers.ToArray();
            return merged;
        }

        protected internal override ReadOnlySequence<byte> _GetSequence(int index, int count)
        {
            if (0u >= (uint)count) { return ReadOnlySequence<byte>.Empty; }

            int i = ToComponentIndex0(index);
            if (i == ToComponentIndex0(index + count - 1))
            {
                ComponentEntry c = _components[i];
                return c.Buffer.GetSequence(c.Idx(index), count);
            }
            var buffers = ThreadLocalList<ReadOnlyMemory<byte>>.NewInstance(_componentCount);
            try
            {
                while (count > 0)
                {
                    ComponentEntry c = _components[i];
                    IByteBuffer s = c.Buffer;
                    int localLength = Math.Min(count, c.EndOffset - index);
                    switch (s.IoBufferCount)
                    {
                        case 0:
                            ThrowHelper.ThrowNotSupportedException();
                            break;
                        case 1:
                            if (s.IsSingleIoBuffer)
                            {
                                buffers.Add(s.GetReadableMemory(c.Idx(index), localLength));
                            }
                            else
                            {
                                var sequence0 = s.GetSequence(c.Idx(index), localLength);
                                foreach (var memory in sequence0)
                                {
                                    buffers.Add(memory);
                                }
                            }
                            break;
                        default:
                            var sequence = s.GetSequence(c.Idx(index), localLength);
                            foreach (var memory in sequence)
                            {
                                buffers.Add(memory);
                            }
                            break;
                    }

                    index += localLength;
                    count -= localLength;
                    i++;
                }

                return ReadOnlyBufferSegment.Create(buffers);
            }
            finally
            {
                buffers.Return();
            }
        }

        protected internal override Memory<byte> _GetMemory(int index, int count)
        {
            if (0u >= (uint)count) { return Memory<byte>.Empty; }

            switch (_componentCount)
            {
                case 0:
                    return Memory<byte>.Empty;
                case 1:
                    ComponentEntry c = _components[0];
                    return c.Buffer.GetMemory(index, count);
                default:
                    var idx = ToComponentIndex0(index);
                    if (idx == ToComponentIndex0(index + count - 1))
                    {
                        ComponentEntry c1 = _components[idx];
                        return c1.Buffer.GetMemory(c1.Idx(index), count);
                    }
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        protected internal override Span<byte> _GetSpan(int index, int count)
        {
            if (0u >= (uint)count) { return Span<byte>.Empty; }

            switch (_componentCount)
            {
                case 0:
                    return Span<byte>.Empty;
                case 1:
                    ComponentEntry c = _components[0];
                    return c.Buffer.GetSpan(index, count);
                default:
                    var idx = ToComponentIndex0(index);
                    if (idx == ToComponentIndex0(index + count - 1))
                    {
                        ComponentEntry c1 = _components[idx];
                        return c1.Buffer.GetSpan(c1.Idx(index), count);
                    }
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        protected internal override void _GetBytes(int index, Span<byte> destination, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return; }

            var srcIndex = 0;
            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.GetBytes(c.Idx(index), destination.Slice(srcIndex, localLength));
                index += localLength;
                srcIndex += localLength;
                length -= localLength;
                i++;
            }
        }

        protected internal override void _GetBytes(int index, Memory<byte> destination, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return; }

            var srcIndex = 0;
            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.GetBytes(c.Idx(index), destination.Slice(srcIndex, localLength));
                index += localLength;
                srcIndex += localLength;
                length -= localLength;
                i++;
            }
        }

        public override IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src)
        {
            var length = src.Length;
            CheckIndex(index, length);
            if (0u >= (uint)length) { return this; }

            var srcIndex = 0;
            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.SetBytes(c.Idx(index), src.Slice(srcIndex, localLength));
                index += localLength;
                srcIndex += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src)
        {
            var length = src.Length;
            CheckIndex(index, length);
            if (0u >= (uint)length) { return this; }

            var srcIndex = 0;
            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.SetBytes(c.Idx(index), src.Slice(srcIndex, localLength));
                index += localLength;
                srcIndex += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        protected internal override int ForEachByteAsc0(int index, int count, IByteProcessor processor)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var start = index;
            var end = index + count;

            for (int i = ToComponentIndex0(start), length = end - start; length > 0; i++)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localStart = c.Idx(start);
                int localLength = Math.Min(length, c.EndOffset - start);
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.ForEachByteAsc0(localStart, localLength, processor)
                    : s.ForEachByte(localStart, localLength, processor);
                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                start += localLength;
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int ForEachByteDesc0(int index, int count, IByteProcessor processor)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var rStart = index + count - 1;  // rStart *and* rEnd are inclusive
            var rEnd = index;

            for (int i = ToComponentIndex0(rStart), length = 1 + rStart - rEnd; length > 0; i--)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localRStart = c.Idx(length + rEnd);
                int localLength = Math.Min(length, localRStart), localIndex = localRStart - localLength;
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.ForEachByteDesc0(localIndex, localLength, processor)
                    : s.ForEachByteDesc(localIndex, localLength, processor);

                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int FindIndex0(int index, int count, Predicate<byte> match)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var start = index;
            var end = index + count;

            for (int i = ToComponentIndex0(start), length = end - start; length > 0; i++)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localStart = c.Idx(start);
                int localLength = Math.Min(length, c.EndOffset - start);
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.FindIndex0(localStart, localLength, match)
                    : s.FindIndex(localStart, localLength, match);
                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                start += localLength;
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int FindLastIndex0(int index, int count, Predicate<byte> match)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var rStart = Math.Max(index + count - 1, 0);  // rStart *and* rEnd are inclusive
            var rEnd = index;

            for (int i = ToComponentIndex0(rStart), length = 1 + rStart - rEnd; length > 0; i--)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localRStart = c.Idx(length + rEnd);
                int localLength = Math.Min(length, localRStart), localIndex = localRStart - localLength;
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.FindLastIndex0(localIndex, localLength, match)
                    : s.FindLastIndex(localIndex, localLength, match);

                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                length -= localLength;
            }
            return IndexNotFound;
        }

        internal protected override int IndexOf0(int index, int count, byte value)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var start = index;
            var end = index + count;

            for (int i = ToComponentIndex0(start), length = end - start; length > 0; i++)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localStart = c.Idx(start);
                int localLength = Math.Min(length, c.EndOffset - start);
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.IndexOf0(localStart, localLength, value)
                    : s.IndexOf(localStart, localStart + localLength - 1, value);
                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                start += localLength;
                length -= localLength;
            }
            return IndexNotFound;
        }

        internal protected override int LastIndexOf0(int index, int count, byte value)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var rStart = Math.Max(index + count - 1, 0);  // rStart *and* rEnd are inclusive
            var rEnd = index;

            for (int i = ToComponentIndex0(rStart), length = 1 + rStart - rEnd; length > 0; i--)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localRStart = c.Idx(length + rEnd);
                int localLength = Math.Min(length, localRStart), localIndex = localRStart - localLength;
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.LastIndexOf0(localIndex, localLength, value)
                    : s.IndexOf(localRStart - 1, localIndex, value);

                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int IndexOfAny0(int index, int count, byte value0, byte value1)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var start = index;
            var end = index + count;

            for (int i = ToComponentIndex0(start), length = end - start; length > 0; i++)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localStart = c.Idx(start);
                int localLength = Math.Min(length, c.EndOffset - start);
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.IndexOfAny0(localStart, localLength, value0, value1)
                    : s.IndexOfAny(localStart, localStart + localLength - 1, value0, value1);
                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                start += localLength;
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int LastIndexOfAny0(int index, int count, byte value0, byte value1)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var rStart = Math.Max(index + count - 1, 0);  // rStart *and* rEnd are inclusive
            var rEnd = index;

            for (int i = ToComponentIndex0(rStart), length = 1 + rStart - rEnd; length > 0; i--)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localRStart = c.Idx(length + rEnd);
                int localLength = Math.Min(length, localRStart), localIndex = localRStart - localLength;
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.LastIndexOfAny0(localIndex, localLength, value0, value1)
                    : s.IndexOfAny(localRStart - 1, localIndex, value0, value1);

                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int IndexOfAny0(int index, int count, byte value0, byte value1, byte value2)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var start = index;
            var end = index + count;

            for (int i = ToComponentIndex0(start), length = end - start; length > 0; i++)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localStart = c.Idx(start);
                int localLength = Math.Min(length, c.EndOffset - start);
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.IndexOfAny0(localStart, localLength, value0, value1, value2)
                    : s.IndexOfAny(localStart, localStart + localLength - 1, value0, value1, value2);
                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                start += localLength;
                length -= localLength;
            }
            return IndexNotFound;
        }

        protected internal override int LastIndexOfAny0(int index, int count, byte value0, byte value1, byte value2)
        {
            CheckIndex(index, count);
            if (0u >= (uint)count) { return IndexNotFound; }

            var rStart = Math.Max(index + count - 1, 0);  // rStart *and* rEnd are inclusive
            var rEnd = index;

            for (int i = ToComponentIndex0(rStart), length = 1 + rStart - rEnd; length > 0; i--)
            {
                ComponentEntry c = _components[i];
                if (c.Offset == c.EndOffset)
                {
                    continue; // empty
                }
                IByteBuffer s = c.Buffer;
                int localRStart = c.Idx(length + rEnd);
                int localLength = Math.Min(length, localRStart), localIndex = localRStart - localLength;
                // avoid additional checks in AbstractByteBuf case
                int result = s is AbstractByteBuffer buf
                    ? buf.LastIndexOfAny0(localIndex, localLength, value0, value1, value2)
                    : s.IndexOfAny(localRStart - 1, localIndex, value0, value1, value2);

                if ((uint)result < SharedConstants.uIndexNotFound)
                {
                    return result - c.Adjustment;
                }
                length -= localLength;
            }
            return IndexNotFound;
        }

        // TODO 无法解决边界问题，先不重写
        //protected internal override int IndexOf0(int index, int count, in ReadOnlySpan<byte> values)
        //{
        //    return base.IndexOf0(index, count, values);
        //}

        //protected internal override int LastIndexOf0(int index, int count, in ReadOnlySpan<byte> values)
        //{
        //    return base.LastIndexOf0(index, count, values);
        //}

        //protected internal override int IndexOfAny0(int index, int count, in ReadOnlySpan<byte> values)
        //{
        //    return base.IndexOfAny0(index, count, values);
        //}

        //protected internal override int LastIndexOfAny0(int index, int count, in ReadOnlySpan<byte> values)
        //{
        //    return base.LastIndexOfAny0(index, count, values);
        //}
    }
}
