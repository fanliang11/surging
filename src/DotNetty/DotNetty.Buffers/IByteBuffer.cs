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
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    /// <summary>
    ///     Inspired by the Netty ByteBuffer implementation
    ///     (https://github.com/netty/netty/blob/master/buffer/src/main/java/io/netty/buffer/ByteBuf.java)
    ///     Provides circular-buffer-esque security around a byte array, allowing reads and writes to occur independently.
    ///     In general, the <see cref="T:DotNetty.Buffers.IByteBuffer" /> guarantees:
    ///     /// <see cref="P:DotNetty.Buffers.IByteBuffer.ReaderIndex" /> LESS THAN OR EQUAL TO <see cref="P:DotNetty.Buffers.IByteBuffer.WriterIndex" /> LESS THAN OR EQUAL TO
    ///     <see cref="P:DotNetty.Buffers.IByteBuffer.Capacity" />.
    /// </summary>
    public partial interface IByteBuffer : IReferenceCounted, IComparable<IByteBuffer>, IEquatable<IByteBuffer>
    {
        /// <summary>
        /// Returns the number of bytes (octets) this buffer can contain.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Adjusts the capacity of this buffer.  If the <paramref name="newCapacity"/> is less than the current
        /// capacity, the content of this buffer is truncated.  If the <paramref name="newCapacity"/> is greater
        /// than the current capacity, the buffer is appended with unspecified data whose length is
        /// <code>newCapacity - currentCapacity</code>
        /// </summary>
        IByteBuffer AdjustCapacity(int newCapacity);

        /// <summary>
        /// Returns the maximum allowed capacity of this buffer. This value provides an upper bound on <see cref="Capacity"/>
        /// </summary>
        int MaxCapacity { get; }

        /// <summary>
        /// Returns the <see cref="IByteBufferAllocator"/> which created this buffer.
        /// </summary>
        IByteBufferAllocator Allocator { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if this buffer is backed by an direct buffer.
        /// </summary>
        bool IsDirect { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if this buffer is read-only.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Returns a read-only version of this buffer.
        /// </summary>
        /// <returns></returns>
        IByteBuffer AsReadOnly();

        /// <summary>
        /// Returns the <see cref="ReaderIndex"/> of this buffer.
        /// </summary>
        int ReaderIndex { get; }

        /// <summary>
        /// Returns the <see cref="WriterIndex"/> of this buffer.
        /// </summary>
        int WriterIndex { get; }

        /// <summary>
        ///     Sets the <see cref="WriterIndex" /> of this buffer
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">thrown if <see cref="WriterIndex" /> exceeds the length of the buffer</exception>
        IByteBuffer SetWriterIndex(int writerIndex);

        /// <summary>
        ///     Sets the <see cref="ReaderIndex" /> of this buffer
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     thrown if <see cref="ReaderIndex" /> is greater than
        ///     <see cref="WriterIndex" /> or less than <c>0</c>.
        /// </exception>
        IByteBuffer SetReaderIndex(int readerIndex);

        /// <summary>
        ///     Sets both indexes
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     thrown if <see cref="WriterIndex" /> or <see cref="ReaderIndex" /> exceeds
        ///     the length of the buffer
        /// </exception>
        IByteBuffer SetIndex(int readerIndex, int writerIndex);

        /// <summary>
        /// Returns the number of readable bytes which is equal to
        /// <code>(this.writerIndex - this.readerIndex)</code>
        /// </summary>
        int ReadableBytes { get; }

        /// <summary>
        /// Returns the number of writable bytes which is equal to
        /// <code>(this.capacity - this.writerIndex)</code>
        /// </summary>
        int WritableBytes { get; }

        /// <summary>
        /// Returns the maximum possible number of writable bytes, which is equal to
        /// <code>(this.maxCapacity - this.writerIndex)</code>
        /// </summary>
        int MaxWritableBytes { get; }

        /// <summary>
        /// Returns the maximum number of bytes which can be written for certain without involving
        /// an internal reallocation or data-copy. The returned value will be &gt; <see cref="WritableBytes"/>
        /// and &lt; <see cref="MaxWritableBytes"/>.
        /// </summary>
        int MaxFastWritableBytes { get; }

        /// <summary>
        /// Used internally by <see cref="AbstractByteBuffer.EnsureAccessible"/> to try to guard
        /// against using the buffer after it was released (best-effort).
        /// </summary>
        bool IsAccessible { get; }

        /// <summary>
        ///     Returns true if <see cref="WriterIndex" /> - <see cref="ReaderIndex" /> is greater than <c>0</c>.
        /// </summary>
        bool IsReadable();

        /// <summary>
        ///     Is the buffer readable if and only if the buffer contains equal or more than the specified number of elements
        /// </summary>
        /// <param name="size">The number of elements we would like to read</param>
        bool IsReadable(int size);

        /// <summary>
        ///     Returns true if and only if <see cref="Capacity" /> - <see cref="WriterIndex" /> is greater than zero.
        /// </summary>
        bool IsWritable();

        /// <summary>
        ///     Returns true if and only if the buffer has enough <see cref="Capacity" /> to accomodate <paramref name="size" />
        ///     additional bytes.
        /// </summary>
        /// <param name="size">The number of additional elements we would like to write.</param>
        bool IsWritable(int size);

        /// <summary>
        ///     Sets the <see cref="WriterIndex" /> and <see cref="ReaderIndex" /> to <c>0</c>. Does not erase any of the data
        ///     written into the buffer already,
        ///     but it will overwrite that data.
        /// </summary>
        IByteBuffer Clear();

        /// <summary>
        ///     Marks the current <see cref="ReaderIndex" /> in this buffer. You can reposition the current
        ///     <see cref="ReaderIndex" />
        ///     to the marked <see cref="ReaderIndex" /> by calling <see cref="ResetReaderIndex" />.
        ///     The initial value of the marked <see cref="ReaderIndex" /> is <c>0</c>.
        /// </summary>
        IByteBuffer MarkReaderIndex();

        /// <summary>
        ///     Repositions the current <see cref="ReaderIndex" /> to the marked <see cref="ReaderIndex" /> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     is thrown if the current <see cref="WriterIndex" /> is less than the
        ///     marked <see cref="ReaderIndex" />
        /// </exception>
        IByteBuffer ResetReaderIndex();

        /// <summary>
        ///     Marks the current <see cref="WriterIndex" /> in this buffer. You can reposition the current
        ///     <see cref="WriterIndex" />
        ///     to the marked <see cref="WriterIndex" /> by calling <see cref="ResetWriterIndex" />.
        ///     The initial value of the marked <see cref="WriterIndex" /> is <c>0</c>.
        /// </summary>
        IByteBuffer MarkWriterIndex();

        /// <summary>
        ///     Repositions the current <see cref="WriterIndex" /> to the marked <see cref="WriterIndex" /> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     is thrown if the current <see cref="ReaderIndex" /> is greater than the
        ///     marked <see cref="WriterIndex" />
        /// </exception>
        IByteBuffer ResetWriterIndex();

        /// <summary>
        ///     Discards the bytes between the 0th index and <see cref="ReaderIndex" />.
        ///     It moves the bytes between <see cref="ReaderIndex" /> and <see cref="WriterIndex" /> to the 0th index,
        ///     and sets <see cref="ReaderIndex" /> and <see cref="WriterIndex" /> to <c>0</c> and
        ///     <c>oldWriterIndex - oldReaderIndex</c> respectively.
        /// </summary>
        IByteBuffer DiscardReadBytes();

        /// <summary>
        ///     Similar to <see cref="DiscardReadBytes" /> except that this method might discard
        ///     some, all, or none of read bytes depending on its internal implementation to reduce
        ///     overall memory bandwidth consumption at the cost of potentially additional memory
        ///     consumption.
        /// </summary>
        IByteBuffer DiscardSomeReadBytes();

        /// <summary>
        ///     Makes sure the number of <see cref="WritableBytes" /> is equal to or greater than
        ///     the specified value (<paramref name="minWritableBytes" />.) If there is enough writable bytes in this buffer,
        ///     the method returns with no side effect.
        /// </summary>
        /// <param name="minWritableBytes">The expected number of minimum writable bytes</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     if <see cref="WriterIndex" /> + <paramref name="minWritableBytes" /> &gt;
        ///     <see cref="MaxCapacity" />.
        /// </exception>
        IByteBuffer EnsureWritable(int minWritableBytes);

        /// <summary>
        ///     Tries to make sure the number of <see cref="WritableBytes" />
        ///     is equal to or greater than the specified value. Unlike <see cref="EnsureWritable(int)" />,
        ///     this method does not raise an exception but returns a code.
        /// </summary>
        /// <param name="minWritableBytes">the expected minimum number of writable bytes</param>
        /// <param name="force">
        ///     When <see cref="WriterIndex" /> + <c>minWritableBytes</c> > <see cref="MaxCapacity" />:
        ///     <ul>
        ///         <li><c>true</c> - the capacity of the buffer is expanded to <see cref="MaxCapacity" /></li>
        ///         <li><c>false</c> - the capacity of the buffer is unchanged</li>
        ///     </ul>
        /// </param>
        /// <returns>
        ///     <c>0</c> if the buffer has enough writable bytes, and its capacity is unchanged.
        ///     <c>1</c> if the buffer does not have enough bytes, and its capacity is unchanged.
        ///     <c>2</c> if the buffer has enough writable bytes, and its capacity has been increased.
        ///     <c>3</c> if the buffer does not have enough bytes, but its capacity has been increased to its maximum.
        /// </returns>
        int EnsureWritable(int minWritableBytes, bool force);

        /// <summary>
        ///     Gets a boolean at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        bool GetBoolean(int index);

        /// <summary>
        ///     Gets a byte at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        byte GetByte(int index);

        /// <summary>
        ///     Gets a short at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="Capacity" />
        /// </exception>
        short GetShort(int index);

        /// <summary>
        ///     Gets a short at the specified absolute <paramref name="index" /> in this buffer 
        ///     in Little Endian Byte Order. This method does not modify <see cref="ReaderIndex" /> 
        ///     or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="Capacity" />
        /// </exception>
        short GetShortLE(int index);

        /// <summary>
        ///     Gets an integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="Capacity" />
        /// </exception>
        int GetInt(int index);

        /// <summary>
        ///     Gets an integer at the specified absolute <paramref name="index" /> in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="ReaderIndex" /> 
        ///     or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="Capacity" />
        /// </exception>
        int GetIntLE(int index);

        /// <summary>
        ///     Gets a long integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="Capacity" />
        /// </exception>
        long GetLong(int index);

        /// <summary>
        ///     Gets a long integer at the specified absolute <paramref name="index" /> in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="ReaderIndex" /> or 
        ///     <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="Capacity" />
        /// </exception>
        long GetLongLE(int index);

        /// <summary>
        ///     Gets an unsigned 24-bit medium integer at the specified absolute index in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <param name="index"/> is less than <c>0</c> or
        ///     <c>index + 3</c> greater than <see cref="Capacity" />
        /// </exception>
        int GetUnsignedMedium(int index);

        /// <summary>
        ///     Gets an unsigned 24-bit medium integer at the specified absolute index in this buffer
        ///     in Little Endian Byte Order. This method does not modify <see cref="ReaderIndex" /> 
        ///     or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <param name="index"/> is less than <c>0</c> or
        ///     <c>index + 3</c> greater than <see cref="Capacity" />
        /// </exception>
        int GetUnsignedMediumLE(int index);

        /// <summary>
        ///     Transfers this buffers data to the specified <paramref name="destination" /> buffer starting at the specified
        ///     absolute <paramref name="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer GetBytes(int index, IByteBuffer destination, int dstIndex, int length);

        /// <summary>
        ///     Transfers this buffers data to the specified <paramref name="destination" /> buffer starting at the specified
        ///     absolute <paramref name="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer GetBytes(int index, byte[] destination);

        /// <summary>
        ///     Transfers this buffers data to the specified <paramref name="destination" /> buffer starting at the specified
        ///     absolute <paramref name="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer GetBytes(int index, byte[] destination, int dstIndex, int length);

        /// <summary>
        ///     Transfers this buffer's data to the specified stream starting at the
        ///     specified absolute <c>index</c>.
        /// </summary>
        /// <remarks>
        ///     This method does not modify <c>readerIndex</c> or <c>writerIndex</c> of
        ///     this buffer.
        /// </remarks>
        /// <param name="index">absolute index in this buffer to start getting bytes from</param>
        /// <param name="destination">destination stream</param>
        /// <param name="length">the number of bytes to transfer</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <c>index</c> is less than <c>0</c> or
        ///     if <c>index + length</c> is greater than
        ///     <c>this.capacity</c>
        /// </exception>
        IByteBuffer GetBytes(int index, Stream destination, int length);

        /// <summary>
        /// Gets a <see cref="ICharSequence"/> with the given length at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length">the length to read</param>
        /// <param name="encoding">that should be used</param>
        /// <returns>the sequence</returns>
        ICharSequence GetCharSequence(int index, int length, Encoding encoding);

        /// <summary>
        ///     Gets a string with the given length at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length">length the length to read</param>
        /// <param name="encoding">charset that should be use</param>
        /// <returns>the string value.</returns>
        /// <exception cref="IndexOutOfRangeException">
        ///     if length is greater than readable bytes.
        /// </exception>
        string GetString(int index, int length, Encoding encoding);

        /// <summary>
        ///     Sets the specified boolean at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetBoolean(int index, bool value);

        /// <summary>
        ///     Sets the specified byte at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetByte(int index, int value);

        /// <summary>
        ///     Sets the specified short at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetShort(int index, int value);

        /// <summary>
        ///     Sets the specified short at the specified absolute <paramref name="index" /> in this buffer
        ///     in the Little Endian Byte Order. This method does not directly modify <see cref="ReaderIndex" /> 
        ///     or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 2</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetShortLE(int index, int value);

        /// <summary>
        ///     Sets the specified integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetInt(int index, int value);

        /// <summary>
        ///     Sets the specified integer at the specified absolute <paramref name="index" /> in this buffer
        ///     in the Little Endian Byte Order. This method does not directly modify <see cref="ReaderIndex" /> 
        ///     or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 4</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetIntLE(int index, int value);

        /// <summary>
        ///     Sets the specified 24-bit medium integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     Note that the most significant byte is ignored in the specified value.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 3</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetMedium(int index, int value);

        /// <summary>
        ///     Sets the specified 24-bit medium integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     Note that the most significant byte is ignored in the specified value.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 3</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetMediumLE(int index, int value);

        /// <summary>
        ///     Sets the specified long integer at the specified absolute <paramref name="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetLong(int index, long value);

        /// <summary>
        ///     Sets the specified long integer at the specified absolute <paramref name="index" /> in this buffer
        ///     in the Little Endian Byte Order. This method does not directly modify <see cref="ReaderIndex" /> or 
        ///     <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c>index + 8</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetLongLE(int index, long value);

        /// <summary>
        ///     Transfers the <paramref name="src" /> byte buffer's contents starting at the specified absolute <paramref name="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/> is less than <c>0</c> or
        ///     <paramref name="length"/> is less than <c>0</c> or
        ///     <c><paramref name="index"/> + <paramref name="length"/></c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetBytes(int index, IByteBuffer src, int length);

        /// <summary>
        ///     Transfers the <paramref name="src" /> byte buffer's contents starting at the specified absolute <paramref name="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/> is less than <c>0</c> or
        ///     <paramref name="srcIndex"/> is less than <c>0</c> or
        ///     <paramref name="length"/> is less than <c>0</c> or
        ///     <c><paramref name="index"/> + <paramref name="length"/></c> greater than <see cref="Capacity" /> or
        ///     <c><paramref name="srcIndex"/> + <paramref name="length"/></c> greater than <c><paramref name="src" />.Capacity</c>
        /// </exception>
        IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length);

        /// <summary>
        ///     Transfers the <paramref name="src" /> byte buffer's contents starting at the specified absolute <paramref name="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index" /> is less than <c>0</c> or
        ///     <c><paramref name="index"/> + <paramref name="src"/>.Length</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuffer SetBytes(int index, byte[] src);

        /// <summary>
        ///     Transfers the <paramref name="src" /> byte buffer's contents starting at the specified absolute <paramref name="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <paramref name="index"/> is less than <c>0</c> or
        ///     <paramref name="srcIndex"/> is less than <c>0</c> or
        ///     <paramref name="length"/> is less than <c>0</c> or
        ///     <c><paramref name="index"/> + <paramref name="length"/></c> greater than <see cref="Capacity" /> or
        ///     <c><paramref name="srcIndex"/> + <paramref name="length"/></c> greater than <c><paramref name="src" />.Length</c>
        /// </exception>
        IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length);

        /// <summary>
        ///     Transfers the content of the specified source stream to this buffer
        ///     starting at the specified absolute <paramref name="index"/>.
        ///     This method does not modify <see cref="ReaderIndex"/> or <see cref="WriterIndex"/> of
        ///     this buffer.
        /// </summary>
        /// <param name="index">absolute index in this byte buffer to start writing to</param>
        /// <param name="src"></param>
        /// <param name="length">number of bytes to transfer</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the actual number of bytes read in from the specified channel.</returns>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <c>index</c> is less than <c>0</c> or
        ///     if <c>index + length</c> is greater than <c>this.capacity</c>
        /// </exception>
        Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken);

        /// <summary>
        ///     Fills this buffer with NULL (0x00) starting at the specified
        ///     absolute index. This method does not modify reader index
        ///     or writer index of this buffer
        /// </summary>
        /// <param name="index">absolute index in this byte buffer to start writing to</param>
        /// <param name="length">length the number of <tt>NUL</tt>s to write to the buffer</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified index is less than 0 or if index + length
        ///     is greater than capacity.
        /// </exception>
        IByteBuffer SetZero(int index, int length);

        /// <summary>
        /// Writes the specified <see cref="ICharSequence"/> at the current <see cref="WriterIndex"/> and increases
        /// the <see cref="WriterIndex"/> by the written bytes.
        /// </summary>
        /// <param name="index">on which the sequence should be written</param>
        /// <param name="sequence">to write</param>
        /// <param name="encoding">that should be used.</param>
        /// <returns>the written number of bytes.</returns>
        int SetCharSequence(int index, ICharSequence sequence, Encoding encoding);

        /// <summary>
        ///     Writes the specified string at the current writer index and increases
        ///     the  writer index by the written bytes.
        /// </summary>
        /// <param name="index">Index on which the string should be written</param>
        /// <param name="value">The string value.</param>
        /// <param name="encoding">Encoding that should be used.</param>
        /// <returns>The written number of bytes.</returns>
        /// <exception cref="IndexOutOfRangeException">
        ///    if writable bytes is not large enough to write the whole string.
        /// </exception>
        int SetString(int index, string value, Encoding encoding);

        /// <summary>
        ///     Gets a boolean at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>1</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>1</c></exception>
        bool ReadBoolean();

        /// <summary>
        ///     Gets a byte at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>1</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>1</c></exception>
        byte ReadByte();

        /// <summary>
        ///     Gets a short at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>2</c></exception>
        short ReadShort();

        /// <summary>
        ///     Gets a short at the current <see cref="ReaderIndex" /> in the Little Endian Byte Order and increases 
        ///     the <see cref="ReaderIndex" /> by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>2</c></exception>
        short ReadShortLE();

        /// <summary>
        ///     Gets a 24-bit medium integer at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>3</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>3</c></exception>
        int ReadMedium();

        /// <summary>
        ///     Gets a 24-bit medium integer at the current <see cref="ReaderIndex" /> in the Little Endian Byte Order and 
        ///     increases the <see cref="ReaderIndex" /> by <c>3</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>3</c></exception>
        int ReadMediumLE();

        /// <summary>
        ///     Gets an unsigned 24-bit medium integer at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>3</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>3</c></exception>
        int ReadUnsignedMedium();

        /// <summary>
        ///     Gets an unsigned 24-bit medium integer at the current <see cref="ReaderIndex" /> in the Little Endian Byte Order 
        ///     and increases the <see cref="ReaderIndex" /> by <c>3</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>3</c></exception>
        int ReadUnsignedMediumLE();

        /// <summary>
        ///     Gets an integer at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>4</c></exception>
        int ReadInt();

        /// <summary>
        ///     Gets an integer at the current <see cref="ReaderIndex" /> in the Little Endian Byte Order and increases 
        ///     the <see cref="ReaderIndex" />  by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>4</c></exception>
        int ReadIntLE();

        /// <summary>
        ///     Gets an long at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" /> 
        ///     by <c>8</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>4</c></exception>
        long ReadLong();

        /// <summary>
        ///     Gets an long at the current <see cref="ReaderIndex" /> in the Little Endian Byte Order and
        ///     increases the <see cref="ReaderIndex" /> by <c>8</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>4</c></exception>
        long ReadLongLE();

        /// <summary>
        ///     Reads <paramref name="length" /> bytes from this buffer into a new destination buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if <see cref="ReadableBytes" /> is less than <paramref name="length" />
        /// </exception>
        IByteBuffer ReadBytes(int length);

        /// <summary>
        /// Transfers this buffer's data to the specified destination starting at
        /// the current <see cref="ReaderIndex"/> and increases the <see cref="ReaderIndex"/>
        /// by the number of the transferred bytes (= <paramref name="length"/>).  This method
        /// is basically same with <see cref="ReadBytes(IByteBuffer, int, int)"/>,
        /// except that this method increases the <see cref="WriterIndex"/> of the
        /// destination by the number of the transferred bytes (= <paramref name="length"/>)
        /// while <see cref="ReadBytes(IByteBuffer, int, int)"/> does not.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="length">the number of bytes to transfer</param>
        /// <returns></returns>
        IByteBuffer ReadBytes(IByteBuffer destination, int length);

        /// <summary>
        /// Transfers this buffer's data to the specified destination starting at
        /// the current <see cref="ReaderIndex"/> and increases the <see cref="ReaderIndex"/>
        /// by the number of the transferred bytes (= <paramref name="length"/>).
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="dstIndex">the first index of the destination</param>
        /// <param name="length">the number of bytes to transfer</param>
        /// <returns></returns>
        IByteBuffer ReadBytes(IByteBuffer destination, int dstIndex, int length);

        /// <summary>
        /// Transfers this buffer's data to the specified destination starting at
        /// the current <see cref="ReaderIndex"/> and increases the <see cref="ReaderIndex"/>
        /// by the number of the transferred bytes (= <see cref="Array.Length"/> of <paramref name="destination"/>).
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IByteBuffer ReadBytes(byte[] destination);

        /// <summary>
        /// Transfers this buffer's data to the specified destination starting at
        /// the current <see cref="ReaderIndex"/> and increases the <see cref="ReaderIndex"/>
        /// by the number of the transferred bytes (= <paramref name="length"/>).
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="dstIndex">the first index of the destination</param>
        /// <param name="length">the number of bytes to transfer</param>
        /// <returns></returns>
        IByteBuffer ReadBytes(byte[] destination, int dstIndex, int length);

        /// <summary>
        /// Transfers this buffer's data to the specified stream starting at the
        /// current <see cref="ReaderIndex"/>.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="length">the number of bytes to transfer</param>
        /// <returns></returns>
        IByteBuffer ReadBytes(Stream destination, int length);

        /// <summary>
        /// Gets a <see cref="ICharSequence"/> with the given length at the current <see cref="ReaderIndex"/>
        /// and increases the <see cref="ReaderIndex"/> by the given length.
        /// </summary>
        /// <param name="length">the length to read</param>
        /// <param name="encoding">that should be used</param>
        /// <returns>the sequence</returns>
        ICharSequence ReadCharSequence(int length, Encoding encoding);

        /// <summary>
        ///     Gets a string with the given length at the current reader index
        ///     and increases the reader index by the given length.
        /// </summary>
        /// <param name="length">The length to read</param>
        /// <param name="encoding">Encoding that should be used</param>
        /// <returns>The string value</returns>
        string ReadString(int length, Encoding encoding);

        /// <summary>
        ///     Increases the current <see cref="ReaderIndex" /> by the specified <paramref name="length" /> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"> if <paramref name="length" /> is greater than <see cref="ReadableBytes" />.</exception>
        IByteBuffer SkipBytes(int length);

        /// <summary>
        /// Sets the specified boolean at the current <see cref="WriterIndex"/>
        /// and increases the <see cref="WriterIndex"/> by <c>1</c> in this buffer.
        /// If <see cref="WritableBytes"/> is less than <c>1</c>, <see cref="EnsureWritable(int)"/>
        /// will be called in an attempt to expand capacity to accommodate.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IByteBuffer WriteBoolean(bool value);

        IByteBuffer WriteByte(int value);

        IByteBuffer WriteShort(int value);

        IByteBuffer WriteShortLE(int value);

        IByteBuffer WriteMedium(int value);

        IByteBuffer WriteMediumLE(int value);

        IByteBuffer WriteInt(int value);

        IByteBuffer WriteIntLE(int value);

        IByteBuffer WriteLong(long value);

        IByteBuffer WriteLongLE(long value);

        IByteBuffer WriteBytes(IByteBuffer src, int length);

        IByteBuffer WriteBytes(IByteBuffer src, int srcIndex, int length);

        IByteBuffer WriteBytes(byte[] src);

        IByteBuffer WriteBytes(byte[] src, int srcIndex, int length);

        /// <summary>Checks if the specified <see cref="IByteBuffer"/> is a direct buffer and is composed of a single NIO buffer.</summary>
        /// <remarks>We check this because otherwise we need to make it a non-composite buffer.</remarks>
        bool IsSingleIoBuffer { get; }

        /// <summary>
        ///     Returns the maximum <see cref="ArraySegment{T}" /> of <see cref="Byte" /> that this buffer holds. Note that
        ///     <see cref="IByteBufferExtensions.GetIoBuffers(IByteBuffer)" />
        ///     or <see cref="GetIoBuffers(int,int)" /> might return a less number of <see cref="ArraySegment{T}" />s of
        ///     <see cref="Byte" />.
        /// </summary>
        /// <returns>
        ///     <c>-1</c> if this buffer cannot represent its content as <see cref="ArraySegment{T}" /> of <see cref="Byte" />.
        ///     the number of the underlying <see cref="IByteBuffer"/>s if this buffer has at least one underlying segment.
        ///     Note that this method does not return <c>0</c> to avoid confusion.
        /// </returns>
        /// <seealso cref="IByteBufferExtensions.GetIoBuffer(IByteBuffer)" />
        /// <seealso cref="GetIoBuffer(int,int)" />
        /// <seealso cref="IByteBufferExtensions.GetIoBuffers(IByteBuffer)" />
        /// <seealso cref="GetIoBuffers(int,int)" />
        int IoBufferCount { get; }

        /// <summary>
        ///     Exposes this buffer's sub-region as an <see cref="ArraySegment{T}" /> of <see cref="Byte" />. Returned segment
        ///     shares the content with this buffer. This method does not
        ///     modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer. Please note that the
        ///     returned segment will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content as <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IoBufferCount" />
        /// <seealso cref="IByteBufferExtensions.GetIoBuffers(IByteBuffer)" />
        /// <seealso cref="GetIoBuffers(int,int)" />
        ArraySegment<byte> GetIoBuffer(int index, int length);

        /// <summary>
        ///     Exposes this buffer's bytes as an array of <see cref="ArraySegment{T}" /> of <see cref="Byte" /> for the specified
        ///     index and length.
        ///     Returned segments share the content with this buffer. This method does
        ///     not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer. Please note that
        ///     returned segments will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content with <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IoBufferCount" />
        /// <seealso cref="IByteBufferExtensions.GetIoBuffer(IByteBuffer)" />
        /// <seealso cref="GetIoBuffer(int,int)" />
        ArraySegment<byte>[] GetIoBuffers(int index, int length);

        /// <summary>
        ///     Flag that indicates if this <see cref="IByteBuffer" /> is backed by a byte array or not
        /// </summary>
        bool HasArray { get; }

        /// <summary>
        ///     Grabs the underlying byte array for this buffer
        /// </summary>
        byte[] Array { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if this buffer has a reference to the low-level memory address that points
        /// to the backing data.
        /// </summary>
        bool HasMemoryAddress { get; }

        /// <summary>
        ///  Returns the low-level memory address that point to the first byte of ths backing data.
        /// </summary>
        /// <returns>The low-level memory address</returns>
        ref byte GetPinnableMemoryAddress();

        /// <summary>
        /// Returns the pointer address of the buffer if the memory is pinned.
        /// </summary>
        /// <returns>IntPtr.Zero if not pinned.</returns>
        IntPtr AddressOfPinnedMemory();

        /// <summary>
        /// Returns <c>true</c> if this <see cref="IByteBuffer"/> implementation is backed by a single memory region.
        /// Composite buffer implementations must return false even if they currently hold &lt; 1 components.
        /// For buffers that return <c>true</c>, it's guaranteed that a successful call to <see cref="DiscardReadBytes"/>
        /// will increase the value of <see cref="MaxFastWritableBytes"/> by the current <see cref="ReaderIndex"/>.
        /// 
        /// <para>This method will return <c>false</c> by default, and a <c>false</c> return value does not necessarily
        /// mean that the implementation is composite or that it is <i>not</i> backed by a single memory region.</para>
        /// </summary>
        bool IsContiguous { get; }

        /// <summary>
        ///     Creates a deep clone of the existing byte array and returns it
        /// </summary>
        IByteBuffer Duplicate();

        IByteBuffer RetainedDuplicate();

        /// <summary>
        /// Return the underlying buffer instance if this buffer is a wrapper of another buffer.
        /// </summary>
        /// <remarks>return <code>null</code> if this buffer is not a wrapper</remarks>
        IByteBuffer Unwrap();

        /// <summary>
        /// Returns a copy of this buffer's sub-region.  Modifying the content of
        /// the returned buffer or this buffer does not affect each other at all.
        /// This method does not modify <see cref="ReaderIndex"/> or <see cref="WriterIndex"/> of
        /// this buffer.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IByteBuffer Copy(int index, int length);

        /// <summary>
        /// Returns a slice of this buffer's readable bytes. Modifying the content
        /// of the returned buffer or this buffer affects each other's content
        /// while they maintain separate indexes and marks.  This method is
        /// identical to <code>buf.Slice(buf.ReaderIndex, buf.ReadableBytes)</code>.
        /// This method does not modify <see cref="ReaderIndex"/> or <see cref="WriterIndex"/> of
        /// this buffer.
        /// </summary>
        /// <remarks>
        /// Also be aware that this method will NOT call <see cref="IReferenceCounted.Retain()"/> and so the
        /// reference count will NOT be increased.
        /// </remarks>
        /// <returns></returns>
        IByteBuffer Slice();

        /// <summary>
        /// Returns a retained slice of this buffer's readable bytes. Modifying the content
        /// of the returned buffer or this buffer affects each other's content
        /// while they maintain separate indexes and marks.  This method is
        /// identical to <code>buf.Slice(buf.ReaderIndex, buf.ReadableBytes)</code>.
        /// This method does not modify <see cref="ReaderIndex"/> or <see cref="WriterIndex"/> of
        /// this buffer.
        /// </summary>
        /// <remarks>
        /// Note that this method returns a {@linkplain #retain() retained} buffer unlike <see cref="Slice()"/>.
        /// This method behaves similarly to {@code slice().retain()} except that this method may return
        /// a buffer implementation that produces less garbage.
        /// </remarks>
        /// <returns></returns>
        IByteBuffer RetainedSlice();

        /// <summary>
        /// Returns a slice of this buffer's sub-region. Modifying the content of
        /// the returned buffer or this buffer affects each other's content while
        /// they maintain separate indexes and marks.
        /// This method does not modify <see cref="ReaderIndex"/> or <see cref="WriterIndex"/> of
        /// this buffer.
        /// </summary>
        /// <remarks>
        /// Also be aware that this method will NOT call <see cref="IReferenceCounted.Retain()"/> and so the
        /// reference count will NOT be increased.
        /// </remarks>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IByteBuffer Slice(int index, int length);

        /// <summary>
        /// Returns a retained slice of this buffer's sub-region. Modifying the content of
        /// the returned buffer or this buffer affects each other's content while
        /// they maintain separate indexes and marks.
        /// This method does not modify <see cref="ReaderIndex"/> or <see cref="WriterIndex"/> of
        /// this buffer.
        /// </summary>
        /// <remarks>
        /// Note that this method returns a {@linkplain #retain() retained} buffer unlike <see cref="Slice(int, int)"/>.
        /// This method behaves similarly to {@code slice(...).retain()} except that this method may return
        /// a buffer implementation that produces less garbage.
        /// </remarks>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IByteBuffer RetainedSlice(int index, int length);

        int ArrayOffset { get; }

        /// <summary>
        /// Returns a new slice of this buffer's sub-region starting at the current
        /// <see cref="ReaderIndex"/> and increases the <see cref="ReaderIndex"/> by the size
        /// of the new slice (= <paramref name="length"/>).
        /// </summary>
        /// <remarks>
        /// Also be aware that this method will NOT call <see cref="IReferenceCounted.Retain()"/> and so the
        /// reference count will NOT be increased.
        /// </remarks>
        /// <param name="length">the size of the new slice</param>
        /// <returns>the newly created slice</returns>
        IByteBuffer ReadSlice(int length);

        /// <summary>
        /// Returns a new retained slice of this buffer's sub-region starting at the current
        /// <see cref="ReaderIndex"/> and increases the <see cref="ReaderIndex"/> by the size
        /// of the new slice (= <paramref name="length"/>).
        /// </summary>
        /// <remarks>
        /// Note that this method returns a {@linkplain #retain() retained} buffer unlike <see cref="ReadSlice(int)"/>.
        /// This method behaves similarly to {@code readSlice(...).retain()} except that this method may return
        /// a buffer implementation that produces less garbage.
        /// </remarks>
        /// <param name="length">the size of the new slice</param>
        /// <returns>the newly created slice</returns>
        IByteBuffer ReadRetainedSlice(int length);

        Task WriteBytesAsync(Stream stream, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Fills this buffer with <tt>NUL (0x00)</tt> starting at the current
        /// <see cref="WriterIndex"/> and increases the <see cref="WriterIndex"/> by the
        /// specified <paramref name="length"/>}.
        /// </summary>
        /// <remarks>If <see cref="WritableBytes"/> is less than <paramref name="length"/>, <see cref="EnsureWritable(int)"/>
        /// will be called in an attempt to expand capacity to accommodate.</remarks>
        /// <param name="length"></param>
        /// <returns></returns>
        IByteBuffer WriteZero(int length);

        /// <summary>
        /// Writes the specified <see cref="ICharSequence"/> at the current <see cref="WriterIndex"/> and increases
        /// the <see cref="WriterIndex"/> by the written bytes.
        /// in this buffer.
        /// If {<see cref="WritableBytes"/> is not large enough to write the whole sequence,
        /// <see cref="EnsureWritable(int)"/> will be called in an attempt to expand capacity to accommodate.
        /// </summary>
        /// <param name="sequence">to write</param>
        /// <param name="encoding">that should be used</param>
        /// <returns>the written number of bytes</returns>
        int WriteCharSequence(ICharSequence sequence, Encoding encoding);

        /// <summary>
        /// Writes the specified <see cref="String"/> at the current <see cref="WriterIndex"/> and increases
        /// the <see cref="WriterIndex"/> by the written bytes.
        /// in this buffer.
        /// If {<see cref="WritableBytes"/> is not large enough to write the whole sequence,
        /// <see cref="EnsureWritable(int)"/> will be called in an attempt to expand capacity to accommodate.
        /// </summary>
        /// <param name="value">to write</param>
        /// <param name="encoding">that should be used</param>
        /// <returns>the written number of bytes</returns>
        int WriteString(string value, Encoding encoding);

        string ToString();

        /// <summary>
        ///     Iterates over the specified area of this buffer with the specified <paramref name="processor"/> in ascending order.
        ///     (i.e. <paramref name="index"/>, <c>(index + 1)</c>,  .. <c>(index + length - 1)</c>)
        /// </summary>
        /// <returns>
        ///     <c>-1</c> if the processor iterated to or beyond the end of the specified area.
        ///     The last-visited index If the <see cref="IByteProcessor.Process"/> returned <c>false</c>.
        /// </returns>
        /// <param name="index">Index.</param>
        /// <param name="length">Length.</param>
        /// <param name="processor">Processor.</param>
        int ForEachByte(int index, int length, IByteProcessor processor);

        /// <summary>
        ///     Iterates over the specified area of this buffer with the specified <paramref name="processor"/> in descending order.
        ///     (i.e. <c>(index + length - 1)</c>, <c>(index + length - 2)</c>, ... <paramref name="index"/>)
        /// </summary>
        /// <returns>
        ///     <c>-1</c> if the processor iterated to or beyond the beginning of the specified area.
        ///     The last-visited index If the <see cref="IByteProcessor.Process"/> returned <c>false</c>.
        /// </returns>
        /// <param name="index">Index.</param>
        /// <param name="length">Length.</param>
        /// <param name="processor">Processor.</param>
        int ForEachByteDesc(int index, int length, IByteProcessor processor);
    }
}
