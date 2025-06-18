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

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public ref partial struct ByteBufferWriter
    {
        /// <summary>Write a UInt16 into the <see cref="IByteBuffer"/> of bytes as big endian.</summary>
        public void WriteUnsignedShort(ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            GrowAndEnsureIf(Int16ValueLength);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(Int16ValueLength);
        }

        /// <summary>Write a UInt16 into the <see cref="IByteBuffer"/> of bytes as little endian.</summary>
        public void WriteUnsignedShortLE(ushort value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            GrowAndEnsureIf(Int16ValueLength);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(Int16ValueLength);
        }

        /// <summary>Write a UInt32 into the <see cref="IByteBuffer"/> of bytes as big endian.</summary>
        public void WriteUnsignedInt(uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            GrowAndEnsureIf(Int32ValueLength);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(Int32ValueLength);
        }

        /// <summary>Write a UInt32 into the <see cref="IByteBuffer"/> of bytes as little endian.</summary>
        public void WriteUnsignedIntLE(uint value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            GrowAndEnsureIf(Int32ValueLength);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(Int32ValueLength);
        }

        /// <summary>Write a UInt64 into the <see cref="IByteBuffer"/> of bytes as big endian.</summary>
        public void WriteUnsignedLong(ulong value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            GrowAndEnsureIf(Int64ValueLength);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(Int64ValueLength);
        }

        /// <summary>Write a UInt64 into the <see cref="IByteBuffer"/> of bytes as little endian.</summary>
        public void WriteUnsignedLongLE(ulong value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            GrowAndEnsureIf(Int64ValueLength);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(Int64ValueLength);
        }
    }
}
