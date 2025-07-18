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
    using DotNetty.Transport.Channels.Sockets;

    /// <summary>
    /// A nexus to a network socket or a component which is capable of I/O
    /// operations such as read, write, connect, and bind.
    /// <para>
    /// A channel provides a user:
    /// <ul>
    /// <li>the current state of the channel (e.g. is it open? is it connected?),</li>
    /// <li>the <see cref="IChannelConfiguration"/> configuration parameters of the channel (e.g. receive buffer size),</li>
    /// <li>the I/O operations that the channel supports (e.g. read, write, connect, and bind), and</li>
    /// <li>the <see cref="IChannelPipeline"/> which handles all I/O events and requests
    ///     associated with the channel.</li>
    /// </ul>
    ///
    /// <h3>All I/O operations are asynchronous.</h3>
    /// </para>
    /// All I/O operations in Netty are asynchronous.  It means any I/O calls will
    /// return immediately with no guarantee that the requested I/O operation has
    /// been completed at the end of the call.  Instead, you will be returned with
    /// a <see cref="Task"/> instance which will notify you when the requested I/O
    /// operation has succeeded, failed, or canceled.
    ///
    /// <h3>Channels are hierarchical</h3>
    /// <para>
    /// A <see cref="IChannel"/> can have a <see cref="Parent"/> depending on
    /// how it was created.  For instance, a <see cref="ISocketChannel"/>, that was accepted
    /// by <see cref="IServerSocketChannel"/>, will return the <see cref="IServerSocketChannel"/>
    /// as its parent on <see cref="Parent"/>.
    /// </para>
    /// The semantics of the hierarchical structure depends on the transport
    /// implementation where the <see cref="IChannel"/> belongs to.  For example, you could
    /// write a new <see cref="IChannel"/> implementation that creates the sub-channels that
    /// share one socket connection, as <a href="http://beepcore.org/">BEEP</a> and
    /// <a href="http://en.wikipedia.org/wiki/Secure_Shell">SSH</a> do.
    ///
    /// <h3>Downcast to access transport-specific operations</h3>
    /// <para>
    /// Some transports exposes additional operations that is specific to the
    /// transport.  Down-cast the <see cref="IChannel"/> to sub-type to invoke such
    /// operations.  For example, with the old I/O datagram transport, multicast
    /// join / leave operations are provided by <see cref="IDatagramChannel"/>.
    ///
    /// <h3>Release resources</h3>
    /// </para>
    /// It is important to call <see cref="CloseAsync()"/> to release all
    /// resources once you are done with the <see cref="IChannel"/>. This ensures all resources are
    /// released in a proper way, i.e. filehandles.
    /// </summary>
    public interface IChannel : IAttributeMap, IComparable<IChannel>, IEquatable<IChannel>
    {
        /// <summary>
        /// Gets the globally unique identifier of this <see cref="IChannel"/>.
        /// </summary>
        IChannelId Id { get; }

        /// <summary>
        /// Return the assigned <see cref="IByteBufferAllocator"/> which will be used to allocate <see cref="IByteBuffer"/>s.
        /// </summary>
        IByteBufferAllocator Allocator { get; }

        /// <summary>
        /// Gets the <see cref="IEventLoop"/> this <see cref="IChannel"/> was registered to.
        /// </summary>
        IEventLoop EventLoop { get; }

        /// <summary>
        /// Gets the parent of this channel.
        /// Returns <c>null</c> if this channel does not have a parent channel.
        /// </summary>
        IChannel Parent { get; }

        [Obsolete("Please use IsOpen instead.")]
        bool Open { get; }

        [Obsolete("Please use IsActive instead.")]
        bool Active { get; }

        [Obsolete("Please use IsRegistered instead.")]
        bool Registered { get; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="IChannel"/> is open and may get active later.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="IChannel"/> is active and so connected.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="IChannel"/> is registered with an <see cref="IEventLoop"/>.
        /// </summary>
        bool IsRegistered { get; }

        /// <summary>
        /// Returns the <see cref="ChannelMetadata"/> of the <see cref="IChannel"/> which describe the nature of the
        /// <see cref="IChannel"/>.
        /// </summary>
        ChannelMetadata Metadata { get; }

        /// <summary>
        /// Returns the local address where this channel is bound to.
        /// </summary>
        EndPoint LocalAddress { get; }

        /// <summary>
        /// Returns the remote address where this channel is connected to.
        /// </summary>
        EndPoint RemoteAddress { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if the I/O thread will perform the
        /// requested write operation immediately.Any write requests made when
        /// this method returns <c>false</c> are queued until the I/O thread is
        /// ready to process the queued write requests.
        /// </summary>
        bool IsWritable { get; }

        /// <summary>
        /// Get how many bytes can be written until <see cref="IsWritable"/> returns <c>false</c>.
        /// This quantity will always be non-negative. If <see cref="IsWritable"/> is <c>false</c> then 0.
        /// </summary>
        long BytesBeforeUnwritable { get; }

        /// <summary>
        /// Get how many bytes must be drained from underlying buffers until <see cref="IsWritable"/> returns <c>true</c>.
        /// This quantity will always be non-negative. If <see cref="IsWritable"/> is <c>true</c> then 0.
        /// </summary>
        long BytesBeforeWritable { get; }

        /// <summary>
        /// Returns an <em>internal-use-only</em> object that provides unsafe operations.
        /// </summary>
        IChannelUnsafe Unsafe { get; }

        /// <summary>
        /// Returns the assigned <see cref="IChannelPipeline"/>.
        /// </summary>
        IChannelPipeline Pipeline { get; }

        /// <summary>
        /// Returns the configuration of this channel.
        /// </summary>
        IChannelConfiguration Configuration { get; }

        Task CloseCompletion { get; }

        Task DeregisterAsync();

        Task DeregisterAsync(IPromise promise);

        /// <summary>
        /// Bind the <see cref="EndPoint"/> to the <see cref="IChannel"/> and notify it once its done.
        /// </summary>
        /// <param name="localAddress"></param>
        /// <returns></returns>
        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        /// <summary>
        /// Connect the <see cref="IChannel"/> with the given remote <see cref="EndPoint"/>.
        /// If a specific local <see cref="EndPoint"/> should be used it need to be given as argument. Otherwise just
        /// pass <c>null</c> to it.
        /// 
        /// The <see cref="Task"/> will get notified once the connect operation was complete.
        /// </summary>
        /// <param name="remoteAddress"></param>
        /// <param name="localAddress"></param>
        /// <returns></returns>
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        /// Disconnect the <see cref="IChannel"/> and notify the <see cref="Task"/> once the operation was complete.
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        Task DisconnectAsync(IPromise promise);

        Task CloseAsync();

        Task CloseAsync(IPromise promise);

        // todo: make these available through separate interface to hide them from public API on channel

        IChannel Read();

        Task WriteAsync(object message);

        Task WriteAsync(object message, IPromise promise);

        IChannel Flush();

        Task WriteAndFlushAsync(object message);

        Task WriteAndFlushAsync(object message, IPromise promise);

        IPromise NewPromise();

        IPromise NewPromise(object state);

        IPromise VoidPromise();
    }
}