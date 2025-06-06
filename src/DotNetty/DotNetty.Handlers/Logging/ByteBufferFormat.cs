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

namespace DotNetty.Handlers.Logging
{
    using System.Text;
    using DotNetty.Buffers;

    /// <summary>
    /// Used to control the format and verbosity of logging for <see cref="IByteBuffer"/>s and <see cref="IByteBufferHolder"/>s.
    /// </summary>
    public enum ByteBufferFormat
    {
        /// <summary>
        /// <see cref="IByteBuffer"/>s will be logged in a simple format, with no hex dump included.
        /// </summary>
        Simple,

        /// <summary>
        /// <see cref="IByteBuffer"/>s will be logged using <see cref="ByteBufferUtil.AppendPrettyHexDump(StringBuilder, IByteBuffer)"/>.
        /// </summary>
        HexDump
    }
}