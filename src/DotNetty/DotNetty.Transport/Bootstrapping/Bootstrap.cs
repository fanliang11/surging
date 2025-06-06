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

namespace DotNetty.Transport.Bootstrapping
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// A <see cref="Bootstrap"/> that makes it easy to bootstrap an <see cref="IChannel"/> to use for clients.
    /// 
    /// The <see cref="AbstractBootstrap{TBootstrap,TChannel}.BindAsync(EndPoint)"/> methods are useful
    /// in combination with connectionless transports such as datagram (UDP). For regular TCP connections,
    /// please use the provided <see cref="ConnectAsync(EndPoint,EndPoint)"/> methods.
    /// </summary>
    public class Bootstrap : AbstractBootstrap<Bootstrap, IChannel>
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Bootstrap>();

        private static readonly INameResolver DefaultResolver = new DefaultNameResolver();

        private INameResolver v_resolver = DefaultResolver;
        private INameResolver InternalResolver
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Volatile.Read(ref v_resolver);
            set => Interlocked.Exchange(ref v_resolver, value);
        }

        private EndPoint v_remoteAddress;
        private EndPoint InternalRemoteAddress
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Volatile.Read(ref v_remoteAddress);
            set => Interlocked.Exchange(ref v_remoteAddress, value);
        }

        public Bootstrap()
        {
        }

        Bootstrap(Bootstrap bootstrap)
            : base(bootstrap)
        {
            InternalResolver = bootstrap.InternalResolver;
            InternalRemoteAddress = bootstrap.InternalRemoteAddress;
        }

        /// <summary>
        /// Sets the <see cref="INameResolver"/> which will resolve the address of the unresolved named address.
        /// </summary>
        /// <param name="resolver">The <see cref="INameResolver"/> which will resolve the address of the unresolved named address.</param>
        /// <returns>The <see cref="Bootstrap"/> instance.</returns>
        public Bootstrap Resolver(INameResolver resolver)
        {
            if (resolver is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.resolver); }
            InternalResolver = resolver;
            return this;
        }

        /// <summary>
        /// Assigns the remote <see cref="EndPoint"/> to connect to once the <see cref="ConnectAsync()"/> method is called.
        /// </summary>
        /// <param name="remoteAddress">The remote <see cref="EndPoint"/> to connect to.</param>
        /// <returns>The <see cref="Bootstrap"/> instance.</returns>
        public Bootstrap RemoteAddress(EndPoint remoteAddress)
        {
            InternalRemoteAddress = remoteAddress;
            return this;
        }

        /// <summary>
        /// Assigns the remote <see cref="EndPoint"/> to connect to once the <see cref="ConnectAsync()"/> method is called.
        /// </summary>
        /// <param name="inetHost">The hostname of the endpoint to connect to.</param>
        /// <param name="inetPort">The port at the remote host to connect to.</param>
        /// <returns>The <see cref="Bootstrap"/> instance.</returns>
        public Bootstrap RemoteAddress(string inetHost, int inetPort)
        {
            InternalRemoteAddress = new DnsEndPoint(inetHost, inetPort);
            return this;
        }

        /// <summary>
        /// Assigns the remote <see cref="EndPoint"/> to connect to once the <see cref="ConnectAsync()"/> method is called.
        /// </summary>
        /// <param name="inetHost">The <see cref="IPAddress"/> of the endpoint to connect to.</param>
        /// <param name="inetPort">The port at the remote host to connect to.</param>
        /// <returns>The <see cref="Bootstrap"/> instance.</returns>
        public Bootstrap RemoteAddress(IPAddress inetHost, int inetPort)
        {
            InternalRemoteAddress = new IPEndPoint(inetHost, inetPort);
            return this;
        }

        /// <summary>
        /// Connects an <see cref="IChannel"/> to the remote peer.
        /// </summary>
        /// <returns>The <see cref="IChannel"/>.</returns>
        public async Task<IChannel> ConnectAsync()
        {
            Validate();
            EndPoint remoteAddress = InternalRemoteAddress;
            if (remoteAddress is null)
            {
                ThrowHelper.ThrowInvalidOperationException_RemoteAddrNotSet();
            }

            return await DoResolveAndConnectAsync(remoteAddress, LocalAddress());
        }

        /// <summary>
        /// Connects an <see cref="IChannel"/> to the remote peer.
        /// </summary>
        /// <param name="inetHost">The hostname of the endpoint to connect to.</param>
        /// <param name="inetPort">The port at the remote host to connect to.</param>
        /// <returns>The <see cref="IChannel"/>.</returns>
        public Task<IChannel> ConnectAsync(string inetHost, int inetPort) => ConnectAsync(new DnsEndPoint(inetHost, inetPort));

        /// <summary>
        /// Connects an <see cref="IChannel"/> to the remote peer.
        /// </summary>
        /// <param name="inetHost">The <see cref="IPAddress"/> of the endpoint to connect to.</param>
        /// <param name="inetPort">The port at the remote host to connect to.</param>
        /// <returns>The <see cref="IChannel"/>.</returns>
        public Task<IChannel> ConnectAsync(IPAddress inetHost, int inetPort) => ConnectAsync(new IPEndPoint(inetHost, inetPort));

        /// <summary>
        /// Connects an <see cref="IChannel"/> to the remote peer.
        /// </summary>
        /// <param name="remoteAddress">The remote <see cref="EndPoint"/> to connect to.</param>
        /// <returns>The <see cref="IChannel"/>.</returns>
        public async Task<IChannel> ConnectAsync(EndPoint remoteAddress)
        {
            if (remoteAddress is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.remoteAddress); }

            Validate();
            return await DoResolveAndConnectAsync(remoteAddress, LocalAddress());
        }

        /// <summary>
        /// Connects an <see cref="IChannel"/> to the remote peer.
        /// </summary>
        /// <param name="remoteAddress">The remote <see cref="EndPoint"/> to connect to.</param>
        /// <param name="localAddress">The local <see cref="EndPoint"/> to connect to.</param>
        /// <returns>The <see cref="IChannel"/>.</returns>
        public async Task<IChannel> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            if (remoteAddress is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.remoteAddress); }

            Validate();
            return await DoResolveAndConnectAsync(remoteAddress, localAddress);
        }

        /// <summary>
        /// Performs DNS resolution for the remote endpoint and connects to it.
        /// </summary>
        /// <param name="remoteAddress">The remote <see cref="EndPoint"/> to connect to.</param>
        /// <param name="localAddress">The local <see cref="EndPoint"/> to connect the remote to.</param>
        /// <returns>The <see cref="IChannel"/>.</returns>
        async Task<IChannel> DoResolveAndConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            IChannel channel = await InitAndRegisterAsync();

            var resolver = InternalResolver;
            if (resolver.IsResolved(remoteAddress))
            {
                // Resolver has no idea about what to do with the specified remote address or it's resolved already.
                await DoConnectAsync(channel, remoteAddress, localAddress);
                return channel;
            }

            EndPoint resolvedAddress;
            try
            {
                resolvedAddress = await resolver.ResolveAsync(remoteAddress);
            }
            catch (Exception)
            {
                try
                {
                    await channel.CloseAsync();
                }
                catch (Exception ex)
                {
                    if (Logger.WarnEnabled) Logger.FailedToCloseChannel(channel, ex);
                }

                throw;
            }

            await DoConnectAsync(channel, resolvedAddress, localAddress);
            return channel;
        }

        static Task DoConnectAsync(IChannel channel,
            EndPoint remoteAddress, EndPoint localAddress)
        {
            // This method is invoked before channelRegistered() is triggered.  Give user handlers a chance to set up
            // the pipeline in its channelRegistered() implementation.
            var promise = channel.NewPromise();
            channel.EventLoop.Execute(() =>
            {
                try
                {
                    if (localAddress is null)
                    {
                        channel.ConnectAsync(remoteAddress).LinkOutcome(promise);
                    }
                    else
                    {
                        channel.ConnectAsync(remoteAddress, localAddress).LinkOutcome(promise);
                    }
                }
                catch (Exception ex)
                {
                    channel.CloseSafe();
                    promise.TrySetException(ex);
                }
            });
            return promise.Task;
        }

        protected override void Init(IChannel channel)
        {
            IChannelPipeline p = channel.Pipeline;
            p.AddLast(null, (string)null, Handler());

            ICollection<ChannelOptionValue> options = Options;
            SetChannelOptions(channel, options, Logger);

            ICollection<AttributeValue> attrs = Attributes;
            foreach (AttributeValue e in attrs)
            {
                e.Set(channel);
            }
        }

        public override Bootstrap Validate()
        {
            base.Validate();
            if (Handler() is null)
            {
                ThrowHelper.ThrowInvalidOperationException_HandlerNotSet();
            }
            return this;
        }

        public override Bootstrap Clone() => new Bootstrap(this);

        /// <summary>
        /// Returns a deep clone of this bootstrap which has the identical configuration except that it uses
        /// the given <see cref="IEventLoopGroup"/>. This method is useful when making multiple <see cref="IChannel"/>s with similar
        /// settings.
        /// </summary>
        public Bootstrap Clone(IEventLoopGroup group) => new Bootstrap(this) { InternalGroup = group };

        public override string ToString()
        {
            var remoteAddress = InternalRemoteAddress;
            if (remoteAddress is null)
            {
                return base.ToString();
            }

            var buf = StringBuilderManager.Allocate().Append(base.ToString());
            buf.Length -= 1;

            return StringBuilderManager.ReturnAndFree(buf.Append(", remoteAddress: ")
                .Append(remoteAddress)
                .Append(')'));
        }
    }
}