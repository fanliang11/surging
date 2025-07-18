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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
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

    public static class IntegerExtensions
    {
        static readonly int[] MultiplyDeBruijnBitPosition =
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        public const int SizeInBits = sizeof(int) * 8;

        public static int RoundUpToPowerOfTwo(int res)
        {
            if (res <= 2)
            {
                return 2;
            }
            res--;
            res |= res >> 1;
            res |= res >> 2;
            res |= res >> 4;
            res |= res >> 8;
            res |= res >> 16;
            res++;
            return res;
        }

        public static int Log2(int v)
        {
            v |= v >> 1; // first round down to one less than a power of 2 
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            return MultiplyDeBruijnBitPosition[unchecked((uint)(v * 0x07C4ACDDU) >> 27)];
        }

        /// <summary>
        /// Returns the number of zero bits preceding the highest-order
        /// ("leftmost") one-bit in the two's complement binary representation
        /// of the specified <see cref="Int32"/> value.  Returns 32 if the
        /// specified value has no one-bits in its two's complement representation,
        /// in other words if it is equal to zero.
        ///
        /// <para>Note that this method is closely related to the logarithm base 2.
        /// For all positive <see cref="Int32"/> values x:</para>
        /// <code>
        /// <li>floor(log<sub>2</sub>(x)) = 31 - numberOfLeadingZeros(x)</li>
        /// <para />
        /// <li>ceil(log<sub>2</sub>(x)) = 32 - numberOfLeadingZeros(x - 1)</li>
        /// </code>
        /// </summary>
        /// <remarks>参考：https://zhuanlan.zhihu.com/p/34608787 </remarks>
        /// <param name="i">the value whose number of leading zeros is to be computed</param>
        /// <returns>the number of zero bits preceding the highest-order
        /// ("leftmost") one-bit in the two's complement binary representation
        /// of the specified <see cref="Int32"/> value, or 32 if the value</returns>
        public static int NumberOfLeadingZeros(int i)
        {
            // HD, Figure 5-6
            if (0u >= (uint)i) { return 32; }

            // PoolThreadCache.Log2 的调用不考虑负整数
            //if ((uint)i > SharedConstants.TooBigOrNegative) { return 0; }

            int n = 1;
            if (0u >= i.RightShift2U(16)) { n += 16; i <<= 16; }
            if (0u >= i.RightShift2U(24)) { n += 8; i <<= 8; }
            if (0u >= i.RightShift2U(28)) { n += 4; i <<= 4; }
            if (0u >= i.RightShift2U(30)) { n += 2; i <<= 2; }
            n -= i.RightUShift(31);
            return n;
        }
    }
}