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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotNetty.Codecs.Http.Cors;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace DotNetty.Codecs.Http
{
    internal static class HttpLoggingExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DecodingWebSocketFrameOpCode(this IInternalLogger logger, int frameOpcode)
        {
            logger.Trace("Decoding WebSocket Frame opCode={}", frameOpcode);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DecodingWebSocketFrameLength(this IInternalLogger logger, long framePayloadLength)
        {
            logger.Trace("Decoding WebSocket Frame length={}", framePayloadLength);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void EncodingWebSocketFrameOpCode(this IInternalLogger logger, byte opcode, int length)
        {
            logger.Trace($"Encoding WebSocket Frame opCode={opcode} length={length}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SkippingCookieWithNullName(this IInternalLogger logger)
        {
            logger.Debug("Skipping cookie with null name");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SkippingCookieWithNullValue(this IInternalLogger logger)
        {
            logger.Debug("Skipping cookie with null value");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SkippingCookieBecauseStartingQuotesAreNotProperlyBalancedIn(this IInternalLogger logger, StringCharSequence sequence)
        {
            logger.Debug("Skipping cookie because starting quotes are not properly balanced in '{}'", sequence);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SkippingCookieBecauseNameContainsInvalidChar(this IInternalLogger logger, string name, int index)
        {
            logger.Debug("Skipping cookie because name '{}' contains invalid char '{}'", name, name[index]);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SkippingCookieBecauseValueContainsInvalidChar(this IInternalLogger logger, ICharSequence value, int index)
        {
            logger.Debug("Skipping cookie because value '{}' contains invalid char '{}'", value, value[index]);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RequestOriginWasNotAmongTheConfiguredOrigins(this IInternalLogger logger, ICharSequence origin, CorsConfig config)
        {
            logger.Debug("Request origin [{}]] was not among the configured origins [{}]", origin, config.Origins);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersion07ClientHandshakeKey(this IInternalLogger logger, string key, AsciiString expectedChallengeResponseString)
        {
            logger.Debug("WebSocket version 07 client handshake key: {}, expected response: {}", key, expectedChallengeResponseString);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersion08ClientHandshakeKey(this IInternalLogger logger, string key, AsciiString expectedChallengeResponseString)
        {
            logger.Debug("WebSocket version 08 client handshake key: {}, expected response: {}", key, expectedChallengeResponseString);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersion13ClientHandshakeKey(this IInternalLogger logger, string key, AsciiString expectedChallengeResponseString)
        {
            logger.Debug("WebSocket version 13 client handshake key: {}, expected response: {}", key, expectedChallengeResponseString);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersionServerHandshake(this IInternalLogger logger, IChannel channel, WebSocketVersion version)
        {
            logger.Debug("{} WebSocket version {} server handshake", channel, version);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RequestedSubprotocolNotSupported(this IInternalLogger logger, ICharSequence subprotocols)
        {
            logger.Debug("Requested subprotocol(s) not supported: {}", subprotocols);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersion07ServerHandshakeKey(this IInternalLogger logger, ICharSequence key, string accept)
        {
            logger.Debug("WebSocket version 07 server handshake key: {}, response: {}.", key, accept);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersion08ServerHandshakeKey(this IInternalLogger logger, ICharSequence key, string accept)
        {
            logger.Debug("WebSocket version 08 server handshake key: {}, response: {}.", key, accept);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WebSocketVersion13ServerHandshakeKey(this IInternalLogger logger, ICharSequence key, string accept)
        {
            logger.Debug("WebSocket version 13 server handshake key: {}, response: {}.", key, accept);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToSendA413RequestEntityTooLarge(this IInternalLogger logger, Task t)
        {
            logger.Debug("Failed to send a 413 Request Entity Too Large.", t.Exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToDelete(this IInternalLogger logger, FileStream fileStream, Exception error)
        {
            logger.Warn("Failed to delete: {} {}", fileStream, error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToDeleteFile(this IInternalLogger logger, Exception error)
        {
            logger.Warn("Failed to delete file.", error);
        }
    }
}
