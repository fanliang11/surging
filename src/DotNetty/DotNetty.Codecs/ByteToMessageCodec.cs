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

namespace DotNetty.Codecs
{
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// A Codec for on-the-fly encoding/decoding of bytes to messages and vise-versa.
    ///
    /// This can be thought of as a combination of <see cref="ByteToMessageDecoder"/> and <see cref="MessageToByteEncoder{T}"/>.
    ///
    /// Be aware that sub-classes of <see cref="ByteToMessageCodec{T}"/> <strong>MUST NOT</strong>
    /// annotated with <see cref="ChannelHandlerAdapter.IsSharable"/>.
    /// </summary>
    public abstract class ByteToMessageCodec<T> : ChannelDuplexHandler
    {
        private readonly Decoder _decoder;
        private readonly Encoder _encoder;

        sealed class Decoder : ByteToMessageDecoder
        {
            private readonly ByteToMessageCodec<T> _codec;

            public Decoder(ByteToMessageCodec<T> codec) => _codec = codec;

            protected internal override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
            {
                _codec.Decode(context, input, output);
            }

            protected override void DecodeLast(IChannelHandlerContext context, IByteBuffer input, List<object> output)
            {
                _codec.DecodeLast(context, input, output);
            }
        }

        sealed class Encoder : MessageToByteEncoder<T>
        {
            private readonly ByteToMessageCodec<T> _codec;

            public Encoder(ByteToMessageCodec<T> codec) => _codec = codec;

            public override bool AcceptOutboundMessage(object message)
            {
                return _codec.AcceptOutboundMessage(message);
            }

            protected override void Encode(IChannelHandlerContext context, T message, IByteBuffer output)
            {
                _codec.Encode(context, message, output);
            }
        }

        /// <summary>Create a new instance which will try to detect the types to match out of the type parameter of the class.</summary>
        public ByteToMessageCodec()
        {
            if (IsSharable)
            {
                CThrowHelper.ThrowInvalidOperationException_ByteToMessageDecoder();
            }
            _decoder = new Decoder(this);
            _encoder = new Encoder(this);
        }

        /// <inheritdoc />
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            _decoder.ChannelRead(context, message);
        }

        /// <inheritdoc />
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            _decoder.ChannelReadComplete(context);
        }

        /// <inheritdoc />
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _decoder.ChannelInactive(context);
        }

        /// <inheritdoc />
        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            _encoder.Write(context, message, promise);
        }

        /// <inheritdoc />
        public override void HandlerAdded(IChannelHandlerContext context)
        {
            try
            {
                _decoder.HandlerAdded(context);
            }
            finally
            {
                _encoder.HandlerAdded(context);
            }
        }

        /// <inheritdoc />
        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            try
            {
                _decoder.HandlerRemoved(context);
            }
            finally
            {
                _encoder.HandlerRemoved(context);
            }
            HandlerRemovedInternal(context);
        }

        /// <summary>
        /// Gets called after the <see cref="ByteToMessageDecoder"/> was removed from the actual context and it doesn't handle
        /// events anymore.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void HandlerRemovedInternal(IChannelHandlerContext context)
        {
        }

        /// <inheritdoc />
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            _decoder.UserEventTriggered(context, evt);
        }

        /// <inheritdoc cref="MessageToByteEncoder{T}.AcceptOutboundMessage(object)"/>
        public virtual bool AcceptOutboundMessage(object message) => message is T;

        /// <inheritdoc cref="MessageToByteEncoder{T}.Encode(IChannelHandlerContext, T, IByteBuffer)"/>
        protected abstract void Encode(IChannelHandlerContext context, T message, IByteBuffer output);

        /// <inheritdoc cref="ByteToMessageDecoder.Decode(IChannelHandlerContext, IByteBuffer, List{object})"/>
        protected abstract void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output);

        /// <inheritdoc cref="ByteToMessageDecoder.DecodeLast(IChannelHandlerContext, IByteBuffer, List{object})"/>
        protected virtual void DecodeLast(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (input.IsReadable())
            {
                // Only call decode() if there is something left in the buffer to decode.
                // See https://github.com/netty/netty/issues/4386
                Decode(context, input, output);
            }
        }
    }
}
