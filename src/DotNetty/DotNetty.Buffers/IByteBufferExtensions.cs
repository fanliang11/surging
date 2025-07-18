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
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    public static partial class IByteBufferExtensions
    {
        /// <summary>
        ///     Gets a 24-bit medium integer at the specified absolute index in this buffer.
        ///     This method does not modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/>  is less than <c>0</c> or
        ///     <c>index + 3</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int GetMedium(this IByteBuffer buf, int index)
        {
            uint value = (uint)buf.GetUnsignedMedium(index);
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        /// <summary>
        ///     Gets a 24-bit medium integer at the specified absolute index in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/> is less than <c>0</c> or
        ///     <c>index + 3</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int GetMediumLE(this IByteBuffer buf, int index)
        {
            uint value = (uint)buf.GetUnsignedMediumLE(index);
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        /// <summary>
        ///     Gets an ushort at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static ushort GetUnsignedShort(this IByteBuffer buf, int index)
        {
            unchecked
            {
                return (ushort)buf.GetShort(index);
            }
        }

        /// <summary>
        ///     Gets an ushort at the specified absolute <paramref name="index" /> in this buffer 
        ///     in Little Endian Byte Order. This method does not modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static ushort GetUnsignedShortLE(this IByteBuffer buf, int index)
        {
            unchecked
            {
                return (ushort)buf.GetShortLE(index);
            }
        }

        /// <summary>
        ///     Gets an unsigned integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static uint GetUnsignedInt(this IByteBuffer buf, int index)
        {
            unchecked
            {
                return (uint)(buf.GetInt(index));
            }
        }

        /// <summary>
        ///     Gets an unsigned integer at the specified absolute <paramref name="index" /> in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static uint GetUnsignedIntLE(this IByteBuffer buf, int index)
        {
            unchecked
            {
                return (uint)buf.GetIntLE(index);
            }
        }

        /// <summary>
        ///     Gets a char at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char GetChar(this IByteBuffer buf, int index) => Convert.ToChar(buf.GetShort(index));

        /// <summary>
        ///     Gets a float at the specified absolute <paramref name="index"/> in this buffer.
        ///     This method does not modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static float GetFloat(this IByteBuffer buf, int index) => ByteBufferUtil.Int32BitsToSingle(buf.GetInt(index));

        /// <summary>
        ///     Gets a float at the specified absolute <paramref name="index"/> in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static float GetFloatLE(this IByteBuffer buf, int index) => ByteBufferUtil.Int32BitsToSingle(buf.GetIntLE(index));

        /// <summary>
        ///     Gets a double at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static double GetDouble(this IByteBuffer buf, int index) => BitConverter.Int64BitsToDouble(buf.GetLong(index));

        /// <summary>
        ///     Gets a double at the specified absolute <paramref name="index" /> in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static double GetDoubleLE(this IByteBuffer buf, int index) => BitConverter.Int64BitsToDouble(buf.GetLongLE(index));

        /// <summary>
        ///     Transfers this buffers data to the specified <paramref name="destination" /> buffer starting at the specified
        ///     absolute <paramref name="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer GetBytes(this IByteBuffer buf, int index, IByteBuffer destination)
            => buf.GetBytes(index, destination, destination.WritableBytes);

        /// <summary>
        ///     Transfers this buffers data to the specified <paramref name="destination" /> buffer starting at the specified
        ///     absolute <paramref name="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer GetBytes(this IByteBuffer buf, int index, IByteBuffer destination, int length)
        {
            var writerIdx = destination.WriterIndex;
            _ = buf.GetBytes(index, destination, writerIdx, length);
            _ = destination.SetWriterIndex(writerIdx + length);
            return buf;
        }

        /// <summary>
        ///     Gets an unsigned short at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the <see cref="IByteBuffer.ReaderIndex" />
        ///     by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>2</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static ushort ReadUnsignedShort(this IByteBuffer buf)
        {
            unchecked
            {
                return (ushort)(buf.ReadShort());
            }
        }

        /// <summary>
        ///     Gets an unsigned short at the current <see cref="IByteBuffer.ReaderIndex" /> in the Little Endian Byte Order and 
        ///     increases the <see cref="IByteBuffer.ReaderIndex" /> by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>2</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static ushort ReadUnsignedShortLE(this IByteBuffer buf)
        {
            unchecked
            {
                return (ushort)buf.ReadShortLE();
            }
        }

        /// <summary>
        ///     Gets an unsigned integer at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the <see cref="IByteBuffer.ReaderIndex" />
        ///     by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>4</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static uint ReadUnsignedInt(this IByteBuffer buf)
        {
            unchecked
            {
                return (uint)(buf.ReadInt());
            }
        }

        /// <summary>
        ///     Gets an unsigned integer at the current <see cref="IByteBuffer.ReaderIndex" /> in the Little Endian Byte Order and
        ///     increases the <see cref="IByteBuffer.ReaderIndex" /> by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>4</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static uint ReadUnsignedIntLE(this IByteBuffer buf)
        {
            unchecked
            {
                return (uint)buf.ReadIntLE();
            }
        }

        /// <summary>
        ///     Gets a 2-byte UTF-16 character at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the
        ///     <see cref="IByteBuffer.ReaderIndex" />
        ///     by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>2</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char ReadChar(this IByteBuffer buf) => (char)buf.ReadShort();

        /// <summary>
        ///     Gets an 4-byte Decimaling integer at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the
        ///     <see cref="IByteBuffer.ReaderIndex" />
        ///     by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>4</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static float ReadFloat(this IByteBuffer buf) => ByteBufferUtil.Int32BitsToSingle(buf.ReadInt());

        /// <summary>
        ///     Gets an 4-byte Decimaling integer at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the
        ///     <see cref="IByteBuffer.ReaderIndex" /> by <c>4</c> in this buffer in Little Endian Byte Order.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>4</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static float ReadFloatLE(this IByteBuffer buf) => ByteBufferUtil.Int32BitsToSingle(buf.ReadIntLE());

        /// <summary>
        ///     Gets an 8-byte Decimaling integer at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the
        ///     <see cref="IByteBuffer.ReaderIndex" />
        ///     by <c>8</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>8</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static double ReadDouble(this IByteBuffer buf) => BitConverter.Int64BitsToDouble(buf.ReadLong());

        /// <summary>
        ///     Gets an 8-byte Decimaling integer at the current <see cref="IByteBuffer.ReaderIndex" /> and increases the
        ///     <see cref="IByteBuffer.ReaderIndex" /> by <c>8</c> in this buffer in Little Endian Byte Order.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="IByteBuffer.ReadableBytes" /> is less than <c>8</c></exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static double ReadDoubleLE(this IByteBuffer buf) => BitConverter.Int64BitsToDouble(buf.ReadLongLE());

        /// <summary>
        ///     Transfers bytes from this buffer's data into the specified destination buffer
        ///     starting at the curent <see cref="IByteBuffer.ReaderIndex" /> until the destination becomes
        ///     non-writable and increases the <see cref="IByteBuffer.ReaderIndex" /> by the number of transferred bytes.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if <c>destination.<see cref="IByteBuffer.WritableBytes" /></c> is greater than
        ///     <see cref="IByteBuffer.ReadableBytes" />.
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer ReadBytes(this IByteBuffer buf, IByteBuffer dst)
        {
            return buf.ReadBytes(dst, dst.WritableBytes);
        }

        /// <summary>
        ///     Sets the specified unsigned short at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetUnsignedShort(this IByteBuffer buf, int index, ushort value)
        {
            return buf.SetShort(index, unchecked((short)value));
        }

        /// <summary>
        ///     Sets the specified unsigned short at the specified absolute <paramref name="index" /> in this buffer
        ///     in the Little Endian Byte Order. This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetUnsignedShortLE(this IByteBuffer buf, int index, ushort value)
        {
            unchecked
            {
                _ = buf.SetShortLE(index, (short)value);
            }
            return buf;
        }

        /// <summary>
        ///     Sets the specified UTF-16 char at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetChar(this IByteBuffer buf, int index, char value)
        {
            return buf.SetShort(index, value);
        }

        /// <summary>
        ///     Sets the specified unsigned integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetUnsignedInt(this IByteBuffer buf, int index, uint value)
        {
            return buf.SetInt(index, unchecked((int)value));
        }

        /// <summary>
        ///     Sets the specified unsigned integer at the specified absolute <paramref name="index" /> in this buffer
        ///     in the Little Endian Byte Order. This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or 
        ///     <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetUnsignedIntLE(this IByteBuffer buf, int index, uint value)
        {
            return buf.SetIntLE(index, unchecked((int)value));
        }

        /// <summary>
        ///     Sets the specified float at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetFloat(this IByteBuffer buf, int index, float value)
        {
            return buf.SetInt(index, ByteBufferUtil.SingleToInt32Bits(value));
        }

        /// <summary>
        ///     Sets the specified float at the specified absolute <paramref name="index" /> in this buffer
        ///     in Little Endian Byte Order. This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetFloatLE(this IByteBuffer buf, int index, float value)
        {
            return buf.SetIntLE(index, ByteBufferUtil.SingleToInt32Bits(value));
        }

        /// <summary>
        ///     Sets the specified double at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetDouble(this IByteBuffer buf, int index, double value)
        {
            return buf.SetLong(index, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        ///     Sets the specified float at the specified absolute <paramref name="index" /> in this buffer
        ///     in Little Endian Byte Order. This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> 
        ///     or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetDoubleLE(this IByteBuffer buf, int index, double value)
        {
            return buf.SetLongLE(index, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        ///     Transfers the <paramref name="src" /> byte buffer's contents starting at the specified absolute <paramref name="index" />.
        ///     This method does not directly modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c><paramref name="index"/> + <paramref name="src"/>.ReadableBytes</c> greater than <see cref="IByteBuffer.Capacity" />
        /// </exception>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer SetBytes(this IByteBuffer buf, int index, IByteBuffer src)
        {
            return buf.SetBytes(index, src, src.ReadableBytes);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteUnsignedShort(this IByteBuffer buf, ushort value)
        {
            return buf.WriteShort(unchecked((short)value));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteUnsignedShortLE(this IByteBuffer buf, ushort value)
        {
            return buf.WriteShortLE(unchecked((short)value));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteChar(this IByteBuffer buf, char value)
        {
            return buf.WriteShort(value);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteFloat(this IByteBuffer buf, float value)
        {
            return buf.WriteInt(ByteBufferUtil.SingleToInt32Bits(value));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteFloatLE(this IByteBuffer buf, float value)
        {
            return buf.WriteIntLE(ByteBufferUtil.SingleToInt32Bits(value));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteDouble(this IByteBuffer buf, double value)
        {
            return buf.WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteDoubleLE(this IByteBuffer buf, double value)
        {
            return buf.WriteLongLE(BitConverter.DoubleToInt64Bits(value));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer WriteBytes(this IByteBuffer buf, IByteBuffer src)
        {
            return buf.WriteBytes(src, src.ReadableBytes);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static Task WriteBytesAsync(this IByteBuffer buf, Stream stream, int length) => buf.WriteBytesAsync(stream, length, CancellationToken.None);

        /// <summary>
        ///     Returns a copy of this buffer's readable bytes. Modifying the content of the 
        ///     returned buffer or this buffer does not affect each other at all.This method is 
        ///     identical to {@code buf.copy(buf.readerIndex(), buf.readableBytes())}.
        ///     This method does not modify {@code readerIndex} or {@code writerIndex} of this buffer.
        ///</summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static IByteBuffer Copy(this IByteBuffer buf)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.Copy();
            }
            return buf.Copy(buf.ReaderIndex, buf.ReadableBytes);
        }

        /// <summary>
        ///     Exposes this buffer's readable bytes as an <see cref="ArraySegment{T}" /> of <see cref="Byte" />. Returned segment
        ///     shares the content with this buffer. This method is identical
        ///     to <c>buf.GetIoBuffer(buf.ReaderIndex, buf.ReadableBytes)</c>. This method does not
        ///     modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.  Please note that the
        ///     returned segment will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content as <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IByteBuffer.IoBufferCount" />
        /// <seealso cref="GetIoBuffers(IByteBuffer)" />
        /// <seealso cref="IByteBuffer.GetIoBuffers(int,int)" />
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static ArraySegment<byte> GetIoBuffer(this IByteBuffer buf)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.GetIoBuffer();
            }
            return buf.GetIoBuffer(buf.ReaderIndex, buf.ReadableBytes);
        }

        /// <summary>
        ///     Exposes this buffer's readable bytes as an array of <see cref="ArraySegment{T}" /> of <see cref="Byte" />. Returned
        ///     segments
        ///     share the content with this buffer. This method does not
        ///     modify <see cref="IByteBuffer.ReaderIndex" /> or <see cref="IByteBuffer.WriterIndex" /> of this buffer.  Please note that
        ///     returned segments will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content with <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IByteBuffer.IoBufferCount" />
        /// <seealso cref="GetIoBuffer(IByteBuffer)" />
        /// <seealso cref="IByteBuffer.GetIoBuffer(int,int)" />
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static ArraySegment<byte>[] GetIoBuffers(this IByteBuffer buf)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.GetIoBuffers();
            }
            return buf.GetIoBuffers(buf.ReaderIndex, buf.ReadableBytes);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int IndexOf(this IByteBuffer buf, byte value)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.IndexOf(value);
            }
            return buf.IndexOf(buf.ReaderIndex, buf.WriterIndex, value);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int BytesBefore(this IByteBuffer buf, byte value)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.BytesBefore(value);
            }
            return buf.BytesBefore(buf.ReaderIndex, buf.ReadableBytes, value);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int BytesBefore(this IByteBuffer buf, int length, byte value)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.BytesBefore(length, value);
            }
            if ((uint)length > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException(); }
            return buf.BytesBefore(buf.ReaderIndex, length, value);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int BytesBefore(this IByteBuffer buf, int index, int length, byte value)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.BytesBefore(index, length, value);
            }
            int endIndex = buf.IndexOf(index, index + length, value);
            if ((uint)endIndex > SharedConstants.TooBigOrNegative) { return SharedConstants.IndexNotFound; }
            return endIndex - index;
        }


        /// <summary>
        ///     Iterates over the readable bytes of this buffer with the specified <c>processor</c> in ascending order.
        /// </summary>
        /// <returns>
        ///     <c>-1</c> if the processor iterated to or beyond the end of the readable bytes.
        ///     The last-visited index If the <see cref="IByteProcessor.Process" /> returned <c>false</c>.
        /// </returns>
        public static int ForEachByte(this IByteBuffer buf, IByteProcessor processor)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.ForEachByte(processor);
            }
            return buf.ForEachByte(buf.ReaderIndex, buf.ReadableBytes, processor);
        }

        /// <summary>
        ///     Iterates over the readable bytes of this buffer with the specified <paramref name="processor"/> in descending order.
        /// </summary>
        /// <returns>
        ///     <c>-1</c> if the processor iterated to or beyond the beginning of the readable bytes.
        ///     The last-visited index If the <see cref="IByteProcessor.Process"/> returned <c>false</c>.
        /// </returns>
        public static int ForEachByteDesc(this IByteBuffer buf, IByteProcessor processor)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.ForEachByteDesc(processor);
            }
            return buf.ForEachByteDesc(buf.ReaderIndex, buf.ReadableBytes, processor);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static string ToString(this IByteBuffer buf, Encoding encoding)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.ToString(encoding);
            }
            return buf.ToString(buf.ReaderIndex, buf.ReadableBytes, encoding);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static string ToString(this IByteBuffer buf, int index, int length, Encoding encoding)
        {
            if (buf is IByteBuffer2 buffer2)
            {
                return buffer2.ToString(index, length, encoding);
            }
            return ByteBufferUtil.DecodeString(buf, index, length, encoding);
        }
    }
}
