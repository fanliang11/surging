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
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Internal;

    partial class AbstractByteBuffer
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckIndexBounds(int readerIndex, int writerIndex)
        {
            if (readerIndex < 0 || readerIndex > writerIndex)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_ReaderIndex(readerIndex, writerIndex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckIndexBounds(int readerIndex, int writerIndex, int capacity)
        {
            if (readerIndex < 0 || readerIndex > writerIndex || writerIndex > capacity)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_CheckIndexBounds(readerIndex, writerIndex, capacity);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckReadableBounds(IByteBuffer src, int length)
        {
            if ((uint)length > (uint)src.ReadableBytes)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_ReadableBytes(length, src);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckMinReadableBounds(int minimumReadableBytes, int readerIndex, int writerIndex, AbstractByteBuffer buf)
        {
            if (CheckBounds && (readerIndex > writerIndex - minimumReadableBytes))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_ReaderIndex(minimumReadableBytes, readerIndex, writerIndex, buf);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckWritableBounds(IByteBuffer dst, int length)
        {
            if ((uint)length > (uint)dst.WritableBytes)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_WritableBytes(length, dst);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckMinWritableBounds(int minWritableBytes, int writerIndex, int maxCapacity, AbstractByteBuffer buf)
        {
            if (minWritableBytes > maxCapacity - writerIndex)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_WriterIndex(minWritableBytes, writerIndex, maxCapacity, buf);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckRangeBounds(ExceptionArgument indexName, int index, int fieldLength, int capacity)
        {
            if (MathUtil.IsOutOfBounds(index, fieldLength, capacity))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Index(indexName, index, fieldLength, capacity);
            }
        }
    }
}
