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

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetty.Common.Internal;

namespace DotNetty.Buffers
{
    //fanly  update 2025-05-15
    /// <summary>Utility class for managing and creating unpooled buffers</summary>
    public static class ArrayPooled
    {
        internal static readonly ArrayPooledByteBufferAllocator Allocator = ArrayPooledByteBufferAllocator.Default;

        internal static readonly ArrayPool<byte> DefaultArrayPool = ArrayPool<byte>.Shared;

        public static readonly IByteBuffer Empty = Unpooled.Empty;

        public static IByteBuffer Buffer() => Allocator.Buffer();

        public static IByteBuffer DirectBuffer() => Allocator.DirectBuffer();

        public static IByteBuffer HeapBuffer() => Allocator.HeapBuffer();

        public static IByteBuffer Buffer(int initialCapacity) => Allocator.Buffer(initialCapacity);

        public static IByteBuffer DirectBuffer(int initialCapacity) => Allocator.DirectBuffer(initialCapacity);

        public static IByteBuffer HeapBuffer(int initialCapacity) => Allocator.HeapBuffer(initialCapacity);

        public static IByteBuffer Buffer(int initialCapacity, int maxCapacity) =>
            Allocator.Buffer(initialCapacity, maxCapacity);

        public static IByteBuffer DirectBuffer(int initialCapacity, int maxCapacity) =>
            Allocator.DirectBuffer(initialCapacity, maxCapacity);

        public static IByteBuffer HeapBuffer(int initialCapacity, int maxCapacity) =>
            Allocator.HeapBuffer(initialCapacity, maxCapacity);

        /// <summary>Creates a new big-endian buffer which wraps the specified array. A modification on the
        /// specified array's content will be visible to the returned buffer.</summary>
        public static IByteBuffer WrappedBuffer(byte[] array)
        {
            if (array is null) { return Empty; }

            return 0u >= (uint)array.Length ? Empty :
                PlatformDependent.DirectBufferPreferred
                    ? ArrayPooledUnsafeDirectByteBuffer.NewInstance(Allocator, DefaultArrayPool, array, array.Length, array.Length)
                    : (IByteBuffer)ArrayPooledHeapByteBuffer.NewInstance(Allocator, DefaultArrayPool, array, array.Length, array.Length);
        }

        public static IByteBuffer WrappedBuffer(ArrayPool<byte> arrayPool, byte[] array)
        {
            if (array is null) { return Empty; }
            if (arrayPool is null) { arrayPool = DefaultArrayPool; }

            return 0u >= (uint)array.Length ? Empty :
                PlatformDependent.DirectBufferPreferred
                    ? ArrayPooledUnsafeDirectByteBuffer.NewInstance(Allocator, arrayPool, array, array.Length, array.Length)
                    : (IByteBuffer)ArrayPooledHeapByteBuffer.NewInstance(Allocator, arrayPool, array, array.Length, array.Length);
        }

        /// <summary>Creates a new big-endian buffer which wraps the sub-region of the specified array. A
        /// modification on the specified array's content will be visible to the returned buffer.</summary>
        public static IByteBuffer WrappedBuffer(byte[] array, int offset, int length)
        {
            if (array is null || 0u >= (uint)length) { return Empty; }
            if (MathUtil.IsOutOfBounds(offset, length, array.Length))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Index(offset, length, array.Length);
            }

            if (0u >= (uint)offset)
            {
                return PlatformDependent.DirectBufferPreferred
                     ? ArrayPooledUnsafeDirectByteBuffer.NewInstance(Allocator, DefaultArrayPool, array, length, array.Length)
                     : (IByteBuffer)ArrayPooledHeapByteBuffer.NewInstance(Allocator, DefaultArrayPool, array, length, array.Length);
            }

            return WrappedBuffer(array).Slice(offset, length);
        }

        public static IByteBuffer WrappedBuffer(ArrayPool<byte> arrayPool, byte[] array, int offset, int length)
        {
            if (array is null || 0u >= (uint)length) { return Empty; }
            if (arrayPool is null) { arrayPool = DefaultArrayPool; }
            if (MathUtil.IsOutOfBounds(offset, length, array.Length))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Index(offset, length, array.Length);
            }

            if (0u >= (uint)offset)
            {
                return PlatformDependent.DirectBufferPreferred
                     ? ArrayPooledUnsafeDirectByteBuffer.NewInstance(Allocator, arrayPool, array, length, array.Length)
                     : (IByteBuffer)ArrayPooledHeapByteBuffer.NewInstance(Allocator, arrayPool, array, length, array.Length);
            }

