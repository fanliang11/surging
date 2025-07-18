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

using System;

namespace DotNetty.Codecs.Base64
{
    public interface IBase64Dialect
    {
        public bool BreakLinesByDefault { get; }

        public ReadOnlySpan<byte> Alphabet { get; }

        public ReadOnlySpan<sbyte> Decodabet { get; }
    }

    public sealed class Base64Dialect
    {
        public static readonly IBase64Dialect Standard = StandardDialect.Instance;

        public static readonly IBase64Dialect UrlSafe = UrlSafeDialect.Instance;

        /// <summary>
        /// http://www.faqs.org/rfcs/rfc3548.html
        /// Table 1: The Base 64 Alphabet
        /// </summary>
        sealed class StandardDialect : IBase64Dialect
        {
            public static readonly IBase64Dialect Instance = new StandardDialect();

            private StandardDialect() { }

            public bool BreakLinesByDefault => true;

            public ReadOnlySpan<byte> Alphabet => InternalAlphabet;

            public ReadOnlySpan<sbyte> Decodabet => InternalDecodabet;

            private static ReadOnlySpan<byte> InternalAlphabet => new byte[]
            {
                (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E',
                (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J',
                (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O',
                (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T',
                (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y',
                (byte)'Z', (byte)'a', (byte)'b', (byte)'c', (byte)'d',
                (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i',
                (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n',
                (byte)'o', (byte)'p', (byte)'q', (byte)'r', (byte)'s',
                (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x',
                (byte)'y', (byte)'z', (byte)'0', (byte)'1', (byte)'2',
                (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                (byte)'8', (byte)'9', (byte)'+', (byte)'/'
            };

            private static ReadOnlySpan<sbyte> InternalDecodabet => new sbyte[256]
            {
                -9, -9, -9, -9, -9, -9,                             // Decimal  0 -  5
                -9, -9, -9,                                         // Decimal  6 -  8
                -5, -5,                                             // Whitespace: Tab and Linefeed
                -9, -9,                                             // Decimal 11 - 12
                -5,                                                 // Whitespace: Carriage Return
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 14 - 26
                -9, -9, -9, -9, -9,                                 // Decimal 27 - 31
                -5,                                                 // Whitespace: Space
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9,             // Decimal 33 - 42
                62,                                                 // Plus sign at decimal 43
                -9, -9, -9,                                         // Decimal 44 - 46
                63,                                                 // Slash at decimal 47
                52, 53, 54, 55, 56, 57, 58, 59, 60, 61,             // Numbers zero through nine
                -9, -9, -9,                                         // Decimal 58 - 60
                -1,                                                 // Equals sign at decimal 61
                -9, -9, -9,                                         // Decimal 62 - 64
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,       // Letters 'A' through 'N'
                14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,     // Letters 'O' through 'Z'
                -9, -9, -9, -9, -9, -9,                             // Decimal 91 - 96
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, // Letters 'a' through 'm'
                39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, // Letters 'n' through 'z'
                -9, -9, -9, -9, -9,                                 // Decimal 123 - 127
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 128 - 140
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 141 - 153
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 154 - 166
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 167 - 179
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 180 - 192
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 193 - 205
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 206 - 218
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 219 - 231
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 232 - 244
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9          // Decimal 245 - 255
            };
        }

        /// <summary>
        /// http://www.faqs.org/rfcs/rfc3548.html
        /// Table 2: The "URL and Filename safe" Base 64 Alphabet
        /// </summary>
        sealed class UrlSafeDialect : IBase64Dialect
        {
            public static readonly IBase64Dialect Instance = new UrlSafeDialect();

            private UrlSafeDialect() { }

            public bool BreakLinesByDefault => false;

            public ReadOnlySpan<byte> Alphabet => InternalAlphabet;

            public ReadOnlySpan<sbyte> Decodabet => InternalDecodabet;

            private static ReadOnlySpan<byte> InternalAlphabet => new byte[]
            {
                (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E',
                (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J',
                (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O',
                (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T',
                (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y',
                (byte)'Z', (byte)'a', (byte)'b', (byte)'c', (byte)'d',
                (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i',
                (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n',
                (byte)'o', (byte)'p', (byte)'q', (byte)'r', (byte)'s',
                (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x',
                (byte)'y', (byte)'z', (byte)'0', (byte)'1', (byte)'2',
                (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                (byte)'8', (byte)'9', (byte)'-', (byte)'_'
            };

            private static ReadOnlySpan<sbyte> InternalDecodabet => new sbyte[256]
            {
                -9, -9, -9, -9, -9, -9,                             // Decimal  0 -  5
                -9, -9, -9,                                         // Decimal  6 -  8
                -5, -5,                                             // Whitespace: Tab and Linefeed
                -9, -9,                                             // Decimal 11 - 12
                -5,                                                 // Whitespace: Carriage Return
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 14 - 26
                -9, -9, -9, -9, -9,                                 // Decimal 27 - 31
                -5,                                                 // Whitespace: Space
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9,             // Decimal 33 - 42
                -9,                                                 // Plus sign at decimal 43
                -9,                                                 // Decimal 44
                62,                                                 // Minus sign at decimal 45
                -9,                                                 // Decimal 46
                -9,                                                 // Slash at decimal 47
                52, 53, 54, 55, 56, 57, 58, 59, 60, 61,             // Numbers zero through nine
                -9, -9, -9,                                         // Decimal 58 - 60
                -1,                                                 // Equals sign at decimal 61
                -9, -9, -9,                                         // Decimal 62 - 64
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,       // Letters 'A' through 'N'
                14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,     // Letters 'O' through 'Z'
                -9, -9, -9, -9,                                     // Decimal 91 - 94
                63,                                                 // Underscore at decimal 95
                -9,                                                 // Decimal 96
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, // Letters 'a' through 'm'
                39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, // Letters 'n' through 'z'
                -9, -9, -9, -9, -9,                                 // Decimal 123 - 127
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 128 - 140
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 141 - 153
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 154 - 166
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 167 - 179
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 180 - 192
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 193 - 205
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 206 - 218
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 219 - 231
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, // Decimal 232 - 244
                -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9          // Decimal 245 - 255
            };
        }
    }
}