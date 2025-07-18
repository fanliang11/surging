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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets.Extensions.Compression
{
    using System;
    using System.Collections.Generic;

    using static DeflateFrameServerExtensionHandshaker;

    /// <summary>
    /// <a href="https://tools.ietf.org/id/draft-tyoshino-hybi-websocket-perframe-deflate-06.txt">perframe-deflate</a>
    /// handshake implementation.
    /// </summary>
    public sealed class DeflateFrameClientExtensionHandshaker : IWebSocketClientExtensionHandshaker
    {
        private readonly int _compressionLevel;
        private readonly bool _useWebkitExtensionName;
        private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

        /// <summary>
        /// Constructor with default configuration.
        /// </summary>
        /// <param name="useWebkitExtensionName"></param>
        public DeflateFrameClientExtensionHandshaker(bool useWebkitExtensionName)
            : this(6, useWebkitExtensionName, WebSocketExtensionFilterProvider.Default)
        {
        }

        /// <summary>
        /// Constructor with custom configuration.
        /// </summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="useWebkitExtensionName"></param>
        public DeflateFrameClientExtensionHandshaker(int compressionLevel, bool useWebkitExtensionName)
            : this(compressionLevel, useWebkitExtensionName, WebSocketExtensionFilterProvider.Default)
        {
        }

        /// <summary>
        /// Constructor with custom configuration.
        /// </summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="useWebkitExtensionName"></param>
        /// <param name="extensionFilterProvider">provides client extension filters for per frame deflate encoder and decoder.</param>
        public DeflateFrameClientExtensionHandshaker(int compressionLevel, bool useWebkitExtensionName,
            IWebSocketExtensionFilterProvider extensionFilterProvider)
        {
            if (/*compressionLevel < 0 || */(uint)compressionLevel > 9u)
            {
                ThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel);
            }
            if (extensionFilterProvider is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionFilterProvider); }

            _compressionLevel = compressionLevel;
            _useWebkitExtensionName = useWebkitExtensionName;
            _extensionFilterProvider = extensionFilterProvider;
        }

        public WebSocketExtensionData NewRequestData() => new WebSocketExtensionData(
            _useWebkitExtensionName ? XWebkitDeflateFrameExtension : DeflateFrameExtension,
            new Dictionary<string, string>(StringComparer.Ordinal));

        public IWebSocketClientExtension HandshakeExtension(WebSocketExtensionData extensionData)
        {
            if ((uint)extensionData.Parameters.Count > 0u) { return null; }

            var extensionDataName = extensionData.Name;
            switch (extensionDataName)
            {
                case XWebkitDeflateFrameExtension:
                case DeflateFrameExtension:
                    return new DeflateFrameClientExtension(_compressionLevel, _extensionFilterProvider);

                default:
                    if (string.Equals(XWebkitDeflateFrameExtension, extensionDataName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(DeflateFrameExtension, extensionDataName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new DeflateFrameClientExtension(_compressionLevel, _extensionFilterProvider);
                    }
                    return null;
            }
        }

        sealed class DeflateFrameClientExtension : IWebSocketClientExtension
        {
            private readonly int _compressionLevel;
            private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

            public DeflateFrameClientExtension(int compressionLevel, IWebSocketExtensionFilterProvider extensionFilterProvider)
            {
                _compressionLevel = compressionLevel;
                _extensionFilterProvider = extensionFilterProvider;
            }

            public int Rsv => WebSocketRsv.Rsv1;

            public WebSocketExtensionEncoder NewExtensionEncoder()
                => new PerFrameDeflateEncoder(_compressionLevel, 15, false, _extensionFilterProvider.EncoderFilter);

            public WebSocketExtensionDecoder NewExtensionDecoder()
                => new PerFrameDeflateDecoder(false, _extensionFilterProvider.DecoderFilter);
        }
    }
}
