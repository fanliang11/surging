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
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets
{
    using System;

    /// <summary>
    /// WebSocket client configuration.
    /// </summary>
    public class WebSocketClientProtocolConfig
    {
        internal const long DefaultHandshakeTimeoutMillis = WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis;
        internal const bool DefaultPerformMasking = true;
        internal const bool DefaultAllowMaskMismatch = false;
        internal const bool DefaultHandleCloseFrames = true;
        internal const bool DefaultDropPongFrames = true;

        private WebSocketClientProtocolConfig(
            Uri webSocketUri,
            string subprotocol,
            WebSocketVersion version,
            bool allowExtensions,
            HttpHeaders customHeaders,
            int maxFramePayloadLength,
            bool performMasking,
            bool allowMaskMismatch,
            bool handleCloseFrames,
            WebSocketCloseStatus sendCloseFrame,
            bool dropPongFrames,
            long handshakeTimeoutMillis,
            long forceCloseTimeoutMillis,
            bool absoluteUpgradeUrl,
            bool withUTF8Validator
        )
        {
            if (handshakeTimeoutMillis <= 0L) { ThrowHelper.ThrowArgumentException_Positive(ExceptionArgument.handshakeTimeoutMillis); }

            WebSocketUri = webSocketUri;
            Subprotocol = subprotocol;
            Version = version;
            AllowExtensions = allowExtensions;
            CustomHeaders = customHeaders;
            MaxFramePayloadLength = maxFramePayloadLength;
            PerformMasking = performMasking;
            AllowMaskMismatch = allowMaskMismatch;
            ForceCloseTimeoutMillis = forceCloseTimeoutMillis;
            HandleCloseFrames = handleCloseFrames;
            SendCloseFrame = sendCloseFrame;
            DropPongFrames = dropPongFrames;
            HandshakeTimeoutMillis = handshakeTimeoutMillis;
            AbsoluteUpgradeUrl = absoluteUpgradeUrl;
            WithUTF8Validator = withUTF8Validator;
        }

        /// <summary>
        /// URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.
        /// </summary>
        public readonly Uri WebSocketUri;

        /// <summary>
        /// Sub protocol request sent to the server.
        /// </summary>
        public readonly string Subprotocol;

        /// <summary>
        /// Version of web socket specification to use to connect to the server
        /// </summary>
        public readonly WebSocketVersion Version;

        /// <summary>
        /// Allow extensions to be used in the reserved bits of the web socket frame
        /// </summary>
        public readonly bool AllowExtensions;

        /// <summary>
        /// Map of custom headers to add to the client request
        /// </summary>
        public readonly HttpHeaders CustomHeaders;

        /// <summary>
        /// Maximum length of a frame's payload
        /// </summary>
        public readonly int MaxFramePayloadLength;

        /// <summary>
        /// Whether to mask all written websocket frames.This must be set to true in order to be fully compatible
        /// with the websocket specifications.Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.
        /// </summary>
        public readonly bool PerformMasking;

        /// <summary>
        /// When set to true, frames which are not masked properly according to the standard will still be accepted.
        /// </summary>
        public readonly bool AllowMaskMismatch;

        /// <summary>
        /// <c>true</c> if close frames should not be forwarded and just close the channel
        /// </summary>
        public readonly bool HandleCloseFrames;

        /// <summary>
        /// Close frame to send, when close frame was not send manually. Or <c>null</c> to disable proper close.
        /// </summary>
        public readonly WebSocketCloseStatus SendCloseFrame;

        /// <summary>
        /// <c>true</c> if pong frames should not be forwarded
        /// </summary>
        public readonly bool DropPongFrames;

        /// <summary>
        /// Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="WebSocketClientProtocolHandler.ClientHandshakeStateEvent.HandshakeTimeout"/>
        /// </summary>
        public readonly long HandshakeTimeoutMillis;

        /// <summary>
        /// Close the connection if it was not closed by the server after timeout specified
        /// </summary>
        public readonly long ForceCloseTimeoutMillis;

        /// <summary>
        /// Use an absolute url for the Upgrade request, typically when connecting through an HTTP proxy over clear HTTP
        /// </summary>
        public readonly bool AbsoluteUpgradeUrl;

        /// <summary>
        /// Allows you to avoid adding of Utf8FrameValidator to the pipeline on the
        /// <see cref="WebSocketClientProtocolHandler"/> creation. This is useful (less overhead)
        /// when you use only BinaryWebSocketFrame within your web socket connection.
        /// </summary>
        public bool WithUTF8Validator;

        public override string ToString()
        {
            return "WebSocketClientProtocolConfig" +
                " {webSocketUri=" + WebSocketUri +
                ", subprotocol=" + Subprotocol +
                ", version=" + Version +
                ", allowExtensions=" + AllowExtensions +
                ", customHeaders=" + CustomHeaders +
                ", maxFramePayloadLength=" + MaxFramePayloadLength +
                ", performMasking=" + PerformMasking +
                ", allowMaskMismatch=" + AllowMaskMismatch +
                ", handleCloseFrames=" + HandleCloseFrames +
                ", sendCloseFrame=" + SendCloseFrame +
                ", dropPongFrames=" + DropPongFrames +
                ", handshakeTimeoutMillis=" + HandshakeTimeoutMillis +
                ", forceCloseTimeoutMillis=" + ForceCloseTimeoutMillis +
                ", absoluteUpgradeUrl=" + AbsoluteUpgradeUrl +
                "}";
        }

        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        public static Builder NewBuilder()
        {
            return new Builder(
                    new Uri("https://localhost/"),
                    null,
                    WebSocketVersion.V13,
                    false,
                    EmptyHttpHeaders.Default,
                    65536,
                    DefaultPerformMasking,
                    DefaultAllowMaskMismatch,
                    DefaultHandleCloseFrames,
                    WebSocketCloseStatus.NormalClosure,
                    DefaultDropPongFrames,
                    DefaultHandshakeTimeoutMillis,
                    -1,
                    false,
                    true);
        }

        public sealed class Builder
        {
            private Uri _webSocketUri;
            private string _subprotocol;
            private WebSocketVersion _version;
            private bool _allowExtensions;
            private HttpHeaders _customHeaders;
            private int _maxFramePayloadLength;
            private bool _performMasking;
            private bool _allowMaskMismatch;
            private bool _handleCloseFrames;
            private WebSocketCloseStatus _sendCloseFrame;
            private bool _dropPongFrames;
            private long _handshakeTimeoutMillis;
            private long _forceCloseTimeoutMillis;
            private bool _absoluteUpgradeUrl;
            private bool _withUTF8Validator;

            internal Builder(WebSocketClientProtocolConfig clientConfig)
            {
                if (clientConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.clientConfig); }

                _webSocketUri = clientConfig.WebSocketUri;
                _subprotocol = clientConfig.Subprotocol;
                _version = clientConfig.Version;
                _allowExtensions = clientConfig.AllowExtensions;
                _customHeaders = clientConfig.CustomHeaders;
                _maxFramePayloadLength = clientConfig.MaxFramePayloadLength;
                _performMasking = clientConfig.PerformMasking;
                _allowMaskMismatch = clientConfig.AllowMaskMismatch;
                _handleCloseFrames = clientConfig.HandleCloseFrames;
                _sendCloseFrame = clientConfig.SendCloseFrame;
                _dropPongFrames = clientConfig.DropPongFrames;
                _handshakeTimeoutMillis = clientConfig.HandshakeTimeoutMillis;
                _forceCloseTimeoutMillis = clientConfig.ForceCloseTimeoutMillis;
                _absoluteUpgradeUrl = clientConfig.AbsoluteUpgradeUrl;
                _withUTF8Validator = clientConfig.WithUTF8Validator;
            }

            internal Builder(
                Uri webSocketUri,
                string subprotocol,
                WebSocketVersion version,
                bool allowExtensions,
                HttpHeaders customHeaders,
                int maxFramePayloadLength,
                bool performMasking,
                bool allowMaskMismatch,
                bool handleCloseFrames,
                WebSocketCloseStatus sendCloseFrame,
                bool dropPongFrames,
                long handshakeTimeoutMillis,
                long forceCloseTimeoutMillis,
                bool absoluteUpgradeUrl,
                bool withUTF8Validator)
            {
                _webSocketUri = webSocketUri;
                _subprotocol = subprotocol;
                _version = version;
                _allowExtensions = allowExtensions;
                _customHeaders = customHeaders;
                _maxFramePayloadLength = maxFramePayloadLength;
                _performMasking = performMasking;
                _allowMaskMismatch = allowMaskMismatch;
                _handleCloseFrames = handleCloseFrames;
                _sendCloseFrame = sendCloseFrame;
                _dropPongFrames = dropPongFrames;
                _handshakeTimeoutMillis = handshakeTimeoutMillis;
                _forceCloseTimeoutMillis = forceCloseTimeoutMillis;
                _absoluteUpgradeUrl = absoluteUpgradeUrl;
                _withUTF8Validator = withUTF8Validator;
            }

            /// <summary>
            /// URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
            /// sent to this URL.
            /// </summary>
            public Builder WebSocketUri(string webSocketUri)
            {
                return WebSocketUri(new Uri(webSocketUri, UriKind.Absolute));
            }

            /// <summary>
            /// URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
            /// sent to this URL.
            /// </summary>
            public Builder WebSocketUri(Uri webSocketUri)
            {
                _webSocketUri = webSocketUri;
                return this;
            }

            /// <summary>
            /// Sub protocol request sent to the server.
            /// </summary>
            public Builder Subprotocol(String subprotocol)
            {
                _subprotocol = subprotocol;
                return this;
            }

            /// <summary>
            /// Version of web socket specification to use to connect to the server
            /// </summary>
            public Builder Version(WebSocketVersion version)
            {
                _version = version;
                return this;
            }

            /// <summary>
            /// Allow extensions to be used in the reserved bits of the web socket frame
            /// </summary>
            public Builder AllowExtensions(bool allowExtensions)
            {
                _allowExtensions = allowExtensions;
                return this;
            }

            /// <summary>
            /// Map of custom headers to add to the client request
            /// </summary>
            public Builder CustomHeaders(HttpHeaders customHeaders)
            {
                _customHeaders = customHeaders;
                return this;
            }

            /// <summary>
            /// Maximum length of a frame's payload
            /// </summary>
            public Builder MaxFramePayloadLength(int maxFramePayloadLength)
            {
                _maxFramePayloadLength = maxFramePayloadLength;
                return this;
            }

            /// <summary>
            /// Whether to mask all written websocket frames.This must be set to true in order to be fully compatible
            /// with the websocket specifications.Client applications that communicate with a non-standard server
            /// which doesn't require masking might set this to false to achieve a higher performance.
            /// </summary>
            public Builder PerformMasking(bool performMasking)
            {
                _performMasking = performMasking;
                return this;
            }

            /// <summary>
            /// When set to true, frames which are not masked properly according to the standard will still be accepted.
            /// </summary>
            public Builder AllowMaskMismatch(bool allowMaskMismatch)
            {
                _allowMaskMismatch = allowMaskMismatch;
                return this;
            }

            /// <summary>
            /// <c>true</c> if close frames should not be forwarded and just close the channel
            /// </summary>
            public Builder HandleCloseFrames(bool handleCloseFrames)
            {
                _handleCloseFrames = handleCloseFrames;
                return this;
            }

            /// <summary>
            /// Close frame to send, when close frame was not send manually. Or <c>null</c> to disable proper close.
            /// </summary>
            public Builder SendCloseFrame(WebSocketCloseStatus sendCloseFrame)
            {
                _sendCloseFrame = sendCloseFrame;
                return this;
            }

            /// <summary>
            /// <c>true</c> if pong frames should not be forwarded
            /// </summary>
            public Builder DropPongFrames(bool dropPongFrames)
            {
                _dropPongFrames = dropPongFrames;
                return this;
            }

            /// <summary>
            /// Handshake timeout in mills, when handshake timeout, will trigger user
            /// event <see cref="WebSocketClientProtocolHandler.ClientHandshakeStateEvent.HandshakeTimeout"/>
            /// </summary>
            public Builder HandshakeTimeoutMillis(long handshakeTimeoutMillis)
            {
                _handshakeTimeoutMillis = handshakeTimeoutMillis;
                return this;
            }

            /// <summary>
            /// Close the connection if it was not closed by the server after timeout specified
            /// </summary>
            public Builder ForceCloseTimeoutMillis(long forceCloseTimeoutMillis)
            {
                _forceCloseTimeoutMillis = forceCloseTimeoutMillis;
                return this;
            }

            /// <summary>
            /// Use an absolute url for the Upgrade request, typically when connecting through an HTTP proxy over clear HTTP
            /// </summary>
            public Builder AbsoluteUpgradeUrl(bool absoluteUpgradeUrl)
            {
                _absoluteUpgradeUrl = absoluteUpgradeUrl;
                return this;
            }

            /// <summary>
            /// Allows you to avoid adding of Utf8FrameValidator to the pipeline on the
            /// <see cref="WebSocketClientProtocolHandler"/> creation. This is useful (less overhead)
            /// when you use only BinaryWebSocketFrame within your web socket connection.
            /// </summary>
            public Builder WithUTF8Validator(bool withUTF8Validator)
            {
                _withUTF8Validator = withUTF8Validator;
                return this;
            }

            /// <summary>
            /// Build unmodifiable client protocol configuration.
            /// </summary>
            public WebSocketClientProtocolConfig Build()
            {
                return new WebSocketClientProtocolConfig(
                    _webSocketUri,
                    _subprotocol,
                    _version,
                    _allowExtensions,
                    _customHeaders,
                    _maxFramePayloadLength,
                    _performMasking,
                    _allowMaskMismatch,
                    _handleCloseFrames,
                    _sendCloseFrame,
                    _dropPongFrames,
                    _handshakeTimeoutMillis,
                    _forceCloseTimeoutMillis,
                    _absoluteUpgradeUrl,
                    _withUTF8Validator
                );
            }
        }
    }
}
