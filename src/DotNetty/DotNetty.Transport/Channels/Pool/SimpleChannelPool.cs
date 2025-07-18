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

namespace DotNetty.Transport.Channels.Pool
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Bootstrapping;

    /// <summary>
    /// Simple <see cref="IChannelPool"/> implementation which will create new <see cref="IChannel"/>s if someone tries to acquire
    /// a <see cref="IChannel"/> but none is in the pool atm. No limit on the maximal concurrent <see cref="IChannel"/>s is enforced.
    /// This implementation uses LIFO order for <see cref="IChannel"/>s in the <see cref="IChannelPool"/>.
    /// </summary>
    public class SimpleChannelPool : IChannelPool
    {
        internal static readonly InvalidOperationException FullException;
        private static readonly AttributeKey<SimpleChannelPool> PoolKey;

        static SimpleChannelPool()
        {
            FullException = new InvalidOperationException("ChannelPool full");
            PoolKey = AttributeKey<SimpleChannelPool>.NewInstance("io.netty.channel.pool.SimpleChannelPool");
        }

        private readonly IQueue<IChannel> _store;

        /// <summary>
        /// Creates a new <see cref="SimpleChannelPool"/> instance using the <see cref="ChannelActiveHealthChecker"/>.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrapping.Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.</param>
        public SimpleChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler)
            : this(bootstrap, handler, ChannelActiveHealthChecker.Instance)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SimpleChannelPool"/> instance.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrapping.Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="healthChecker">
        /// The <see cref="IChannelHealthChecker"/> that will be used to check if a <see cref="IChannel"/> is still
        /// healthy when obtained from the <see cref="IChannelPool"/>.
        /// </param>
        public SimpleChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, IChannelHealthChecker healthChecker)
            : this(bootstrap, handler, healthChecker, true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SimpleChannelPool"/> instance.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrapping.Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="healthChecker">
        /// The <see cref="IChannelHealthChecker"/> that will be used to check if a <see cref="IChannel"/> is still
        /// healthy when obtained from the <see cref="IChannelPool"/>.
        /// </param>
        /// <param name="releaseHealthCheck">
        /// If <c>true</c>, will check channel health before offering back. Otherwise, channel health is only checked
        /// at acquisition time.
        /// </param>
        public SimpleChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, IChannelHealthChecker healthChecker, bool releaseHealthCheck)
            : this(bootstrap, handler, healthChecker, releaseHealthCheck, true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SimpleChannelPool"/> instance.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrapping.Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="healthChecker">
        /// The <see cref="IChannelHealthChecker"/> that will be used to check if a <see cref="IChannel"/> is still
        /// healthy when obtained from the <see cref="IChannelPool"/>.
        /// </param>
        /// <param name="releaseHealthCheck">
        /// If <c>true</c>, will check channel health before offering back. Otherwise, channel health is only checked
        /// at acquisition time.
        /// </param>
        /// <param name="lastRecentUsed">
        /// If <c>true</c>, <see cref="IChannel"/> selection will be LIFO. If <c>false</c>, it will be FIFO.
        /// </param>
        public SimpleChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, IChannelHealthChecker healthChecker, bool releaseHealthCheck, bool lastRecentUsed)
        {
            if (handler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handler); }
            if (healthChecker is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.healthChecker); }
            if (bootstrap is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bootstrap); }

            Handler = handler;
            HealthChecker = healthChecker;
            ReleaseHealthCheck = releaseHealthCheck;

            // Clone the original Bootstrap as we want to set our own handler
            Bootstrap = bootstrap.Clone();
            _ = Bootstrap.Handler(new ActionChannelInitializer<IChannel>(OnChannelInitializing));
            _store =
                lastRecentUsed
                    ? (IQueue<IChannel>)new CompatibleConcurrentStack<IChannel>()
                    : new CompatibleConcurrentQueue<IChannel>();
        }

        void OnChannelInitializing(IChannel channel)
        {
            Debug.Assert(channel.EventLoop.InEventLoop);
            Handler.ChannelCreated(channel);
        }

        /// <summary>
        /// Returns the <see cref="Bootstrapping.Bootstrap"/> this pool will use to open new connections. 
        /// </summary>
        internal Bootstrap Bootstrap { get; }

        /// <summary>
        /// Returns the <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </summary>
        internal IChannelPoolHandler Handler { get; }

        /// <summary>
        /// Returns the <see cref="IChannelHealthChecker"/> that will be used to check if an <see cref="IChannel"/> is healthy.
        /// </summary>
        internal IChannelHealthChecker HealthChecker { get; }

        /// <summary>
        /// Indicates whether this pool will check the health of channels before offering them back into the pool.
        /// Returns <c>true</c> if this pool will check the health of channels before offering them back into the pool, or
        /// <c>false</c> if channel health is only checked at acquisition time.
        /// </summary>
        internal bool ReleaseHealthCheck { get; }

        public virtual ValueTask<IChannel> AcquireAsync()
        {
            if (!TryPollChannel(out IChannel channel))
            {
                Bootstrap bs = Bootstrap.Clone();
                _ = bs.Attribute(PoolKey, this);
                return new ValueTask<IChannel>(ConnectChannel(bs));
            }

            IEventLoop eventLoop = channel.EventLoop;
            if (eventLoop.InEventLoop)
            {
                return DoHealthCheck(channel);
            }
            else
            {
                var completionSource = new ManualResetValueTaskSource<IChannel>();
                eventLoop.Execute(DoHealthCheck, channel, completionSource);
                return completionSource.AwaitValue(CancellationToken.None);
            }
        }

        async void DoHealthCheck(object channel, object state)
        {
            var promise = state as ManualResetValueTaskSource<IChannel>;
            try
            {
                var result = await DoHealthCheck((IChannel)channel);
                promise.SetResult(result);
            }
            catch (Exception ex)
            {
                promise.SetException(ex);
            }
        }

        async ValueTask<IChannel> DoHealthCheck(IChannel channel)
        {
            Debug.Assert(channel.EventLoop.InEventLoop);
            try
            {
                if (await HealthChecker.IsHealthyAsync(channel))
                {
                    try
                    {
                        channel.GetAttribute(PoolKey).Set(this);
                        Handler.ChannelAcquired(channel);
                        return channel;
                    }
                    catch (Exception)
                    {
                        CloseChannel(channel);
                        throw;
                    }
                }
                else
                {
                    CloseChannel(channel);
                    return await AcquireAsync();
                }
            }
            catch
            {
                CloseChannel(channel);
                return await AcquireAsync();
            }
        }

        /// <summary>
        /// Bootstrap a new <see cref="IChannel"/>. The default implementation uses
        /// <see cref="Bootstrapping.Bootstrap.ConnectAsync()"/>, sub-classes may override this.
        /// </summary>
        /// <param name="bs">
        /// The <see cref="Bootstrapping.Bootstrap"/> instance to use to bootstrap a new <see cref="IChannel"/>.
        /// The <see cref="Bootstrapping.Bootstrap"/> passed here is cloned via
        /// <see cref="Bootstrapping.Bootstrap.Clone()"/>, so it is safe to modify.
        /// </param>
        /// <returns>The newly connected <see cref="IChannel"/>.</returns>
        protected virtual Task<IChannel> ConnectChannel(Bootstrap bs) => bs.ConnectAsync();

        public virtual async Task<bool> ReleaseAsync(IChannel channel)
        {
            if (channel is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.channel); }
            try
            {
                IEventLoop loop = channel.EventLoop;
                if (loop.InEventLoop)
                {
                    return await DoReleaseChannel(channel);
                }
                else
                {
                    var promise = new ManualResetValueTaskSource<bool>();
                    loop.Execute(DoReleaseChannel, channel, promise);
                    return await promise.AwaitValue(CancellationToken.None);
                }
            }
            catch (Exception)
            {
                CloseChannel(channel);
                throw;
            }
        }

        async void DoReleaseChannel(object channel, object state)
        {
            var promise = state as ManualResetValueTaskSource<bool>;
            try
            {
                var result = await DoReleaseChannel((IChannel)channel);
                _ = promise.SetResult(result);
            }
            catch (Exception ex)
            {
                promise.SetException(ex);
            }
        }

        async ValueTask<bool> DoReleaseChannel(IChannel channel)
        {
            Debug.Assert(channel.EventLoop.InEventLoop);

            // Remove the POOL_KEY attribute from the Channel and check if it was acquired from this pool, if not fail.
            if (channel.GetAttribute(PoolKey).GetAndSet(null) != this)
            {
                CloseChannel(channel);
                // Better include a stacktrace here as this is an user error.
                return ThrowHelper.FromArgumentException_ChannelWasNotAcquiredFromPool(channel);
            }
            else
            {
                try
                {
                    if (ReleaseHealthCheck)
                    {
                        return await DoHealthCheckOnRelease(channel);
                    }
                    else
                    {
                        ReleaseAndOffer(channel);
                        return true;
                    }
                }
                catch
                {
                    CloseChannel(channel);
                    throw;
                }
            }
        }

        /// <summary>
        /// Releases the channel back to the pool only if the channel is healthy.
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> to put back to the pool.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="IChannel"/> was healthy, released, and offered back to the pool.
        /// <c>false</c> if the <see cref="IChannel"/> was NOT healthy and was simply released.
        /// </returns>
        async ValueTask<bool> DoHealthCheckOnRelease(IChannel channel)
        {
            if (await HealthChecker.IsHealthyAsync(channel))
            {
                //channel turns out to be healthy, offering and releasing it.
                ReleaseAndOffer(channel);
                return true;
            }
            else
            {
                //channel not healthy, just releasing it.
                Handler.ChannelReleased(channel);
                return false;
            }
        }

        void ReleaseAndOffer(IChannel channel)
        {
            if (TryOfferChannel(channel))
            {
                Handler.ChannelReleased(channel);
            }
            else
            {
                CloseChannel(channel);
                ThrowHelper.ThrowInvalidOperationException_ChannelPoolFull();
            }
        }

        private void CloseChannel(IChannel channel)
        {
            _ = channel.GetAttribute(PoolKey).GetAndSet(null);
            _ = channel.CloseAsync();
        }

        /// <summary>
        /// Polls an <see cref="IChannel"/> out of the internal storage to reuse it.
        /// </summary>
        /// <remarks>
        /// Sub-classes may override <see cref="TryPollChannel"/> and <see cref="TryOfferChannel"/>.
        /// Be aware that implementations of these methods needs to be thread-safe!
        /// </remarks>
        /// <param name="channel">
        /// An output parameter that will contain the <see cref="IChannel"/> obtained from the pool.
        /// </param>
        /// <returns>
        /// <c>true</c> if an <see cref="IChannel"/> was retrieved from the pool, otherwise <c>false</c>.
        /// </returns>
        protected virtual bool TryPollChannel(out IChannel channel) => _store.TryDequeue(out channel);

        /// <summary>
        /// Offers a <see cref="IChannel"/> back to the internal storage. This will return 
        /// </summary>
        /// <remarks>
        /// Sub-classes may override <see cref="TryPollChannel"/> and <see cref="TryOfferChannel"/>.
        /// Be aware that implementations of these methods needs to be thread-safe!
        /// </remarks>
        /// <param name="channel"></param>
        /// <returns><c>true</c> if the <see cref="IChannel"/> could be added, otherwise <c>false</c>.</returns>
        protected virtual bool TryOfferChannel(IChannel channel) => _store.TryEnqueue(channel);

        public virtual void Close()
        {
            while (TryPollChannel(out IChannel channel))
            {
                // Just ignore any errors that are reported back from CloseAsync().
                try
                {
                    channel.CloseAsync().GetAwaiter().GetResult();
                }
                catch { }
            }
        }

        /// <summary>
        /// Closes the pool in an async manner.
        /// </summary>
        /// <returns><see cref="Task"/> which represents completion of the close task</returns>
        public virtual Task CloseAsync()
        {
            return Task.Run(() => Close());
        }

        sealed class CompatibleConcurrentStack<T> : ConcurrentStack<T>, IQueue<T>
        {
            public bool TryEnqueue(T item)
            {
                Push(item);
                return true;
            }

            public bool TryDequeue(out T item) => TryPop(out item);

            public bool NonEmpty => !IsEmpty;
        }
    }
}
