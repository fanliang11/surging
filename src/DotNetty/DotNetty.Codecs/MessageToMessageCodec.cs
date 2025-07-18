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

namespace DotNetty.Codecs
{
    using System.Collections.Generic;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// A Codec for on-the-fly encoding/decoding of message.
    /// 
    /// This can be thought of as a combination of <see cref="MessageToMessageDecoder{T}"/> and <see cref="MessageToMessageEncoder{T}"/>.
    /// 
    /// Here is an example of a <see cref="MessageToMessageCodec{TInbound, TOutbound}"/> which just decode from {@link Integer} to {@link Long}
    /// and encode from {@link Long} to {@link Integer}.
    /// 
    /// <![CDATA[
    ///     public class NumberCodec extends
    ///             {@link MessageToMessageCodec}&lt;{@link Integer}, {@link Long}&gt; {
    ///         {@code @Override}
    ///         public {@link Long} decode({@link ChannelHandlerContext} ctx, {@link Integer} msg, List&lt;Object&gt; out)
    ///                 throws {@link Exception} {
    ///             out.add(msg.longValue());
    ///         }
    ///
    ///         {@code @Override}
    ///         public {@link Integer} encode({@link ChannelHandlerContext} ctx, {@link Long} msg, List&lt;Object&gt; out)
    ///                 throws {@link Exception} {
    ///             out.add(msg.intValue());
    ///         }
    ///     }
    /// ]]>
    /// 
    /// Be aware that you need to call <see cref="IReferenceCounted.Retain()"/> on messages that are just passed through if they
    /// are of type <see cref="IReferenceCounted"/>. This is needed as the <see cref="MessageToMessageCodec{TInbound, TOutbound}"/> will call
    /// <see cref="IReferenceCounted.Release()"/> on encoded / decoded messages.
    /// </summary>
    /// <typeparam name="TInbound"></typeparam>
    /// <typeparam name="TOutbound"></typeparam>
    public abstract class MessageToMessageCodec<TInbound, TOutbound> : ChannelDuplexHandler
    {
        private readonly Encoder _encoder;
        private readonly Decoder _decoder;

        sealed class Encoder : MessageToMessageEncoder<TOutbound>
        {
            readonly MessageToMessageCodec<TInbound, TOutbound> _codec;

            public Encoder(MessageToMessageCodec<TInbound, TOutbound> codec)
            {
                _codec = codec;
            }

            /// <inheritdoc />
            public override bool AcceptOutboundMessage(object msg)
                => _codec.AcceptOutboundMessage(msg);

            /// <inheritdoc />
            protected internal override void Encode(IChannelHandlerContext context, TOutbound message, List<object> output)
                => _codec.Encode(context, message, output);
        }

        sealed class Decoder : MessageToMessageDecoder<TInbound>
        {
            readonly MessageToMessageCodec<TInbound, TOutbound> _codec;

            public Decoder(MessageToMessageCodec<TInbound, TOutbound> codec)
            {
                _codec = codec;
            }

            /// <inheritdoc />
            public override bool AcceptInboundMessage(object msg)
                => _codec.AcceptInboundMessage(msg);

            /// <inheritdoc />
            protected internal override void Decode(IChannelHandlerContext context, TInbound message, List<object> output)
                => _codec.Decode(context, message, output);
        }

        /// <summary>
        /// Create a new instance which will try to detect the types to decode and encode out of the type parameter
        /// of the class.
        /// </summary>
        protected MessageToMessageCodec()
        {
            _encoder = new Encoder(this);
            _decoder = new Decoder(this);
        }

        /// <inheritdoc />
        public sealed override void ChannelRead(IChannelHandlerContext context, object message)
            => _decoder.ChannelRead(context, message);

        /// <inheritdoc />
        public sealed override void Write(IChannelHandlerContext context, object message, IPromise promise)
            => _encoder.Write(context, message, promise);

        /// <summary>
        /// Returns <c>true</c> if and only if the specified message can be decoded by this codec.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public virtual bool AcceptInboundMessage(object msg) => msg is TInbound;

        /// <summary>
        /// Returns <c>true</c> if and only if the specified message can be encoded by this codec.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public virtual bool AcceptOutboundMessage(object msg) => msg is TOutbound;

        /// <summary>
        /// <see cref="MessageToMessageEncoder{T}.Encode(IChannelHandlerContext, T, List{object})"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="msg"></param>
        /// <param name="output"></param>
        protected abstract void Encode(IChannelHandlerContext ctx, TOutbound msg, List<object> output);

        /// <summary>
        /// <see cref="MessageToMessageDecoder{T}.Decode(IChannelHandlerContext, T, List{object})"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="msg"></param>
        /// <param name="output"></param>
        protected abstract void Decode(IChannelHandlerContext ctx, TInbound msg, List<object> output);
    }
}
