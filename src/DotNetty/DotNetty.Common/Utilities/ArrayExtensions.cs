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

namespace DotNetty.Common.Utilities
{
    using System;
    using DotNetty.Common.Internal;

    /// <summary>
    ///     Extension methods used for slicing byte arrays
    /// </summary>
    public static class ArrayExtensions
    {
        public static readonly byte[] ZeroBytes = EmptyArray<byte>.Instance;

        public static T[] Slice<T>(this T[] array, int length) => Slice(array, 0, length);

        public static T[] Slice<T>(this T[] array, int index, int length)
        {
            if (array is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array); }
            if ((uint)(index + length) > (uint)array.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_Slice(index, length, array.Length);
            }

            T[] result;
            Span<T> destSpan = result = new T[length];
            array.AsSpan(index, length).CopyTo(destSpan);
            return result;
        }

        public static void SetRange<T>(this T[] array, int index, T[] src) => SetRange(array, index, src, 0, src.Length);

        public static void SetRange<T>(this T[] array, int index, T[] src, int srcIndex, int srcLength)
        {
            if (array is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array); }
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }

            Span<T> srcSpan = src.AsSpan(srcIndex, srcLength);
            srcSpan.CopyTo(array.AsSpan(index));
        }

        public static void Fill<T>(this T[] array, T value)
        {
            Span<T> span = array;
            span.Fill(value);
        }

        public static void Fill<T>(this T[] array, int offset, int count, T value)
        {
            if (MathUtil.IsOutOfBounds(offset, count, array.Length))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Index(offset, count, array.Length);
            }

            Span<T> span = array.AsSpan(offset, count);
            span.Fill(value);
        }

        /// <summary>
        ///     Merge the byte arrays into one byte array.
        /// </summary>
        public static byte[] CombineBytes(this byte[][] arrays)
        {
            long newlength = 0;
            foreach (byte[] array in arrays)
            {
                newlength += array.Length;
            }

            var mergedArray = new byte[newlength];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, mergedArray, offset, array.Length);
                offset += array.Length;
            }

            return mergedArray;
        }
    }
}