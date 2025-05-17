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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    public abstract partial class AbstractChannel<TChannel, TUnsafe> : DefaultAttributeMap, IChannel
        where TChannel : AbstractChannel<TChannel, TUnsafe>
        where TUnsafe : IChannelUnsafe, new()
    {
        protected static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance(typeof(TChannel));

        private static readonly ClosedChannelException Flush0ClosedChannelException = new ClosedChannelException();
        private static readonly ClosedChannelException EnsureOpenClosedChannelException = new ClosedChannelException();
        private static readonly ClosedChannelException CloseClosedChannelException = new ClosedChannelException();
        protected static readonly ClosedChannelException WriteClosedChannelException = new ClosedChannelException();
        private static readonly NotYetConnectedException Flush0NotYetConnectedException = new NotYetConnectedException();

        private readonly TUnsafe _channelUnsafe;

        private readonly DefaultChannelPipeline _pipeline;
        private readonly VoidChannelPromise _unsafeVoidPromise;
        private readonly IPromise _closeFuture;

        private EndPoint v_localAddress;
        private EndPoint v_remoteAddress;
        private IEventLoop v_eventLoop;
        private int v_registered;
        private int v_closeInitiated;

        /// <summary>Cache for the string representation of this channel</summary>
        private bool _strValActive;

        private string _strVal;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="parent">The parent of this channel. Pass <c>null</c> if there's no parent.</param>
        protected AbstractChannel(IChannel parent)
        {
            Parent = parent;
            Id = NewId();
            _channelUnsafe = NewUnsafe();
            _pipeline = NewChannelPipeline();
            _unsafeVoidPromise = new VoidChannelPromise(this, false);
            _closeFuture = NewPromise();
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="parent">The parent of this channel. Pass <c>null</c> if there's no parent.</param>
        /// <param name="id">An <see cref="IChannelId"/> for the new channel.</param>
        protected AbstractChannel(IChannel parent, IChannelId id)
        {
            Parent = parent;
            Id = id;
            _channelUnsafe = NewUnsafe();
            _pipeline = NewChannelPipeline();
            _unsafeVoidPromise = new VoidChannelPromise(this, false);
            _closeFuture = NewPromise();
        }

        public IChannelId Id { get; }

        public bool IsWritable
        {
            get
            {
                ChannelOutboundBuffer buf = _channelUnsafe.OutboundBuffer;
                return buf is object && buf.IsWritable;
            }
        }

        public long BytesBeforeUnwritable
        {
            get
            {
                ChannelOutboundBuffer buf = _channelUnsafe.OutboundBuffer;
                // isWritable() is currently assuming if there is no outboundBuffer then the channel is not writable.
                // We should be consistent with that here.
                return buf is object ? buf.BytesBeforeUnwritable : 0;
            }
        }

        public long BytesBeforeWritable
        {
            get
            {
                ChannelOutboundBuffer buf = _channelUnsafe.OutboundBuffer;
                // isWritable() is currently assuming if there is no outboundBuffer then the channel is not writable.
                // We should be consistent with that here.
                return buf is object ? buf.BytesBeforeWritable : long.MaxValue;
            }
        }

        public IChannel Parent { get; }

        public IChannelPipeline Pipeline => _pipeline;

        public abstract IChannelConfiguration Configuration { get; }

        public IByteBufferAllocator Allocator => Configuration.Allocator;

        public IEventLoop EventLoop
        {
            get
            {
                IEventLoop eventLoop = Volatile.Read(ref v_eventLoop);
                if (eventLoop is null)
                {
                    ThrowHelper.ThrowInvalidOperationException_ChannelNotReg();
                }
                return eventLoop;
            }
        }

        [Obsolete("Please use IsOpen instead.")]
        public bool Open => IsOpen;

        public abstract bool IsOpen { get; }

        [Obsolete("Please use IsActive instead.")]
        public bool Active => IsActive;

        public abstract bool IsActive { get; }

        public abstract ChannelMetadata Metadata { get; }

        public EndPoint LocalAddress
        {
            get
            {
                EndPoint address = Volatile.Read(ref v_localAddress);
                return address ?? CacheLocalAddress();
            }
        }

        public EndPoint RemoteAddress
        {
            get
            {
                EndPoint address = Volatile.Read(ref v_remoteAddress);
                return address ?? CacheRemoteAddress();
            }
        }

        protected abstract EndPoint LocalAddressInternal { get; }

        protected void InvalidateLocalAddress() => Interlocked.Exchange(ref v_localAddress, null);

        protected EndPoint CacheLocalAddress()
        {
            try
            {
                var localAddr = LocalAddressInternal;
                _ = Interlocked.Exchange(ref v_localAddress, localAddr);
                return localAddr;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }

        protected abstract EndPoint RemoteAddressInternal { get; }

        /// <summary>
        /// Resets the stored <see cref="RemoteAddress"/>.
        /// </summary>
        protected void InvalidateRemoteAddress() => Interlocked.Exchange(ref v_remoteAddress, null);

        protected EndPoint CacheRemoteAddress()
        {
            try
            {
                var remoteAddr = RemoteAddressInternal;
                _ = Interlocked.Exchange(ref v_remoteAddress, remoteAddr);
                return remoteAddr;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }

        [Obsolete("Please use IsRegistered instead.")]
        public bool Registered => IsRegistered;

        public bool IsRegistered => SharedConstants.False < (uint)Volatile.Read(ref v_registered);

        /// <summary>
        /// Returns a new <see cref="DefaultChannelId"/> instance. Subclasses may override this method to assign custom
        /// <see cref="IChannelId"/>s to <see cref="IChannel"/>s that use the <see cref="AbstractChannel{TChannel, TUnsafe}"/> constructor.
        /// </summary>
        /// <returns>A new <see cref="DefaultChannelId"/> instance.</returns>
        protected virtual IChannelId NewId() => DefaultChannelId.NewInstance();

        /// <summary>Returns a new pipeline instance.</summary>
        protected virtual DefaultChannelPipeline NewChannelPipeline() => new DefaultChannelPipeline(this);

        public virtual Task BindAsync(EndPoint localAddress) => _pipeline.BindAsync(localAddress);

        public virtual Task ConnectAsync(EndPoint remoteAddress) => _pipeline.ConnectAsync(remoteAddress);

        public virtual Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress) => _pipeline.ConnectAsync(remoteAddress, localAddress);

        public virtual Task DisconnectAsync() => _pipeline.DisconnectAsync();

        public virtual Task DisconnectAsync(IPromise promise) => _pipeline.DisconnectAsync(promise);

        public virtual Task CloseAsync() => _pipeline.CloseAsync();

        public virtual Task CloseAsync(IPromise promise) => _pipeline.CloseAsync(promise);

        public Task DeregisterAsync() => _pipeline.DeregisterAsync();

        public Task DeregisterAsync(IPromise promise) => _pipeline.DeregisterAsync(promise);

        public IChannel Flush()
        {
            _ = _pipeline.Flush();
            return this;
        }

        public IChannel Read()
        {
            _ = _pipeline.Read();
            return this;
        }

        public Task WriteAsync(object msg) => _pipeline.WriteAsync(msg);

        public Task WriteAsync(object message, IPromise promise) => _pipeline.WriteAsync(message, promise);

        public Task WriteAndFlushAsync(object message) => _pipeline.WriteAndFlushAsync(message);

        public Task WriteAndFlushAsync(object message, IPromise promise) => _pipeline.WriteAndFlushAsync(message, promise);

        public IPromise NewPromise() => new DefaultPromise();

        public IPromise NewPromise(object state) => new DefaultPromise(state);

        public IPromise VoidPromise() => _pipeline.VoidPromise();

        public Task CloseCompletion => _closeFuture.Task;

        IChannelUnsafe IChannel.Unsafe => _channelUnsafe;
        public TUnsafe Unsafe => _channelUnsafe;

        /// <summary>
        /// Create a new <see cref="AbstractUnsafe" /> instance which will be used for the life-time of the
        /// <see cref="IChannel" />
        /// </summary>
        protected virtual TUnsafe NewUnsafe()
        {
            var @unsafe = new TUnsafe();
            @unsafe.Initialize((TChannel)this);
            return @unsafe;
        }

        /// <summary>
        /// Returns the ID of this channel.
        /// </summary>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Returns <c>true</c> if and only if the specified object is identical
        /// with this channel (i.e. <c>this == o</c>).
        /// </summary>
        public override bool Equals(object o) => this == o;

        public bool Equals(IChannel other) => ReferenceEquals(this, other);

        public int CompareTo(IChannel o) => ReferenceEquals(this, o) ? 0 : Id.CompareTo(o.Id);

        /// <summary>
        /// Returns the string representation of this channel. The returned string contains a hex dump of the
        /// <see cref="IChannelId"/>, the <see cref="LocalAddress"/>, and the <see cref="RemoteAddress"/> of this
        /// channel for easier identification.
        /// </summary>
        public override string ToString()
        {
            bool active = IsActive;
            if (_strValActive == active && _strVal is object)
            {
                return _strVal;
            }

            EndPoint remoteAddr = RemoteAddress;
            EndPoint localAddr = LocalAddress;
            if (remoteAddr is object)
            {
                var buf = StringBuilderCache.Acquire(96)
                    .Append("[id: 0x")
                    .Append(Id.AsShortText())
                    .Append(", L:")
                    .Append(localAddr)
                    .Append(active ? " - " : " ! ")
                    .Append("R:")
                    .Append(remoteAddr)
                    .Append(']');
                _strVal = StringBuilderCache.GetStringAndRelease(buf);
            }
            else if (localAddr is object)
            {
                var buf = StringBuilderCache.Acquire(64)
                    .Append("[id: 0x")
                    .Append(Id.AsShortText())
                    .Append(", L:")
                    .Append(localAddr)
                    .Append(']');
                _strVal = StringBuilderCache.GetStringAndRelease(buf);
            }
            else
            {
                var buf = StringBuilderCache.Acquire(16)
                    .Append("[id: 0x")
                    .Append(Id.AsShortText())
                    .Append(']');
                _strVal = StringBuilderCache.GetStringAndRelease(buf);
            }

            _strValActive = active;
            return _strVal;
        }


        /// <summary>
        /// Checks whether a given <see cref="IEventLoop"/> is compatible with the <see cref="AbstractChannel{TChannel, TUnsafe}"/>.
        /// </summary>
        /// <param name="eventLoop">The <see cref="IEventLoop"/> to check compatibility.</param>
        /// <returns>
        /// <c>true</c> if the given <see cref="IEventLoop"/> is compatible with this <see cref="AbstractChannel{TChannel, TUnsafe}"/>
        /// instance, otherwise <c>false</c>.
        /// </returns>
        protected abstract bool IsCompatible(IEventLoop eventLoop);

        /// <summary>
        /// Is called after the <see cref="IChannel"/> is registered with its <see cref="IEventLoop"/> as part of the
        /// register process. Sub-classes may override this method.
        /// </summary>
        protected virtual void DoRegister()
        {
            // NOOP
        }

        /// <summary>
        /// Binds the <see cref="IChannel"/> to the <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="localAddress">The <see cref="EndPoint"/> to bind.</param>
        protected abstract void DoBind(EndPoint localAddress);

        /// <summary>
        /// Disconnects this <see cref="IChannel"/> from its remote peer.
        /// </summary>
        protected abstract void DoDisconnect();

        /// <summary>
        /// Closes the <see cref="IChannel"/>.
        /// </summary>
        protected abstract void DoClose();

        /// <summary>
        /// Called when conditions justify shutting down the output portion of the channel. This may happen if a write
        /// operation throws an exception.
        /// </summary>
        protected virtual void DoShutdownOutput() => DoClose();

        /// <summary>
        /// Deregisters the <see cref="IChannel"/> from its <see cref="IEventLoop"/>. Sub-classes may override this
        /// method.
        /// </summary>
        protected virtual void DoDeregister()
        {
            // NOOP
        }

        /// <summary>
        /// ScheduleAsync a read operation.
        /// </summary>
        protected abstract void DoBeginRead();

        /// <summary>
        /// Flush the content of the given buffer to the remote peer.
        /// </summary>
        protected abstract void DoWrite(ChannelOutboundBuffer input);

        /// <summary>
        /// Invoked when a new message is added to a <see cref="ChannelOutboundBuffer"/> of this
        /// <see cref="AbstractChannel{TChannel, TUnsafe}"/>, so that the <see cref="IChannel"/> implementation converts the message to
        /// another. (e.g. heap buffer -> direct buffer).
        /// </summary>
        /// <param name="msg">The message to be filtered.</param>
        /// <returns>The filtered message.</returns>
        protected virtual object FilterOutboundMessage(object msg) => msg;
    }
}