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
    using DotNetty.Common.Concurrency;

    /// <summary>
    /// Handles an I/O event or intercepts an I/O operation, and forwards it to its next handler in
    /// its <see cref="IChannelPipeline"/>.
    ///
    /// <h3>Sub-types</h3>
    /// <para>
    /// <see cref="IChannelHandler"/> itself does not provide many methods, but you usually have to implement one of its subtypes:
    /// <ul>
    /// <li>{@link ChannelInboundHandler} to handle inbound I/O events, and</li>
    /// <li>{@link ChannelOutboundHandler} to handle outbound I/O operations.</li>
    /// </ul>
    /// </para>
    /// <para>
    /// Alternatively, the following adapter classes are provided for your convenience:
    /// <ul>
    /// <li>{@link ChannelInboundHandlerAdapter} to handle inbound I/O events,</li>
    /// <li>{@link ChannelOutboundHandlerAdapter} to handle outbound I/O operations, and</li>
    /// <li><see cref="ChannelDuplexHandler"/> to handle both inbound and outbound events</li>
    /// </ul>
    /// </para>
    /// <para>
    /// For more information, please refer to the documentation of each subtype.
    /// </para>
    ///
    /// <h3>The context object</h3>
    /// <para>
    /// A <see cref="IChannelHandler"/> is provided with a <see cref="IChannelHandlerContext"/>
    /// object.  A <see cref="IChannelHandler"/> is supposed to interact with the
    /// <see cref="IChannelPipeline"/> it belongs to via a context object.  Using the
    /// context object, the <see cref="IChannelHandler"/> can pass events upstream or
    /// downstream, modify the pipeline dynamically, or store the information
    /// (using {@link AttributeKey}s) which is specific to the handler.
    ///
    /// <h3>State management</h3>
    ///
    /// A <see cref="IChannelHandler"/> often needs to store some stateful information.
    /// The simplest and recommended approach is to use member variables:
    /// </para>
    /// <code>
    /// public interface Message {
    ///     // your methods here
    /// }
    ///
    /// public class DataServerHandler extends {@link SimpleChannelInboundHandler}&lt;Message&gt; {
    ///
    ///     <b>private boolean loggedIn;</b>
    ///
    ///     {@code @Override}
    ///     public void channelRead0(<see cref="IChannelHandlerContext"/> ctx, Message message) {
    ///         if (message instanceof LoginMessage) {
    ///             authenticate((LoginMessage) message);
    ///             <b>loggedIn = true;</b>
    ///         } else (message instanceof GetDataMessage) {
    ///             if (<b>loggedIn</b>) {
    ///                 ctx.writeAndFlush(fetchSecret((GetDataMessage) message));
    ///             } else {
    ///                 fail();
    ///             }
    ///         }
    ///     }
    ///     ...
    /// }
    /// </code>
    /// Because the handler instance has a state variable which is dedicated to
    /// one connection, you have to create a new handler instance for each new
    /// channel to avoid a race condition where a unauthenticated client can get
    /// the confidential information:
    /// <code>
    /// // Create a new handler instance per channel.
    /// // See {@link ChannelInitializer#initChannel(Channel)}.
    /// public class DataServerInitializer extends {@link ChannelInitializer}&lt;{@link Channel}&gt; {
    ///     {@code @Override}
    ///     public void initChannel({@link Channel} channel) {
    ///         channel.pipeline().addLast("handler", <b>new DataServerHandler()</b>);
    ///     }
    /// }
    ///
    /// </code>
    ///
    /// <h4>Using {@link AttributeKey}s</h4>
    ///
    /// Although it's recommended to use member variables to store the state of a
    /// handler, for some reason you might not want to create many handler instances.
    /// In such a case, you can use {@link AttributeKey}s which is provided by
    /// <see cref="IChannelHandlerContext"/>:
    /// <code>
    /// public interface Message {
    ///     // your methods here
    /// }
    ///
    /// {@code @Sharable}
    /// public class DataServerHandler extends {@link SimpleChannelInboundHandler}&lt;Message&gt; {
    ///     private final {@link AttributeKey}&lt;{@link Boolean}&gt; auth =
    ///           {@link AttributeKey#valueOf(String) AttributeKey.valueOf("auth")};
    ///
    ///     {@code @Override}
    ///     public void channelRead(<see cref="IChannelHandlerContext"/> ctx, Message message) {
    ///         {@link Attribute}&lt;{@link Boolean}&gt; attr = ctx.attr(auth);
    ///         if (message instanceof LoginMessage) {
    ///             authenticate((LoginMessage) o);
    ///             <b>attr.set(true)</b>;
    ///         } else (message instanceof GetDataMessage) {
    ///             if (<b>Boolean.TRUE.equals(attr.get())</b>) {
    ///                 ctx.writeAndFlush(fetchSecret((GetDataMessage) o));
    ///             } else {
    ///                 fail();
    ///             }
    ///         }
    ///     }
    ///     ...
    /// }
    /// </code>
    /// Now that the state of the handler is attached to the <see cref="IChannelHandlerContext"/>, you can add the
    /// same handler instance to different pipelines:
    /// <code>
    /// public class DataServerInitializer extends {@link ChannelInitializer}&lt;{@link Channel}&gt; {
    ///
    ///     private static final DataServerHandler <b>SHARED</b> = new DataServerHandler();
    ///
    ///     {@code @Override}
    ///     public void initChannel({@link Channel} channel) {
    ///         channel.pipeline().addLast("handler", <b>SHARED</b>);
    ///     }
    /// }
    /// </code>
    ///
    ///
    /// <h3>Additional resources worth reading</h3>
    /// <para>
    /// Please refer to the <see cref="IChannelHandler"/>, and
    /// <see cref="IChannelPipeline"/> to find out more about inbound and outbound operations,
    /// what fundamental differences they have, how they flow in a  pipeline,  and how to handle
    /// the operation in your application.
    /// </para>
    /// </summary>
    public interface IChannelHandler
    {
        /// <summary>
        /// The <see cref="IChannel"/> of the <see cref="IChannelHandlerContext"/> was registered with its
        /// <see cref="IEventLoop"/>.
        /// </summary>
        void ChannelRegistered(IChannelHandlerContext context);

        /// <summary>
        /// The <see cref="IChannel"/> of the <see cref="IChannelHandlerContext"/> was unregistered from its
        /// <see cref="IEventLoop"/>.
        /// </summary>
        void ChannelUnregistered(IChannelHandlerContext context);

        void ChannelActive(IChannelHandlerContext context);

        void ChannelInactive(IChannelHandlerContext context);

        void ChannelRead(IChannelHandlerContext context, object message);

        void ChannelReadComplete(IChannelHandlerContext context);

        /// <summary>
        /// Gets called once the writable state of a <see cref="IChannel"/> changed. You can check the state with
        /// <see cref="IChannel.IsWritable"/>.
        /// </summary>
        void ChannelWritabilityChanged(IChannelHandlerContext context);

        /// <summary>
        /// Gets called after the <see cref="IChannelHandler"/> was added to the actual context and it's ready to handle events.
        /// </summary>
        /// <param name="context"></param>
        void HandlerAdded(IChannelHandlerContext context);

        /// <summary>
        /// Gets called after the <see cref="IChannelHandler"/> was removed from the actual context and it doesn't handle events
        /// anymore.
        /// </summary>
        /// <param name="context"></param>
        void HandlerRemoved(IChannelHandlerContext context);

        void Write(IChannelHandlerContext context, object message, IPromise promise);

        void Flush(IChannelHandlerContext context);

        /// <summary>
        /// Called once a bind operation is made.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IChannelHandlerContext"/> for which the bind operation is made.
        /// </param>
        /// <param name="localAddress">The <see cref="EndPoint"/> to which it should bind.</param>
        /// <returns>An await-able task.</returns>
        Task BindAsync(IChannelHandlerContext context, EndPoint localAddress);

        /// <summary>
        /// Called once a connect operation is made.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IChannelHandlerContext"/> for which the connect operation is made.
        /// </param>
        /// <param name="remoteAddress">The <see cref="EndPoint"/> to which it should connect.</param>
        /// <param name="localAddress">The <see cref="EndPoint"/> which is used as source on connect.</param>
        /// <returns>An await-able task.</returns>
        Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        /// Called once a disconnect operation is made.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IChannelHandlerContext"/> for which the disconnect operation is made.
        /// </param>
        /// <param name="promise"></param>
        void Disconnect(IChannelHandlerContext context, IPromise promise);

        void Close(IChannelHandlerContext context, IPromise promise);

        /// <summary>
        /// Gets called if a <see cref="Exception"/> was thrown.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void ExceptionCaught(IChannelHandlerContext context, Exception exception);

        void Deregister(IChannelHandlerContext context, IPromise promise);

        void Read(IChannelHandlerContext context);

        void UserEventTriggered(IChannelHandlerContext context, object evt);
    }
}