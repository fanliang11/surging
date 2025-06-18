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
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Utilities;
#if !NET
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;
#endif

    partial class ByteBufferUtil
    {
        /// <summary>
        /// Compares the two specified buffers as described in {@link ByteBuf#compareTo(ByteBuf)}.
        /// This method is useful when implementing a new buffer type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(IByteBuffer bufferA, IByteBuffer bufferB)
        {
            if (bufferA.IsSingleIoBuffer && bufferB.IsSingleIoBuffer)
            {
                var spanA = bufferA.GetReadableSpan();
                var spanB = bufferB.GetReadableSpan();
#if NET
                return spanA.SequenceCompareTo(spanB);
#else
                return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(spanA), spanA.Length, ref MemoryMarshal.GetReference(spanB), spanB.Length);
#endif
            }
            return CompareSlow(bufferA, bufferB);
        }

        private static int CompareSlow(IByteBuffer bufferA, IByteBuffer bufferB)
        {
            int aLen = bufferA.ReadableBytes;
            int bLen = bufferB.ReadableBytes;
            int minLength = Math.Min(aLen, bLen);
            int uintCount = minLength.RightUShift(2);
            int byteCount = minLength & 3;

            int aIndex = bufferA.ReaderIndex;
            int bIndex = bufferB.ReaderIndex;

            if (uintCount > 0)
            {
                int uintCountIncrement = uintCount << 2;
                int res = CompareUint(bufferA, bufferB, aIndex, bIndex, uintCountIncrement);
                if (res != 0)
                {
                    return res;
                }

                aIndex += uintCountIncrement;
                bIndex += uintCountIncrement;
            }

            for (int aEnd = aIndex + byteCount; aIndex < aEnd; ++aIndex, ++bIndex)
            {
                int comp = bufferA.GetByte(aIndex) - bufferB.GetByte(bIndex);
                if (comp != 0)
                {
                    return comp;
                }
            }

            return aLen - bLen;
        }

        static int CompareUint(IByteBuffer bufferA, IByteBuffer bufferB, int aIndex, int bIndex, int uintCountIncrement)
        {
            for (int aEnd = aIndex + uintCountIncrement; aIndex < aEnd; aIndex += 4, bIndex += 4)
            {
                long va = bufferA.GetUnsignedInt(aIndex);
                long vb = bufferB.GetUnsignedInt(bIndex);
                if (va > vb)
                {
                    return 1;
                }
                if (va < vb)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}
