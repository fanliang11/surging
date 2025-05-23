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
    /// <summary>
    /// Per-frame implementation of deflate compressor.
    /// </summary>
    sealed class PerFrameDeflateEncoder : DeflateEncoder
    {
        /// <summary>Constructor</summary>
        /// <param name="compressionLevel">compression level of the compressor.</param>
        /// <param name="windowSize">maximum size of the window compressor buffer.</param>
        /// <param name="noContext">true to disable context takeover.</param>
        public PerFrameDeflateEncoder(int compressionLevel, int windowSize, bool noContext)
            : base(compressionLevel, windowSize, noContext, NeverSkipWebSocketExtensionFilter.Instance)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="compressionLevel">compression level of the compressor.</param>
        /// <param name="windowSize">maximum size of the window compressor buffer.</param>
        /// <param name="noContext">true to disable context takeover.</param>
        /// <param name="extensionEncoderFilter">extension encoder filter for per frame deflate encoder.</param>
        public PerFrameDeflateEncoder(int compressionLevel, int windowSize, bool noContext, IWebSocketExtensionFilter extensionEncoderFilter)
            : base(compressionLevel, windowSize, noContext, extensionEncoderFilter)
        {
        }

        /// <inheritdoc />
        public override bool AcceptOutboundMessage(object msg)
        {
            if (!(msg is WebSocketFrame wsFrame)) { return false; }

            if (ExtensionEncoderFilter.MustSkip(wsFrame)) { return false; }

            switch (wsFrame.Opcode)
            {
                case Opcode.Text:
                case Opcode.Binary:
                case Opcode.Cont:
                    return (uint)wsFrame.Content.ReadableBytes > 0u && 0u >= (uint)(wsFrame.Rsv & WebSocketRsv.Rsv1);
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override int Rsv(WebSocketFrame msg) => msg.Rsv | WebSocketRsv.Rsv1;

        /// <inheritdoc />
        protected override bool RemoveFrameTail(WebSocketFrame msg) => true;
    }
}
