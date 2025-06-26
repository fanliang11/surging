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

    /// <summary>
    /// <a href="https://tools.ietf.org/id/draft-tyoshino-hybi-websocket-perframe-deflate-06.txt">perframe-deflate</a>
    /// handshake implementation.
    /// </summary>
    public sealed class DeflateFrameServerExtensionHandshaker : IWebSocketServerExtensionHandshaker
    {
        internal const string XWebkitDeflateFrameExtension = "x-webkit-deflate-frame";
        internal const string DeflateFrameExtension = "deflate-frame";

        private readonly int _compressionLevel;
        private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

        /// <summary>
        /// Constructor with default configuration.
        /// </summary>
        public DeflateFrameServerExtensionHandshaker()
            : this(6, WebSocketExtensionFilterProvider.Default)
        {
        }

        /// <summary>
        /// Constructor with custom configuration.
        /// </summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        public DeflateFrameServerExtensionHandshaker(int compressionLevel)
            : this(compressionLevel, WebSocketExtensionFilterProvider.Default)
        {
        }

        /// <summary>
        /// Constructor with custom configuration.
        /// </summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="extensionFilterProvider">provides client extension filters for per frame deflate encoder and decoder.</param>
        public DeflateFrameServerExtensionHandshaker(int compressionLevel, IWebSocketExtensionFilterProvider extensionFilterProvider)
        {
            if (/*compressionLevel < 0 || */(uint)compressionLevel > 9u)
            {
                ThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel);
            }
            if (extensionFilterProvider is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionFilterProvider); }

            _compressionLevel = compressionLevel;
            _extensionFilterProvider = extensionFilterProvider;
        }

        public IWebSocketServerExtension HandshakeExtension(WebSocketExtensionData extensionData)
        {
            if ((uint)extensionData.Parameters.Count > 0u) { return null; }

            var extensionDataName = extensionData.Name;
            switch (extensionDataName)
            {
                case XWebkitDeflateFrameExtension:
                case DeflateFrameExtension:
                    return new DeflateFrameServerExtension(_compressionLevel, extensionDataName, _extensionFilterProvider);

                default:
                    if (string.Equals(XWebkitDeflateFrameExtension, extensionDataName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(DeflateFrameExtension, extensionDataName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new DeflateFrameServerExtension(_compressionLevel, extensionDataName, _extensionFilterProvider);
                    }
                    return null;
            }
        }

        sealed class DeflateFrameServerExtension : IWebSocketServerExtension
        {
            private readonly string _extensionName;
            private readonly int _compressionLevel;
            private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

            public DeflateFrameServerExtension(int compressionLevel, string extensionName, IWebSocketExtensionFilterProvider extensionFilterProvider)
            {
                _extensionName = extensionName;
                _compressionLevel = compressionLevel;
                _extensionFilterProvider = extensionFilterProvider;
            }

            public int Rsv => WebSocketRsv.Rsv1;

            public WebSocketExtensionEncoder NewExtensionEncoder()
                => new PerFrameDeflateEncoder(_compressionLevel, 15, false, _extensionFilterProvider.EncoderFilter);

            public WebSocketExtensionDecoder NewExtensionDecoder()
                => new PerFrameDeflateDecoder(false, _extensionFilterProvider.DecoderFilter);

            public WebSocketExtensionData NewReponseData()
                => new WebSocketExtensionData(_extensionName, new Dictionary<string, string>(StringComparer.Ordinal));
        }
    }
}
