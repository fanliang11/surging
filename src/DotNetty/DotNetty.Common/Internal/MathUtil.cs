// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class MathUtil
    {
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsOutOfBounds(int index, int length, int capacity) =>
            (index | length | (index + length) | (capacity - (index + length))) < 0;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int FindNextPositivePowerOfTwo(int value)
        {
            Debug.Assert(value > int.MinValue && value < 0x40000000);
            return 1 << (32 - NumberOfLeadingZeros(value - 1));
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int SafeFindNextPositivePowerOfTwo(int value) =>
            /*value <= 0*/(uint)(value - 1) > SharedConstants.TooBigOrNegative ? 1 : value >= 0x40000000 ? 0x40000000 : FindNextPositivePowerOfTwo(value);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int NumberOfLeadingZeros(this int i)
        {
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            i = ~i;

            //bit count
            i -= ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            i = (((i >> 4) + i) & 0x0F0F0F0F);
            i += (i >> 8);
            i += (i >> 16);
            return (i & 0x0000003F);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int Compare(long x, long y)
        {
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
        }
    }
}
