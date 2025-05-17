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
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Enables a <see cref="IChannelHandler"/> to interact with its <see cref="IChannelPipeline"/>
    /// and other handlers. Among other things a handler can notify the next <see cref="IChannelHandler"/> in the
    /// <see cref="IChannelPipeline"/> as well as modify the <see cref="IChannelPipeline"/> it belongs to dynamically.
    ///
    /// <h3>Notify</h3>
    ///
    /// You can notify the closest handler in the same <see cref="IChannelPipeline"/> by calling one of the various methods
    /// provided here.
    ///
    /// Please refer to <see cref="IChannelPipeline"/> to understand how an event flows.
    ///
    /// <h3>Modifying a pipeline</h3>
    ///
    /// You can get the <see cref="IChannelPipeline"/> your handler belongs to by calling
    /// <see cref="Pipeline"/>.  A non-trivial application could insert, remove, or
    /// replace handlers in the pipeline dynamically at runtime.
    ///
    /// <h3>Retrieving for later use</h3>
    ///
    /// You can keep the <see cref="IChannelHandlerContext"/> for later use, such as
    /// triggering an event outside the handler methods, even from a different thread.
    /// <code>
    /// public class MyHandler extends <see cref="ChannelDuplexHandler"/> {
    ///
    ///     <b>private <see cref="IChannelHandlerContext"/> ctx;</b>
    ///
    ///     public void beforeAdd(<see cref="IChannelHandlerContext"/> ctx) {
    ///         <b>this.ctx = ctx;</b>
    ///     }
    ///
    ///     public void login(String username, password) {
    ///         ctx.write(new LoginMessage(username, password));
    ///     }
    ///     ...
    /// }
    /// </code>
    ///
    /// <h3>Storing stateful information</h3>
    ///
    /// <see cref="IAttributeMap.GetAttribute{T}(AttributeKey{T})"/> allow you to
    /// store and access stateful information that is related with a handler and its
    /// context.  Please refer to <see cref="IChannelHandler"/> to learn various recommended
    /// ways to manage stateful information.
    ///
    /// <h3>A handler can have more than one context</h3>
    ///
    /// Please note that a <see cref="IChannelHandler"/> instance can be added to more than
    /// one <see cref="IChannelPipeline"/>.  It means a single <see cref="IChannelHandler"/>
    /// instance can have more than one <see cref="IChannelHandlerContext"/> and therefore
    /// the single instance can be invoked with different
    /// <see cref="IChannelHandlerContext"/>s if it is added to one or more
    /// <see cref="IChannelPipeline"/>s more than once.
    /// <para>
    /// For example, the following handler will have as many independent <see cref="AttributeKey{T}"/>s
    /// as how many times it is added to pipelines, regardless if it is added to the
    /// same pipeline multiple times or added to different pipelines multiple times:
    /// </para>
    /// <code>
    /// public class FactorialHandler extends {@link ChannelInboundHandlerAdapter} {
    ///
    ///   private final <see cref="AttributeKey{T}"/>&lt;{@link Integer}&gt; counter = <see cref="AttributeKey{T}"/>.valueOf("counter");
    ///
    ///   // This handler will receive a sequence of increasing integers starting
    ///   // from 1.
    ///   {@code @Override}
    ///   public void channelRead({@link ChannelHandlerContext} ctx, Object msg) {
    ///     Integer a = ctx.attr(counter).get();
    ///
    ///     if (a == null) {
    ///       a = 1;
    ///     }
    ///
    ///     attr.set(a /// (Integer) msg);
    ///   }
    /// }
    ///
    /// // Different context objects are given to "f1", "f2", "f3", and "f4" even if
    /// // they refer to the same handler instance.  Because the FactorialHandler
    /// // stores its state in a context object (using an <see cref="AttributeKey{T}"/>), the factorial is
    /// // calculated correctly 4 times once the two pipelines (p1 and p2) are active.
    /// FactorialHandler fh = new FactorialHandler();
    ///
    /// <see cref="IChannelPipeline"/> p1 = {@link Channels}.pipeline();
    /// p1.addLast("f1", fh);
    /// p1.addLast("f2", fh);
    ///
    /// <see cref="IChannelPipeline"/> p2 = {@link Channels}.pipeline();
    /// p2.addLast("f3", fh);
    /// p2.addLast("f4", fh);
    /// </code>
    ///
    /// <h3>Additional resources worth reading</h3>
    /// <para>
    /// Please refer to the <see cref="IChannelHandler"/>, and
    /// <see cref="IChannelPipeline"/> to find out more about inbound and outbound operations,
    /// what fundamental differences they have, how they flow in a  pipeline,  and how to handle
    /// the operation in your application.
    /// </para>
    /// </summary>
    public interface IChannelHandlerContext : IAttributeMap
    {
        /// <summary>
        /// Return the <see cref="IChannel"/> which is bound to the <see cref="IChannelHandlerContext"/>.
        /// </summary>
        IChannel Channel { get; }

        /// <summary>
        /// Return the assigned <see cref="IByteBufferAllocator"/> which will be used to allocate <see cref="IByteBuffer"/>s.
        /// </summary>
        IByteBufferAllocator Allocator { get; }

        /// <summary>
        /// Returns the <see cref="IEventExecutor"/> which is used to execute an arbitrary task.
        /// </summary>
        IEventExecutor Executor { get; }

        /// <summary>
        /// The unique name of the <see cref="IChannelHandlerContext"/>.
        /// </summary>
        /// <remarks>
        /// The name was used when the <see cref="IChannelHandler"/> was added to the <see cref="IChannelPipeline"/>.
        /// This name can also be used to access the registered <see cref="IChannelHandler"/> from the
        /// <see cref="IChannelPipeline"/>.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// The <see cref="IChannelHandler"/> that is bound this <see cref="IChannelHandlerContext"/>.
        /// </summary>
        IChannelHandler Handler { get; }

        /// <summary>
        /// Return <c>true</c> if the <see cref="IChannelHandler"/> which belongs to this context was removed
        /// from the <see cref="IChannelPipeline"/>. Note that this method is only meant to be called from with in the
        /// <see cref="IEventLoop"/>.
        /// </summary>
        [Obsolete("Please use IsRemoved instead.")]
        bool Removed { get; }

        /// <summary>
        /// Return <c>true</c> if the <see cref="IChannelHandler"/> which belongs to this context was removed
        /// from the <see cref="IChannelPipeline"/>. Note that this method is only meant to be called from with in the
        /// <see cref="IEventLoop"/>.
        /// </summary>
        bool IsRemoved { get; }

        /// <summary>
        /// A <see cref="IChannel"/> was registered to its <see cref="IEventLoop"/>. This will result in having the
        /// <see cref="IChannelHandler.ChannelRegistered"/> method called of the next <see cref="IChannelHandler"/>
        /// contained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <returns>The current <see cref="IChannelHandlerContext"/>.</returns>
        IChannelHandlerContext FireChannelRegistered();

        /// <summary>
        /// A <see cref="IChannel"/> was unregistered from its <see cref="IEventLoop"/>. This will result in having the
        /// <see cref="IChannelHandler.ChannelUnregistered"/> method called of the next <see cref="IChannelHandler"/>
        /// contained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <returns>The current <see cref="IChannelHandlerContext"/>.</returns>
        IChannelHandlerContext FireChannelUnregistered();

        IChannelHandlerContext FireChannelActive();

        IChannelHandlerContext FireChannelInactive();

        IChannelHandlerContext FireChannelRead(object message);

        IChannelHandlerContext FireChannelReadComplete();

        IChannelHandlerContext FireChannelWritabilityChanged();

        IChannelHandlerContext FireExceptionCaught(Exception ex);

        IChannelHandlerContext FireUserEventTriggered(object evt);

        IChannelHandlerContext Read();

        Task WriteAsync(object message);

        Task WriteAsync(object message, IPromise promise);

        IChannelHandlerContext Flush();

        /// <summary>
        ///  Return the assigned <see cref="IChannelPipeline"/>
        /// </summary>
        IChannelPipeline Pipeline { get; }

        Task WriteAndFlushAsync(object message);

        Task WriteAndFlushAsync(object message, IPromise promise);

        /// <summary>
        /// Request to bind to the given <see cref="EndPoint"/>.
        /// <para>
        /// This will result in having the <see cref="IChannelHandler.BindAsync"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the
        /// <see cref="IChannel"/>.
        /// </para>
        /// </summary>
        /// <param name="localAddress">The <see cref="EndPoint"/> to bind to.</param>
        /// <returns>An await-able task.</returns>
        Task BindAsync(EndPoint localAddress);

        /// <summary>
        /// Request to connect to the given <see cref="EndPoint"/>.
        /// <para>
        /// This will result in having the <see cref="IChannelHandler.ConnectAsync"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the
        /// <see cref="IChannel"/>.
        /// </para>
        /// </summary>
        /// <param name="remoteAddress">The <see cref="EndPoint"/> to connect to.</param>
        /// <returns>An await-able task.</returns>
        Task ConnectAsync(EndPoint remoteAddress);

        /// <summary>
        /// Request to connect to the given <see cref="EndPoint"/> while also binding to the localAddress.
        /// <para>
        /// This will result in having the <see cref="IChannelHandler.ConnectAsync"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the
        /// <see cref="IChannel"/>.
        /// </para>
        /// </summary>
        /// <param name="remoteAddress">The <see cref="EndPoint"/> to connect to.</param>
        /// <param name="localAddress">The <see cref="EndPoint"/> to bind to.</param>
        /// <returns>An await-able task.</returns>
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        /// Request to disconnect from the remote peer.
        /// <para>
        /// This will result in having the <see cref="IChannelHandler.Disconnect(IChannelHandlerContext, IPromise)"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the
        /// <see cref="IChannel"/>.
        /// </para>
        /// </summary>
        /// <returns>An await-able task.</returns>
        Task DisconnectAsync();

        Task DisconnectAsync(IPromise promise);

        Task CloseAsync();

        Task CloseAsync(IPromise promise);

        /// <summary>
        /// Request to deregister from the previous assigned <see cref="IEventExecutor"/>.
        /// <para>
        /// This will result in having the <see cref="IChannelHandler.Deregister(IChannelHandlerContext, IPromise)"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the
        /// <see cref="IChannel"/>.
        /// </para>
        /// </summary>
        /// <returns>An await-able task.</returns>
        Task DeregisterAsync();

        Task DeregisterAsync(IPromise promise);

        IPromise NewPromise();

        IPromise NewPromise(object state);

        IPromise VoidPromise();
    }
}