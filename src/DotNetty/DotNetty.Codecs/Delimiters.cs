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

namespace DotNetty.Codecs
{
    using DotNetty.Buffers;

    public class Delimiters
    {
        /// <summary>Returns a null (0x00) delimiter, which could be used for Flash XML socket or any similar protocols</summary>
        public static IByteBuffer[] NullDelimiter() => new[] { Unpooled.WrappedBuffer(new byte[] { 0 }) };

        /// <summary>
        ///     Returns {@code CR ('\r')} and {@code LF ('\n')} delimiters, which could
        ///     be used for text-based line protocols.
        /// </summary>
        public static IByteBuffer[] LineDelimiter()
        {
            return new[]
            {
                Unpooled.WrappedBuffer(new[] { (byte)'\r', (byte)'\n' }),
                Unpooled.WrappedBuffer(new[] { (byte)'\n' }),
            };
        }

        Delimiters()
        {
            // Unused
        }
    }
}