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


namespace DotNetty.Transport.Channels
{
    using System;
    using System.Collections.Concurrent;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Bootstrapping;

    /// <summary>
    /// A special <see cref="IChannelHandler"/> which offers an easy way to initialize a <see cref="IChannel"/> once it was
    /// registered to its <see cref="IEventLoop"/>.
    /// <para>
    /// Implementations are most often used in the context of <see cref="AbstractBootstrap{TBootstrap,TChannel}.Handler(IChannelHandler)"/>
    /// and <see cref="ServerBootstrap.ChildHandler"/> to setup the <see cref="IChannelPipeline"/> of a <see cref="IChannel"/>.
    /// </para>
    /// Be aware that this class is marked as Sharable (via <see cref="IsSharable"/>) and so the implementation must be safe to be re-used.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyChannelInitializer extends <see cref="ChannelInitializer{T}"/> {
    ///     public void InitChannel(<see cref="IChannel"/> channel) {
    ///         channel.Pipeline().AddLast("myHandler", new MyHandler());
    ///     }
    /// }
    /// <see cref="ServerBootstrap"/> bootstrap = ...;
    /// ...
    /// bootstrap.childHandler(new MyChannelInitializer());
    /// ...
    /// </code>
    /// </example>
    /// <typeparam name="T">A sub-type of <see cref="IChannel"/>.</typeparam>
    public abstract class ChannelInitializer<T> : ChannelHandlerAdapter
        where T : IChannel
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ChannelInitializer<T>>();

        private readonly ConcurrentDictionary<IChannelHandlerContext, bool> _initMap =
            new ConcurrentDictionary<IChannelHandlerContext, bool>(ChannelHandlerContextComparer.Default);

        /// <summary>
        /// This method will be called once the <see cref="IChannel"/> was registered. After the method returns this instance
        /// will be removed from the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> which was registered.</param>
        protected abstract void InitChannel(T channel);

        public override bool IsSharable => true;

        public sealed override void ChannelRegistered(IChannelHandlerContext ctx)
        {
            // Normally this method will never be called as handlerAdded(...) should call initChannel(...) and remove
            // the handler.
            if (InitChannel(ctx))
            {
                // we called InitChannel(...) so we need to call now pipeline.fireChannelRegistered() to ensure we not
                // miss an event.
                _ = ctx.Pipeline.FireChannelRegistered();

                // We are done with init the Channel, removing all the state for the Channel now.
                RemoveState(ctx);
            }
            else
            {
                // Called InitChannel(...) before which is the expected behavior, so just forward the event.
                _ = ctx.FireChannelRegistered();
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (Logger.WarnEnabled) Logger.FailedToInitializeAChannel(ctx, cause);
            _ = ctx.CloseAsync();
        }

        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            if (ctx.Channel.IsRegistered)
            {
                // This should always be true with our current DefaultChannelPipeline implementation.
                // The good thing about calling InitChannel(...) in HandlerAdded(...) is that there will be no ordering
                // surprises if a ChannelInitializer will add another ChannelInitializer. This is as all handlers
                // will be added in the expected order.
                if (InitChannel(ctx))
                {

                    // We are done with init the Channel, removing the initializer now.
                    RemoveState(ctx);
                }
            }
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            _ = _initMap.TryRemove(ctx, out _);
        }

        bool InitChannel(IChannelHandlerContext ctx)
        {
            if (_initMap.TryAdd(ctx, true)) // Guard against re-entrance.
            {
                try
                {
                    InitChannel((T)ctx.Channel);
                }
                catch (Exception cause)
                {
                    // Explicitly call exceptionCaught(...) as we removed the handler before calling initChannel(...).
                    // We do so to prevent multiple calls to initChannel(...).
                    ExceptionCaught(ctx, cause);
                }
                finally
                {
                    var pipeline = ctx.Pipeline;
                    if (pipeline.Context(this) is object)
                    {
                        _ = pipeline.Remove(this);
                    }
                }
                return true;
            }
            return false;
        }

        void RemoveState(IChannelHandlerContext ctx)
        {
            // The removal may happen in an async fashion if the EventExecutor we use does something funky.
            if (ctx.IsRemoved)
            {
                _ = _initMap.TryRemove(ctx, out _);
            }
            else
            {
                // The context is not removed yet which is most likely the case because a custom EventExecutor is used.
                // Let's schedule it on the EventExecutor to give it some more time to be completed in case it is offloaded.
                ctx.Executor.Execute(() => _initMap.TryRemove(ctx, out _));
            }
        }
    }
}