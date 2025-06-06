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
    using DotNetty.Common.Utilities;
#if !NET
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;
#endif

    partial class ByteBufferUtil
    {
        /// <summary>
        ///     Returns <c>true</c> if and only if the two specified buffers are
        ///     identical to each other for {@code length} bytes starting at {@code aStartIndex}
        ///     index for the {@code a} buffer and {@code bStartIndex} index for the {@code b} buffer.
        ///     A more compact way to express this is:
        ///     <p />
        ///     {@code a[aStartIndex : aStartIndex + length] == b[bStartIndex : bStartIndex + length]}
        /// </summary>
        public static bool Equals(IByteBuffer a, int aStartIndex, IByteBuffer b, int bStartIndex, int length)
        {
            if (aStartIndex < 0 || bStartIndex < 0 || length < 0)
            {
                ThrowHelper.ThrowArgumentException_NonNegative();
            }
            if (a.WriterIndex - length < aStartIndex || b.WriterIndex - length < bStartIndex)
            {
                return false;
            }

            if (a.IsSingleIoBuffer && b.IsSingleIoBuffer)
            {
                var spanA = a.GetReadableSpan(aStartIndex, length);
                var spanB = b.GetReadableSpan(bStartIndex, length);
#if NET
                return spanA.SequenceEqual(spanB);
#else
                return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(spanA), ref MemoryMarshal.GetReference(spanB), length);
#endif
            }
            return EqualsSlow(a, aStartIndex, b, bStartIndex, length);
        }

        private static bool EqualsSlow(IByteBuffer a, int aStartIndex, IByteBuffer b, int bStartIndex, int length)
        {
            int longCount = unchecked((int)((uint)length >> 3));
            int byteCount = length & 7;

            for (int i = longCount; i > 0; i--)
            {
                if (a.GetLong(aStartIndex) != b.GetLong(bStartIndex))
                {
                    return false;
                }
                aStartIndex += 8;
                bStartIndex += 8;
            }

            for (int i = byteCount; i > 0; i--)
            {
                if (a.GetByte(aStartIndex) != b.GetByte(bStartIndex))
                {
                    return false;
                }
                aStartIndex++;
                bStartIndex++;
            }

            return true;
        }

        /// <summary>
        ///     Returns <c>true</c> if and only if the two specified buffers are
        ///     identical to each other as described in {@link ByteBuf#equals(Object)}.
        ///     This method is useful when implementing a new buffer type.
        /// </summary>
        public static bool Equals(IByteBuffer bufferA, IByteBuffer bufferB)
        {
            int aLen = bufferA.ReadableBytes;
            if (aLen != bufferB.ReadableBytes)
            {
                return false;
            }

            return Equals(bufferA, bufferA.ReaderIndex, bufferB, bufferB.ReaderIndex, aLen);
        }

        /// <summary>
        ///     Calculates the hash code of the specified buffer.  This method is
        ///     useful when implementing a new buffer type.
        /// </summary>
        public static int HashCode(IByteBuffer buffer)
        {
            int aLen = buffer.ReadableBytes;
            int intCount = aLen.RightUShift(2);
            int byteCount = aLen & 3;

            int hashCode = EmptyByteBuffer.EmptyByteBufferHashCode;
            int arrayIndex = buffer.ReaderIndex;
            for (int i = intCount; i > 0; i--)
            {
                hashCode = 31 * hashCode + buffer.GetInt(arrayIndex);
                arrayIndex += 4;
            }

            for (int i = byteCount; i > 0; i--)
            {
                hashCode = 31 * hashCode + buffer.GetByte(arrayIndex++);
            }

            if (0u >= (uint)hashCode)
            {
                hashCode = 1;
            }

            return hashCode;
        }
    }
}
