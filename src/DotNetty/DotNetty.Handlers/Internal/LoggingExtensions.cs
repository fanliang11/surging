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

using System;
using System.Runtime.CompilerServices;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Flow;
using DotNetty.Handlers.Streams;
using DotNetty.Transport.Channels;

namespace DotNetty.Handlers
{
    internal static class HttpLoggingExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void NonEmptyQueue(this IInternalLogger logger, RecyclableQueue queue)
        {
            logger.Trace($"Non-empty queue: {queue}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnexpectedClientHelloPacket(this IInternalLogger logger, IByteBuffer input, Exception e)
        {
            logger.Warn($"Unexpected client hello packet: {ByteBufferUtil.HexDump(input)}", e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IsEndOfInputFailed<T>(this IInternalLogger logger, Exception exception)
        {
            logger.Warn($"{StringUtil.SimpleClassName(typeof(ChunkedWriteHandler<T>))} failed", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnexpectedExceptionWhileSendingChunks(this IInternalLogger logger, Exception exception)
        {
            logger.Warn("Unexpected exception while sending chunks.", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseAChunkedInput(this IInternalLogger logger, Exception exception)
        {
            logger.Warn("Failed to close a chunked input.", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TlsHandshakeFailure(this IInternalLogger logger, IChannelHandlerContext ctx, Exception cause)
        {
            logger.Warn("{} TLS handshake failed:", ctx.Channel, cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToSelectAppProtocol(this IInternalLogger logger, IChannelHandlerContext ctx, Exception cause)
        {
            logger.Warn("{} Failed to select the application-level protocol:", ctx.Channel, cause);
        }
    }
}
