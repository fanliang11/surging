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
    using System.Diagnostics;

    using static ByteProcessorUtils;

    /// <summary>
    ///     Provides a mechanism to iterate over a collection of bytes.
    /// </summary>
    public interface IByteProcessor
    {
        bool Process(byte value);
    }

    public sealed class IndexOfProcessor : IByteProcessor
    {
        readonly uint byteToFind; // Use uint for comparisons to avoid unnecessary 8->32 extensions

        public IndexOfProcessor(byte byteToFind)
        {
            this.byteToFind = byteToFind;
        }

        public bool Process(byte value) => value != this.byteToFind;
    }

    public sealed class IndexNotOfProcessor : IByteProcessor
    {
        readonly uint byteToNotFind; // Use uint for comparisons to avoid unnecessary 8->32 extensions

        public IndexNotOfProcessor(byte byteToNotFind)
        {
            this.byteToNotFind = byteToNotFind;
        }

        public bool Process(byte value) => value == this.byteToNotFind;
    }

    public sealed class ByteProcessor : IByteProcessor
    {
        readonly Func<byte, bool> customHandler;
        public ByteProcessor(Func<byte, bool> customHandler)
        {
            Debug.Assert(customHandler is object, "'customHandler' is required parameter.");
            this.customHandler = customHandler;
        }

        public bool Process(byte value) => this.customHandler(value);

        /// <summary>
        ///     Aborts on a <c>NUL (0x00)</c>.
        /// </summary>
        public static IByteProcessor FindNul = new IndexOfProcessor(0);

        /// <summary>
        ///     Aborts on a non-<c>NUL (0x00)</c>.
        /// </summary>
        public static IByteProcessor FindNonNul = new IndexNotOfProcessor(0);

        /// <summary>
        ///     Aborts on a <c>CR ('\r')</c>.
        /// </summary>
        public static IByteProcessor FindCR = new IndexOfProcessor(CarriageReturn);

        /// <summary>
        ///     Aborts on a non-<c>CR ('\r')</c>.
        /// </summary>
        public static IByteProcessor FindNonCR = new IndexNotOfProcessor(CarriageReturn);

        /// <summary>
        ///     Aborts on a <c>LF ('\n')</c>.
        /// </summary>
        public static IByteProcessor FindLF = new IndexOfProcessor(LineFeed);

        /// <summary>
        ///     Aborts on a non-<c>LF ('\n')</c>.
        /// </summary>
        public static IByteProcessor FindNonLF = new IndexNotOfProcessor(LineFeed);

        /// <summary>
        ///     Aborts on a <c>CR (';')</c>.
        /// </summary>
        public static IByteProcessor FindSemicolon = new IndexOfProcessor((byte)';');

        /// <summary>
        ///     Aborts on a comma <c>(',')</c>.
        /// </summary>
        public static IByteProcessor FindComma = new IndexOfProcessor((byte)',');

        /// <summary>
        ///     Aborts on a ascii space character (<c>' '</c>).
        /// </summary>
        public static IByteProcessor FindAsciiSpace = new IndexOfProcessor(Space);

        /// <summary>
        ///     Aborts on a <c>CR ('\r')</c> or a <c>LF ('\n')</c>.
        /// </summary>
        public static IByteProcessor FindCrlf = new ByteProcessor(new Func<byte, bool>(value => value != CarriageReturn && value != LineFeed));

        /// <summary>
        ///     Aborts on a byte which is neither a <c>CR ('\r')</c> nor a <c>LF ('\n')</c>.
        /// </summary>
        public static IByteProcessor FindNonCrlf = new ByteProcessor(new Func<byte, bool>(value => value == CarriageReturn || value == LineFeed));

        /// <summary>
        ///     Aborts on a linear whitespace (a <c>' '</c> or a <c>'\t'</c>).
        /// </summary>
        public static IByteProcessor FindLinearWhitespace = new ByteProcessor(new Func<byte, bool>(value => value != Space && value != HTab));

        /// <summary>
        ///     Aborts on a byte which is not a linear whitespace (neither <c>' '</c> nor <c>'\t'</c>).
        /// </summary>
        public static IByteProcessor FindNonLinearWhitespace = new ByteProcessor(new Func<byte, bool>(value => value == Space || value == HTab));
    }
}