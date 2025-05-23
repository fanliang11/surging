// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using DotNetty.Common.Utilities;

    public static class ConstantTimeUtils
    {
        /// <summary>
        /// Compare two <see cref="int"/>s without leaking timing information.
        /// <code>
        /// int v1 = 1;
        /// int v1 = 1;
        /// int v1 = 1;
        /// int v1 = 500;
        /// bool equals = (EqualsConstantTime(l1, l2) &amp; EqualsConstantTime(l3, l4)) != 0;
        /// </code>.
        /// </summary>
        /// <param name="x">the first value.</param>
        /// <param name="y">the second value.</param>
        /// <returns><c>0</c>if not equal. <c>1</c> if equal.</returns>
        public static int EqualsConstantTime(int x, int y)
        {
            int z = -1 ^ (x ^ y);
            z &= z >> 16;
            z &= z >> 8;
            z &= z >> 4;
            z &= z >> 2;
            z &= z >> 1;
            return z & 1;
        }

        /// <summary>
        /// Compare two <see cref="long"/>s without leaking timing information.
        /// <code>
        /// long v1 = 1;
        /// long v1 = 1;
        /// long v1 = 1;
        /// long v1 = 500;
        /// bool equals = (EqualsConstantTime(l1, l2) &amp; EqualsConstantTime(l3, l4)) != 0;
        /// </code>.
        /// </summary>
        /// <param name="x">the first value.</param>
        /// <param name="y">the second value.</param>
        /// <returns><c>0</c>if not equal. <c>1</c> if equal.</returns>
        public static int EqualsConstantTime(long x, long y)
        {
            long z = -1L ^ (x ^ y);
            z &= z >> 32;
            z &= z >> 16;
            z &= z >> 8;
            z &= z >> 4;
            z &= z >> 2;
            z &= z >> 1;
            return (int)(z & 1);
        }

        /// <summary>
        /// Compare two {@code byte} arrays for equality without leaking timing information.
        /// For performance reasons no bounds checking on the parameters is performed.
        /// 
        /// <para>The <see cref="int"/> return type is intentional and is designed to allow cascading of constant time operations:</para>
        /// <code>
        ///     byte[] s1 = new {1, 2, 3};
        ///     byte[] s2 = new {1, 2, 3};
        ///     byte[] s3 = new {1, 2, 3};
        ///     byte[] s4 = new {4, 5, 6};
        ///     boolean equals = (EqualsConstantTime(s1, 0, s2, 0, s1.length) &amp;
        ///                       EqualsConstantTime(s3, 0, s4, 0, s3.length)) != 0;
        /// </code>
        /// </summary>
        /// <param name="bytes1">the first byte array.</param>
        /// <param name="startPos1">the position (inclusive) to start comparing in <paramref name="bytes1"/>.</param>
        /// <param name="bytes2">the second byte array.</param>
        /// <param name="startPos2">the position (inclusive) to start comparing in <paramref name="bytes2"/>.</param>
        /// <param name="length">the amount of bytes to compare. This is assumed to be validated as not going out of bounds
        /// by the caller.</param>
        /// <returns><c>0</c> if not equal. <c>1</c> if equal.</returns>
        public static int EqualsConstantTime(byte[] bytes1, int startPos1, byte[] bytes2, int startPos2, int length)
        {
            // Benchmarking demonstrates that using an int to accumulate is faster than other data types.
            int b = 0;
            int end = startPos1 + length;
            for (; startPos1 < end; ++startPos1, ++startPos2)
            {
                b |= bytes1[startPos1] ^ bytes2[startPos2];
            }

            return EqualsConstantTime(b, 0);
        }

        /// <summary>
        /// Compare two {@link CharSequence} objects without leaking timing information.
        /// 
        /// <para>The <see cref="int"/> return type is intentional and is designed to allow cascading of constant time operations:</para>
        /// <code>
        ///     String s1 = "foo";
        ///     String s2 = "foo";
        ///     String s3 = "foo";
        ///     String s4 = "goo";
        ///     boolean equals = (EqualsConstantTime(s1, s2) &amp; EqualsConstantTime(s3, s4)) != 0;
        /// </code>
        /// </summary>
        /// <param name="s1">the first value.</param>
        /// <param name="s2">the second value.</param>
        /// <returns><c>0</c> if not equal. <c>1</c> if equal.</returns>
        public static int EqualsConstantTime(ICharSequence s1, ICharSequence s2)
        {
            if (s1.Count != s2.Count)
            {
                return 0;
            }

            // Benchmarking demonstrates that using an int to accumulate is faster than other data types.
            int c = 0;
            for (int i = 0; i < s1.Count; ++i)
            {
                c |= s1[i] ^ s2[i];
            }

            return EqualsConstantTime(c, 0);
        }
    }
}