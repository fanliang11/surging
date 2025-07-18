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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Handlers.Flow
{
    using System;
    using DotNetty.Codecs;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// The <see cref="FlowControlHandler"/> ensures that only one message per {@code read()} is sent downstream.
    ///
    /// Classes such as <see cref="ByteToMessageDecoder"/> or <see cref="MessageToByteEncoder{T}"/> are free to emit as
    /// many events as they like for any given input. A channel's auto reading configuration doesn't usually
    /// apply in these scenarios. This is causing problems in downstream <see cref="IChannelHandler"/>s that would
    /// like to hold subsequent events while they're processing one event. It's a common problem with the
    /// <see cref="T:DotNetty.Codecs.Http.HttpObjectDecoder"/> that will very often fire an
    /// <see cref="T:DotNetty.Codecs.Http.IHttpRequest"/> that is immediately followed
    /// by a <see cref="T:DotNetty.Codecs.Http.ILastHttpContent"/> event.
    ///
    /// <code>
    /// ChannelPipeline pipeline = ...;
    ///
    /// pipeline.addLast(new HttpServerCodec());
    /// pipeline.addLast(new FlowControlHandler());
    ///
    /// pipeline.addLast(new MyExampleHandler());
    ///
    /// class MyExampleHandler extends ChannelInboundHandlerAdapter {
    ///   @Override
    ///   public void channelRead(ChannelHandlerContext ctx, Object msg) {
    ///     if (msg instanceof HttpRequest) {
    ///       ctx.channel().config().setAutoRead(false);
    ///
    ///       // The FlowControlHandler will hold any subsequent events that
    ///       // were emitted by HttpObjectDecoder until auto reading is turned
    ///       // back on or Channel#read() is being called.
    ///     }
    ///   }
    /// }
    /// }</code>
    ///
    /// @see ChannelConfig#setAutoRead(boolean)
    /// </summary>
    public class FlowControlHandler : ChannelDuplexHandler
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<FlowControlHandler>();

        private static readonly ThreadLocalPool<RecyclableQueue> Recycler = new ThreadLocalPool<RecyclableQueue>(h => new RecyclableQueue(h));

        private readonly bool _releaseMessages;

        private RecyclableQueue _queue;

        private IChannelConfiguration _config;

        private bool _shouldConsume;

        /// <summary>Create new instance.</summary>
        public FlowControlHandler()
            : this(true)
        {
        }

        /// <summary>Create new instance.</summary>
        /// <param name="releaseMessages">If <c>false</c>, the handler won't release the buffered messages
        /// when the handler is removed.</param>
        public FlowControlHandler(bool releaseMessages)
        {
            _releaseMessages = releaseMessages;
        }

        /**
         * Determine if the underlying {@link Queue} is empty. This method exists for
         * testing, debugging and inspection purposes and it is not Thread safe!
         */
        public bool IsQueueEmpty => _queue is null || _queue.IsEmpty;

        /**
         * Releases all messages and destroys the {@link Queue}.
         */
        void Destroy()
        {
            if (_queue is object)
            {
                if (_queue.NonEmpty)
                {
#if DEBUG
                    if (Logger.TraceEnabled) Logger.NonEmptyQueue(_queue);
#endif

                    if (_releaseMessages)
                    {
                        while (_queue.TryDequeue(out object msg))
                        {
                            ReferenceCountUtil.SafeRelease(msg);
                        }
                    }
                }

                _queue.Recycle();
                _queue = null;
            }
        }

        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            _config = ctx.Channel.Configuration;
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            base.HandlerRemoved(ctx);
            if (!IsQueueEmpty)
            {
                Dequeue(ctx, _queue.Count);
            }
            Destroy();
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Destroy();
            _ = ctx.FireChannelInactive();
        }

        public override void Read(IChannelHandlerContext ctx)
        {
            if (0u >= (uint)Dequeue(ctx, 1))
            {
                // It seems no messages were consumed. We need to read() some
                // messages from upstream and once one arrives it need to be
                // relayed to downstream to keep the flow going.
                _shouldConsume = true;
                _ = ctx.Read();
            }
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            if (_queue is null)
            {
                _queue = Recycler.Take();
            }

            _ = _queue.TryEnqueue(msg);

            // We just received one message. Do we need to relay it regardless
            // of the auto reading configuration? The answer is yes if this
            // method was called as a result of a prior read() call.
            int minConsume = _shouldConsume ? 1 : 0;
            _shouldConsume = false;

            _ = Dequeue(ctx, minConsume);
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            if (IsQueueEmpty)
            {
                _ = ctx.FireChannelReadComplete();
            }
            else
            {
                // Don't relay completion events from upstream as they
                // make no sense in this context. See dequeue() where
                // a new set of completion events is being produced.
            }
        }

        /**
         * Dequeues one or many (or none) messages depending on the channel's auto
         * reading state and returns the number of messages that were consumed from
         * the internal queue.
         *
         * The {@code minConsume} argument is used to force {@code dequeue()} into
         * consuming that number of messages regardless of the channel's auto
         * reading configuration.
         *
         * @see #read(ChannelHandlerContext)
         * @see #channelRead(ChannelHandlerContext, Object)
         */
        private int Dequeue(IChannelHandlerContext ctx, int minConsume)
        {
            int consumed = 0;

            // fireChannelRead(...) may call ctx.read() and so this method may reentrance. Because of this we need to
            // check if queue was set to null in the meantime and if so break the loop.
            while (_queue is object && (consumed < minConsume || _config.IsAutoRead))
            {
                if (!_queue.TryDequeue(out object msg) || msg is null) { break; }

                ++consumed;
                _ = ctx.FireChannelRead(msg);
            }

            // We're firing a completion event every time one (or more)
            // messages were consumed and the queue ended up being drained
            // to an empty state.
            if (_queue is object && _queue.IsEmpty)
            {
                _queue.Recycle();
                _queue = null;

                if (consumed > 0) { _ = ctx.FireChannelReadComplete(); }
            }

            return consumed;
        }
    }

    sealed class RecyclableQueue : CompatibleConcurrentQueue<object>
    {
        readonly ThreadLocalPool.Handle _handle;

        internal RecyclableQueue(ThreadLocalPool.Handle handle)
        {
            _handle = handle;
        }

        public void Recycle()
        {
            ((IQueue<object>)this).Clear();
            _handle.Release(this);
        }
    }
}