            return WrappedBuffer(arrayPool, array).Slice(offset, length);
        }

        public static IByteBuffer WrappedBuffer(in ArraySegment<byte> buffer) => WrappedBuffer(buffer.Array, buffer.Offset, buffer.Count);
        public static IByteBuffer WrappedBuffer(ArrayPool<byte> arrayPool, ArraySegment<byte> buffer) => WrappedBuffer(arrayPool, buffer.Array, buffer.Offset, buffer.Count);

        public static IByteBuffer WrappedBuffer(IList<ArraySegment<byte>> buffers)
        {
            if (buffers is null) { return Empty; }

            return WrappedBuffer(buffers.Select(_ => WrappedBuffer(_)).ToArray());
        }

        /// <summary>Creates a new buffer which wraps the specified buffer's readable bytes. A modification on
        /// the specified buffer's content will be visible to the returned buffer.</summary>
        /// <param name="buffer">The buffer to wrap. Reference count ownership of this variable is transferred to this method.</param>
        /// <returns>The readable portion of the buffer, or an empty buffer if there is no readable portion.</returns>
        public static IByteBuffer WrappedBuffer(IByteBuffer buffer)
        {
            if (buffer.IsReadable())
            {
                return buffer.Slice();
            }
            else
            {
                _ = buffer.Release();
                return Empty;
            }
        }

        /// <summary>Creates a new big-endian composite buffer which wraps the readable bytes of the specified
        /// buffers without copying them. A modification on the content of the specified buffers will
        /// be visible to the returned buffer.</summary>
        /// <param name="buffers">The buffers to wrap. Reference count ownership of all variables is transferred to this method.</param>
        /// <returns>The readable portion of the buffers. The caller is responsible for releasing this buffer.</returns>
        public static IByteBuffer WrappedBuffer(params IByteBuffer[] buffers)
        {
            if (buffers is null) { return Empty; }
            return WrappedBuffer(buffers.Length, buffers);
        }

        /// <summary>Creates a new big-endian composite buffer which wraps the readable bytes of the specified
        /// buffers without copying them. A modification on the content of the specified buffers will
        /// be visible to the returned buffer.</summary>
        /// <param name="maxNumComponents">Advisement as to how many independent buffers are allowed to exist before consolidation occurs.</param>
        /// <param name="buffers">The buffers to wrap. Reference count ownership of all variables is transferred to this method.</param>
        /// <returns>The readable portion of the buffers. The caller is responsible for releasing this buffer.</returns>
        public static IByteBuffer WrappedBuffer(int maxNumComponents, params IByteBuffer[] buffers)
        {
            if (buffers is null) { return Empty; }

            switch (buffers.Length)
            {
                case 0:
                    break;

                case 1:
                    IByteBuffer buffer = buffers[0];
                    if (buffer.IsReadable())
                        return WrappedBuffer(buffer);
                    else
                        _ = buffer.Release();
                    break;

                default:
                    for (int i = 0; i < buffers.Length; i++)
                    {
                        IByteBuffer buf = buffers[i];
                        if (buf.IsReadable())
                            return new CompositeByteBuffer(Allocator, false, maxNumComponents, buffers, i);
                        else
                            _ = buf.Release();
                    }
                    break;
            }

            return Empty;
        }

        public static CompositeByteBuffer CompositeBuffer() => CompositeBuffer(AbstractByteBufferAllocator.DefaultMaxComponents);

        public static CompositeByteBuffer CompositeBuffer(int maxNumComponents) => new CompositeByteBuffer(Allocator, false, maxNumComponents);

        /// <summary>Creates a new big-endian buffer whose content is a copy of the specified array The new
        /// buffer's <see cref="IByteBuffer.ReaderIndex"/> and <see cref="IByteBuffer.WriterIndex"/>
        /// are <c>0</c> and <see cref="Array.Length"/> respectively.</summary>
        /// <param name="array">A buffer we're going to copy.</param>
        /// <returns>The new buffer that copies the contents of array.</returns>
        public static IByteBuffer CopiedBuffer(byte[] array)
        {
            if (array is null || 0u >= (uint)array.Length) { return Empty; } 
                 var newArray = DefaultArrayPool.Rent(array.Length);
            try
            {
             
                PlatformDependent.CopyMemory(array, 0, newArray, 0, array.Length);

                return WrappedBuffer(newArray, 0, array.Length);
            }
            finally
            {
                DefaultArrayPool.Return(newArray);
            }
        }

        /// <summary>Creates a new big-endian buffer whose content is a copy of the specified array. The new
        /// buffer's <see cref="IByteBuffer.ReaderIndex"/> and <see cref="IByteBuffer.WriterIndex"/>
        /// are <c>0</c> and <see cref="Array.Length"/> respectively.</summary>
        /// <param name="array">A buffer we're going to copy.</param>
        /// <param name="offset">The index offset from which we're going to read array.</param>
        /// <param name="length">The number of bytes we're going to read from array beginning from position offset.</param>
        /// <returns>The new buffer that copies the contents of array.</returns>
        public static IByteBuffer CopiedBuffer(byte[] array, int offset, int length)
        {
            if (array is null || 0u >= (uint)length) { return Empty; }

            var copy = DefaultArrayPool.Rent(length);
            try
            {
                PlatformDependent.CopyMemory(array, offset, copy, 0, length);
                return WrappedBuffer(copy, 0, length);
            }
            finally { DefaultArrayPool.Return(copy); }
        }

        /// <summary>Creates a new big-endian buffer whose content is a copy of the specified <see
        /// cref="Array"/>. The new buffer's <see cref="IByteBuffer.ReaderIndex"/> and <see
        /// cref="IByteBuffer.WriterIndex"/> are <c>0</c> and <see cref="IByteBuffer.Capacity"/> respectively.</summary>
        /// <param name="buffer">A buffer we're going to copy.</param>
        /// <returns>The new buffer that copies the contents of buffer.</returns>
        public static IByteBuffer CopiedBuffer(IByteBuffer buffer)
        {
            if (buffer is null) { return Empty; }

            int readable = buffer.ReadableBytes;
            if (readable > 0)
            {
                IByteBuffer copy = Buffer(readable);
                _ = copy.WriteBytes(buffer, buffer.ReaderIndex, readable);
                return copy;
            }
            else
            {
                return Empty;
            }
        }

        /// <summary>Creates a new big-endian buffer whose content is a merged copy of the specified <see
        /// cref="Array"/>. The new buffer's <see cref="IByteBuffer.ReaderIndex"/> and <see
        /// cref="IByteBuffer.WriterIndex"/> are <c>0</c> and <see cref="IByteBuffer.Capacity"/> respectively.</summary>
        /// <param name="buffers">Buffers we're going to copy.</param>
        /// <returns>The new buffer that copies the contents of buffers.</returns>
        public static IByteBuffer CopiedBuffer(params IByteBuffer[] buffers)
        {
            if (buffers is null) { return Empty; }

            switch (buffers.Length)
            {
                case 0:
                    return Empty;

                case 1:
                    return CopiedBuffer(buffers[0]);
            }

            // Merge the specified buffers into one buffer.
            int length = 0;
            for (int i = 0; i < buffers.Length; i++)
            {
                int bLen = buffers[i].ReadableBytes;
                if ((uint)(bLen - 1) > SharedConstants.TooBigOrNegative) // bLen <= 0
                {
                    continue;
                }
                if (int.MaxValue - length < bLen)
                {
                    ThrowHelper.ThrowArgumentException_LenIsTooBig();
                }

                length += bLen;
            }

            if (0u >= (uint)length) { return Empty; }

            var mergedArray = DefaultArrayPool.Rent(length);
            try
            {
                for (int i = 0, j = 0; i < buffers.Length; i++)
                {
                    IByteBuffer buf = buffers[i];
                    int bLen = buf.ReadableBytes;
                    _ = buf.GetBytes(buf.ReaderIndex, mergedArray, j, bLen);
                    j += bLen;
                }

                return WrappedBuffer(mergedArray, 0, length);
            }
            finally
            {
                DefaultArrayPool.Return(mergedArray);
            }
        }

        public static IByteBuffer CopiedBuffer(char[] array, int offset, int length, Encoding encoding)
        {
            return array is null || 0u >= (uint)length ? Empty : CopiedBuffer(new string(array, offset, length), encoding);
        }

        public static IByteBuffer CopiedBuffer(string value, Encoding encoding) => ByteBufferUtil.EncodeString0(Allocator, true, value, encoding, 0);

        /// <summary>Creates a new 4-byte big-endian buffer that holds the specified 32-bit integer.</summary>
        public static IByteBuffer CopyInt(int value)
        {
            IByteBuffer buf = Buffer(4);
            _ = buf.WriteInt(value);
            return buf;
        }

        /// <summary>Create a big-endian buffer that holds a sequence of the specified 32-bit integers.</summary>
        public static IByteBuffer CopyInt(params int[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 4);
            foreach (int v in values) { _ = buffer.WriteInt(v); }

            return buffer;
        }

        /// <summary>Creates a new 2-byte big-endian buffer that holds the specified 16-bit integer.</summary>
        public static IByteBuffer CopyShort(int value)
        {
            IByteBuffer buf = Buffer(2);
            _ = buf.WriteShort(value);
            return buf;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified 16-bit integers.</summary>
        public static IByteBuffer CopyShort(params short[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 2);
            foreach (short v in values) { _ = buffer.WriteShort(v); }

            return buffer;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified 16-bit integers.</summary>
        public static IByteBuffer CopyShort(params int[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 2);
            foreach (int v in values) { _ = buffer.WriteShort(v); }
            return buffer;
        }

        /// <summary>Creates a new 3-byte big-endian buffer that holds the specified 24-bit integer.</summary>
        public static IByteBuffer CopyMedium(int value)
        {
            IByteBuffer buf = Buffer(3);
            _ = buf.WriteMedium(value);
            return buf;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified 24-bit integers.</summary>
        public static IByteBuffer CopyMedium(params int[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 3);
            foreach (int v in values) { _ = buffer.WriteMedium(v); }

            return buffer;
        }

        /// <summary>Creates a new 8-byte big-endian buffer that holds the specified 64-bit integer.</summary>
        public static IByteBuffer CopyLong(long value)
        {
            IByteBuffer buf = Buffer(8);
            _ = buf.WriteLong(value);
            return buf;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified 64-bit integers.</summary>
        public static IByteBuffer CopyLong(params long[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 8);
            foreach (long v in values) { _ = buffer.WriteLong(v); }

            return buffer;
        }

        /// <summary>Creates a new single-byte big-endian buffer that holds the specified boolean value.</summary>
        public static IByteBuffer CopyBoolean(bool value)
        {
            IByteBuffer buf = Buffer(1);
            _ = buf.WriteBoolean(value);
            return buf;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified boolean values.</summary>
        public static IByteBuffer CopyBoolean(params bool[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length);
            foreach (bool v in values) { _ = buffer.WriteBoolean(v); }

            return buffer;
        }

        /// <summary>Creates a new 4-byte big-endian buffer that holds the specified 32-bit floating point number.</summary>
        public static IByteBuffer CopyFloat(float value)
        {
            IByteBuffer buf = Buffer(4);
            _ = buf.WriteFloat(value);
            return buf;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified 32-bit floating point numbers.</summary>
        public static IByteBuffer CopyFloat(params float[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 4);
            foreach (float v in values) { _ = buffer.WriteFloat(v); }

            return buffer;
        }

        /// <summary>Creates a new 8-byte big-endian buffer that holds the specified 64-bit floating point number.</summary>
        public static IByteBuffer CopyDouble(double value)
        {
            IByteBuffer buf = Buffer(8);
            _ = buf.WriteDouble(value);
            return buf;
        }

        /// <summary>Create a new big-endian buffer that holds a sequence of the specified 64-bit floating point numbers.</summary>
        public static IByteBuffer CopyDouble(params double[] values)
        {
            if (values is null || 0u >= (uint)values.Length) { return Empty; }

            IByteBuffer buffer = Buffer(values.Length * 8);
            foreach (double v in values) { _ = buffer.WriteDouble(v); }

            return buffer;
        }

        ///// <summary>Return a unreleasable view on the given {@link ByteBuf} which will just ignore release and retain calls.</summary>
        //public static IByteBuffer UnreleasableBuffer(IByteBuffer buffer) => new UnreleasableByteBuffer(buffer);

        /// <summary>Encode the given <see cref="string" /> using the given <see cref="Encoding" /> into a new
        /// <see cref="IByteBuffer" /> which is allocated via the <see cref="IByteBufferAllocator" />.</summary>
        /// <param name="src">src The <see cref="string" /> to encode.</param>
        /// <param name="encoding">charset The specified <see cref="Encoding" /></param>
        /// <param name="extraCapacity">the extra capacity to alloc except the space for decoding.</param>
        public static IByteBuffer EncodeString(string src, Encoding encoding, int extraCapacity = 0) => ByteBufferUtil.EncodeString0(Allocator, false, src, encoding, extraCapacity);

        /// <summary>Read the given amount of bytes into a new <see cref="IByteBuffer"/> that is allocated from the <see cref="IByteBufferAllocator"/>.</summary>
        public static IByteBuffer ReadBytes(IByteBuffer buffer, int length)
        {
            bool release = true;
            IByteBuffer dst = Allocator.Buffer(length);
            try
            {
                _ = buffer.ReadBytes(dst);
                release = false;
                return dst;
            }
            finally
            {
                if (release)
                {
                    _ = dst.Release();
                }
            }
        }
    }
}