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
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Codecs.Compression;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Embedded;

    using static DeflateDecoder;

    /// <summary>
    /// Deflate implementation of a payload compressor for
    /// <tt>io.netty.handler.codec.http.websocketx.WebSocketFrame</tt>.
    /// </summary>
    abstract class DeflateEncoder : WebSocketExtensionEncoder
    {
        private readonly int _compressionLevel;
        private readonly int _windowSize;
        private readonly bool _noContext;
        private readonly IWebSocketExtensionFilter _extensionEncoderFilter;

        private EmbeddedChannel _encoder;

        protected DeflateEncoder(int compressionLevel, int windowSize, bool noContext, IWebSocketExtensionFilter extensionEncoderFilter)
        {
            if (extensionEncoderFilter is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionEncoderFilter); }

            _compressionLevel = compressionLevel;
            _windowSize = windowSize;
            _noContext = noContext;
            _extensionEncoderFilter = extensionEncoderFilter;
        }

        /// <summary>
        /// Returns the extension encoder filter.
        /// </summary>
        protected IWebSocketExtensionFilter ExtensionEncoderFilter => _extensionEncoderFilter;

        /// <summary>
        /// return the rsv bits to set in the compressed frame.
        /// </summary>
        /// <param name="msg">the current frame.</param>
        protected abstract int Rsv(WebSocketFrame msg);

        /// <summary>
        /// return true if compressed payload tail needs to be removed.
        /// </summary>
        /// <param name="msg">the current frame.</param>
        protected abstract bool RemoveFrameTail(WebSocketFrame msg);

        protected override void Encode(IChannelHandlerContext ctx, WebSocketFrame msg, List<object> output)
        {
            IByteBuffer compressedContent = null;
            if (msg.Content.IsReadable())
            {
                compressedContent = CompressContent(ctx, msg);
            }
            else if (msg.IsFinalFragment)
            {
                // Set empty DEFLATE block manually for unknown buffer size
                // https://tools.ietf.org/html/rfc7692#section-7.2.3.6
                compressedContent = DeflateDecoder.EmptyDeflateBlock.Duplicate();
            }
            else
            {
                ThrowHelper.ThrowCodecException_CannotCompressContentBuffer();
            }

            WebSocketFrame outMsg = null;
            switch (msg.Opcode)
            {
                case Opcode.Text:
                    outMsg = new TextWebSocketFrame(msg.IsFinalFragment, Rsv(msg), compressedContent);
                    break;
                case Opcode.Binary:
                    outMsg = new BinaryWebSocketFrame(msg.IsFinalFragment, Rsv(msg), compressedContent);
                    break;
                case Opcode.Cont:
                    outMsg = new ContinuationWebSocketFrame(msg.IsFinalFragment, Rsv(msg), compressedContent);
                    break;
                default:
                    ThrowHelper.ThrowCodecException_UnexpectedFrameType(msg);
                    break;
            }
            output.Add(outMsg);
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            Cleanup();
            base.HandlerRemoved(ctx);
        }

        private IByteBuffer CompressContent(IChannelHandlerContext ctx, WebSocketFrame msg)
        {
            if (_encoder is null)
            {
                _encoder = new EmbeddedChannel(
                    ZlibCodecFactory.NewZlibEncoder(
                        ZlibWrapper.None,
                        _compressionLevel,
                        _windowSize,
                        8));
            }

            _ = _encoder.WriteOutbound(msg.Content.Retain());

            CompositeByteBuffer fullCompressedContent = ctx.Allocator.CompositeBuffer();
            while (true)
            {
                var partCompressedContent = _encoder.ReadOutbound<IByteBuffer>();
                if (partCompressedContent is null)
                {
                    break;
                }

                if (!partCompressedContent.IsReadable())
                {
                    _ = partCompressedContent.Release();
                    continue;
                }

                _ = fullCompressedContent.AddComponent(true, partCompressedContent);
            }

            if (fullCompressedContent.NumComponents <= 0)
            {
                _ = fullCompressedContent.Release();
                ThrowHelper.ThrowCodecException_CannotReadCompressedBuf();
            }

            if (msg.IsFinalFragment && _noContext)
            {
                Cleanup();
            }

            IByteBuffer compressedContent;
            if (RemoveFrameTail(msg))
            {
                int realLength = fullCompressedContent.ReadableBytes - FrameTail.ReadableBytes;
                compressedContent = fullCompressedContent.Slice(0, realLength);
            }
            else
            {
                compressedContent = fullCompressedContent;
            }
            return compressedContent;
        }

        void Cleanup()
        {
            if (_encoder is object)
            {
                // Clean-up the previous encoder if not cleaned up correctly.
                _ = _encoder.FinishAndReleaseAll();
                _encoder = null;
            }
        }
    }
}
