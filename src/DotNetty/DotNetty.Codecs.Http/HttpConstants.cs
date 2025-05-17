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

namespace DotNetty.Codecs.Http
{
    using System.Text;
    using DotNetty.Buffers;

    public static partial class HttpConstants
    {
        /// <summary>Horizontal space</summary>
        public const byte HorizontalSpace = 32;

        /// <summary>Horizontal tab</summary>
        public const byte HorizontalTab = 9;

        /// <summary>Carriage return</summary>
        public const byte CarriageReturn = 13;
        public const uint NCarriageReturn = 13u;

        /// <summary>Equals '='</summary>
        public const byte EqualsSign = 61;

        /// <summary>Line feed character</summary>
        public const byte LineFeed = 10;
        public const uint NLineFeed = 10u;

        /// <summary>Colon ':'</summary>
        public const byte Colon = 58;

        /// <summary>Semicolon ';'</summary>
        public const byte Semicolon = 59;

        /// <summary>Comma ','</summary>
        public const byte Comma = 44;

        /// <summary>Double quote '"'</summary>
        public const byte DoubleQuote = (byte)'"';

         // Default character set (UTF-8)
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        // Horizontal space in char
        public const char HorizontalSpaceChar = (char)HorizontalSpace;

        // For HttpObjectEncoder
        internal const int CrlfShort = (CarriageReturn << 8) | LineFeed;

        internal const int ZeroCrlfMedium = ('0' << 16) | CrlfShort;

        internal static readonly byte[] ZeroCrlfCrlf = { (byte)'0', CarriageReturn, LineFeed, CarriageReturn, LineFeed };

        internal static readonly IByteBuffer CrlfBuf = Unpooled.UnreleasableBuffer(Unpooled.WrappedBuffer(new[] { CarriageReturn, LineFeed }));

        internal static readonly IByteBuffer ZeroCrlfCrlfBuf = Unpooled.UnreleasableBuffer(Unpooled.WrappedBuffer(ZeroCrlfCrlf));
    }
}
