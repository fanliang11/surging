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

namespace DotNetty.Codecs.Http.WebSockets
{
    /// <summary>
    /// Decodes a web socket frame from wire protocol version 7 format. V7 is essentially the same as V8.
    /// </summary>
    public class WebSocket07FrameDecoder : WebSocket08FrameDecoder
    {
        /// <summary>Constructor</summary>
        /// <param name="expectMaskedFrames">Web socket servers must set this to true processed incoming masked payload. Client implementations
        /// must set this to false.</param>
        /// <param name="allowExtensions">Flag to allow reserved extension bits to be used or not</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload. Setting this to an appropriate value for you application
        /// helps check for denial of services attacks.</param>
        public WebSocket07FrameDecoder(bool expectMaskedFrames, bool allowExtensions, int maxFramePayloadLength)
            : this(WebSocketDecoderConfig.NewBuilder()
                .ExpectMaskedFrames(expectMaskedFrames)
                .AllowExtensions(allowExtensions)
                .MaxFramePayloadLength(maxFramePayloadLength)
                .Build())
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="expectMaskedFrames">Web socket servers must set this to true processed incoming masked payload. Client implementations
        /// must set this to false.</param>
        /// <param name="allowExtensions">Flag to allow reserved extension bits to be used or not</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload. Setting this to an appropriate value for you application
        /// helps check for denial of services attacks.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        public WebSocket07FrameDecoder(bool expectMaskedFrames, bool allowExtensions, int maxFramePayloadLength, bool allowMaskMismatch)
            : this(WebSocketDecoderConfig.NewBuilder()
                .ExpectMaskedFrames(expectMaskedFrames)
                .AllowExtensions(allowExtensions)
                .MaxFramePayloadLength(maxFramePayloadLength)
                .AllowMaskMismatch(allowMaskMismatch)
                .Build())
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="decoderConfig">Frames decoder configuration.</param>
        public WebSocket07FrameDecoder(WebSocketDecoderConfig decoderConfig)
            : base(decoderConfig)
        {
        }
    }
}
