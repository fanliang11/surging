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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// Largely based from https://github.com/dotnet/corefx/blob/release/3.0/src/System.Memory/src/System/Buffers/SequenceReaderExtensions.Binary.cs

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static partial class ByteBufferReaderExtensions
    {
        #region -- TryPeek / CopyTo --

        /// <summary>Copies data from the current <see cref="ByteBufferReader.Position"/> to the given <paramref name="destination"/> span.</summary>
        /// <returns>True if there is enough data to copy to the <paramref name="destination"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(ref this ByteBufferReader reader, Span<byte> destination)
        {
            ReadOnlySpan<byte> firstSpan = reader.UnreadSpan;
            int destLen = destination.Length;
            if ((uint)firstSpan.Length >= (uint)destLen)
            {
                firstSpan.Slice(0, destLen).CopyTo(destination);
                return;
            }

            CopyToMultisegment(ref reader, destination);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CopyToMultisegment(ref ByteBufferReader reader, Span<byte> destination)
        {
            if (!reader.TryCopyMultisegment(destination))
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }
        }

        /// <summary>Peek forward up to the number of positions specified by <paramref name="count"/>.</summary>
        /// <returns>Span over the peeked data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Peek(ref this ByteBufferReader reader, int count)
        {
            ReadOnlySpan<byte> firstSpan = reader.UnreadSpan;
            if ((uint)firstSpan.Length >= (uint)count)
            {
                return firstSpan.Slice(0, count);
            }

            // Not enough contiguous Ts, allocate and copy what we can get
            return PeekMultisegment(ref reader, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ReadOnlySpan<byte> PeekMultisegment(ref ByteBufferReader reader, int count)
        {
            if (!TryPeekMultisegment(ref reader, count, out var buffer))
            {
                ThrowHelper.ThrowArgumentException_NeedMoreData();
            }
            return buffer;
        }

        /// <summary>Peek forward up to the number of positions specified by <paramref name="count"/>.</summary>
        /// <returns>Span over the peeked data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryPeek(ref this ByteBufferReader reader, int count, out ReadOnlySpan<byte> buffer)
        {
            ReadOnlySpan<byte> firstSpan = reader.UnreadSpan;
            if ((uint)firstSpan.Length >= (uint)count)
            {
                buffer = firstSpan.Slice(0, count);
                return true;
            }

            // Not enough contiguous Ts, allocate and copy what we can get
            return TryPeekMultisegment(ref reader, count, out buffer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool TryPeekMultisegment(ref ByteBufferReader reader, int count, out ReadOnlySpan<byte> buffer)
        {
            Span<byte> tempBuffer = new byte[count];
            if (reader.TryCopyMultisegment(tempBuffer))
            {
                buffer = tempBuffer;
                return true;
            }
            buffer = default; return false;
        }

        #endregion

        #region -- TryRead --

        /// <summary>Try to read the given type out of the buffer if possible. Warning: this is dangerous to use with arbitrary
        /// structs- see remarks for full details.</summary>
        /// <remarks>IMPORTANT: The read is a straight copy of bits. If a struct depends on specific state of it's members to
        /// behave correctly this can lead to exceptions, etc. If reading endian specific integers, use the explicit
        /// overloads such as <see cref="TryReadShortLE(ref ByteBufferReader, out short)"/>.</remarks>
        /// <returns>True if successful. <paramref name="value"/> will be default if failed (due to lack of space).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryRead<T>(ref this ByteBufferReader reader, out T value) where T : unmanaged
        {
            ReadOnlySpan<byte> span = reader.UnreadSpan;
            var vSize = sizeof(T);
            if ((uint)span.Length >= (uint)vSize)
            {
                value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
                reader.Advance(vSize);
                return true;
            }

            return TryReadMultisegment(ref reader, out value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe bool TryReadMultisegment<T>(ref ByteBufferReader reader, out T value) where T : unmanaged
        {
            Debug.Assert(reader.UnreadSpan.Length < sizeof(T));

            // Not enough data in the current segment, try to peek for the data we need.
            T buffer = default;
            var vSize = sizeof(T);
            Span<byte> tempSpan = new Span<byte>(&buffer, vSize);

            if (!reader.TryCopyMultisegment(tempSpan))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(tempSpan));
            reader.Advance(vSize);
            return true;
        }

        #endregion

        #region -- Int16 --

        /// <summary>Reads an <see cref="Int16"/> as big endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="Int16"/>.</returns>
        public static bool TryReadShort(ref this ByteBufferReader reader, out short value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        /// <summary>Reads an <see cref="Int16"/> as little endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="Int16"/>.</returns>
        public static bool TryReadShortLE(ref this ByteBufferReader reader, out short value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        private static bool TryReadReverseEndianness(ref ByteBufferReader reader, out short value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        #endregion

        #region -- UInt16 --

        /// <summary>Reads an <see cref="UInt16"/> as big endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt16"/>.</returns>
        public static bool TryReadUnsignedShort(ref this ByteBufferReader reader, out ushort value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        /// <summary>Reads an <see cref="UInt16"/> as little endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt16"/>.</returns>
        public static bool TryReadUnsignedShortLE(ref this ByteBufferReader reader, out ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        private static bool TryReadReverseEndianness(ref ByteBufferReader reader, out ushort value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        #endregion

        #region -- Medium --

        const int MediumSize = 3;

        public static unsafe bool TryReadUnsignedMedium(ref this ByteBufferReader reader, out int value)
        {
            if (reader.TryPeek(MediumSize, out ReadOnlySpan<byte> span))
            {
                //fixed (byte* bytes = &MemoryMarshal.GetReference(span))
                //{
                //    value = UnsafeByteBufferUtil.GetUnsignedMedium(bytes);
                //}
                ref byte b = ref MemoryMarshal.GetReference(span);
                value = b << 16 | Unsafe.Add(ref b, 1) << 8 | Unsafe.Add(ref b, 2);
                reader.Advance(MediumSize);
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadUnsignedMediumLE(ref this ByteBufferReader reader, out int value)
        {
            if (reader.TryPeek(MediumSize, out ReadOnlySpan<byte> span))
            {
                ref byte b = ref MemoryMarshal.GetReference(span);
                value = b | Unsafe.Add(ref b, 1) << 8 | Unsafe.Add(ref b, 2) << 16;
                reader.Advance(MediumSize);
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadMedium(ref this ByteBufferReader reader, out int value)
        {
            if (reader.TryReadUnsignedMedium(out int v))
            {
                uint nValue = (uint)v;
                if ((nValue & 0x800000) != 0)
                {
                    nValue |= 0xff000000;
                }

                value = (int)nValue;
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadMediumLE(ref this ByteBufferReader reader, out int value)
        {
            if (reader.TryReadUnsignedMediumLE(out int v))
            {
                uint nValue = (uint)v;
                if ((nValue & 0x800000) != 0)
                {
                    nValue |= 0xff000000;
                }

                value = (int)nValue;
                return true;
            }
            value = default; return false;
        }

        #endregion

        #region -- Int32 --

        /// <summary>Reads an <see cref="Int32"/> as big endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="Int32"/>.</returns>
        public static bool TryReadInt(ref this ByteBufferReader reader, out int value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        /// <summary>Reads an <see cref="Int32"/> as little endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="Int32"/>.</returns>
        public static bool TryReadIntLE(ref this ByteBufferReader reader, out int value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        private static bool TryReadReverseEndianness(ref ByteBufferReader reader, out int value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        #endregion

        #region -- UInt32 --

        /// <summary>Reads an <see cref="UInt32"/> as big endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt32"/>.</returns>
        public static bool TryReadUnsignedInt(ref this ByteBufferReader reader, out uint value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        /// <summary>Reads an <see cref="UInt32"/> as little endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt32"/>.</returns>
        public static bool TryReadUnsignedIntLE(ref this ByteBufferReader reader, out uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        private static bool TryReadReverseEndianness(ref ByteBufferReader reader, out uint value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        #endregion

        #region -- Int64 --

        /// <summary>Reads an <see cref="Int64"/> as big endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="Int64"/>.</returns>
        public static bool TryReadLong(ref this ByteBufferReader reader, out long value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        /// <summary>Reads an <see cref="Int64"/> as little endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="Int64"/>.</returns>
        public static bool TryReadLongLE(ref this ByteBufferReader reader, out long value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        private static bool TryReadReverseEndianness(ref ByteBufferReader reader, out long value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        #endregion

        #region -- UInt64 --

        /// <summary>Reads an <see cref="UInt64"/> as big endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt64"/>.</returns>
        public static bool TryReadUnsignedLong(ref this ByteBufferReader reader, out ulong value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        /// <summary>Reads an <see cref="UInt64"/> as little endian.</summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt64"/>.</returns>
        public static bool TryReadUnsignedLongLE(ref this ByteBufferReader reader, out ulong value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead(out value);
            }

            return TryReadReverseEndianness(ref reader, out value);
        }

        private static bool TryReadReverseEndianness(ref ByteBufferReader reader, out ulong value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        #endregion

        #region -- Float --

        public static bool TryReadFloat(ref this ByteBufferReader reader, out float value)
        {
            if (reader.TryReadInt(out int v))
            {
                value = ByteBufferUtil.Int32BitsToSingle(v);
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadFloatLE(ref this ByteBufferReader reader, out float value)
        {
            if (reader.TryReadIntLE(out int v))
            {
                value = ByteBufferUtil.Int32BitsToSingle(v);
                return true;
            }
            value = default; return false;
        }

        #endregion

        #region -- Double --

        public static bool TryReadDouble(ref this ByteBufferReader reader, out double value)
        {
            if (reader.TryReadLong(out long v))
            {
                value = BitConverter.Int64BitsToDouble(v);
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadDoubleLE(ref this ByteBufferReader reader, out double value)
        {
            if (reader.TryReadLongLE(out long v))
            {
                value = BitConverter.Int64BitsToDouble(v);
                return true;
            }
            value = default; return false;
        }

        #endregion

        #region -- Decimal --

        const int DecimalValueLength = 16;

        public static bool TryReadDecimal(ref this ByteBufferReader reader, out decimal value)
        {
            if (reader.Remaining < DecimalValueLength) { value = default; return false; }

            _ = reader.TryReadInt(out int lo);
            _ = reader.TryReadInt(out int mid);
            _ = reader.TryReadInt(out int high);
            _ = reader.TryReadInt(out int flags);
            value = new decimal(new int[] { lo, mid, high, flags });
            return true;
        }

        public static bool TryReadDecimalLE(ref this ByteBufferReader reader, out decimal value)
        {
            if (reader.Remaining < DecimalValueLength) { value = default; return false; }

            _ = reader.TryReadIntLE(out int lo);
            _ = reader.TryReadIntLE(out int mid);
            _ = reader.TryReadIntLE(out int high);
            _ = reader.TryReadIntLE(out int flags);
            value = new decimal(new int[] { lo, mid, high, flags });
            return true;
        }

        #endregion

        #region -- Datetime --

        public static bool TryReadDatetime(ref this ByteBufferReader reader, out DateTime value)
        {
            if (reader.TryReadLong(out long v))
            {
                value = new DateTime(v);
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadDatetimeLE(ref this ByteBufferReader reader, out DateTime value)
        {
            if (reader.TryReadLongLE(out long v))
            {
                value = new DateTime(v);
                return true;
            }
            value = default; return false;
        }

        #endregion

        #region -- TimeSpan --

        public static bool TryReadTimeSpan(ref this ByteBufferReader reader, out TimeSpan value)
        {
            if (reader.TryReadLong(out long v))
            {
                value = new TimeSpan(v);
                return true;
            }
            value = default; return false;
        }

        public static bool TryReadTimeSpanLE(ref this ByteBufferReader reader, out TimeSpan value)
        {
            if (reader.TryReadLongLE(out long v))
            {
                value = new TimeSpan(v);
                return true;
            }
            value = default; return false;
        }

        #endregion

        #region -- 7BitEncodedInt --

        public static bool TryRead7BitEncodedInt(ref this ByteBufferReader reader, out int value)
        {
            // Read out an Int32 7 bits at a time.  The high bit
            // of the byte when on means to continue reading more bytes.
            value = 0;
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream.  Read a max of 5 bytes.
                // In a future version, add a DataFormatException.
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                {
                    ThrowFormatException_Bad7BitInt32();
                }

                // ReadByte handles end of stream cases for us.
                if (!reader.TryRead(out b))
                {
                    reader.Rewind(count);
                    value = 0;
                    return false;
                }
                value |= (b & 0x7F) << shift;
                shift += 7;
                count++;
            } while ((b & 0x80) != 0);

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFormatException_Bad7BitInt32()
        {
            throw GetFormatException();
            static FormatException GetFormatException()
            {
                return new FormatException("Too many bytes in what should have been a 7 bit encoded Int32.");
            }
        }

        #endregion
    }
}
