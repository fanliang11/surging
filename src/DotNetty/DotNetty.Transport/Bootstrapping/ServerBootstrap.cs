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
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// A <see cref="Bootstrap"/> sub-class which allows easy bootstrapping of <see cref="IServerChannel"/>.
    /// </summary>
    public class ServerBootstrap : AbstractBootstrap<ServerBootstrap, IServerChannel>
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerBootstrap>();

        private readonly CachedReadConcurrentDictionary<ChannelOption, ChannelOptionValue> _childOptions;
        private readonly CachedReadConcurrentDictionary<IConstant, AttributeValue> _childAttrs;

        private IEventLoopGroup v_childGroup;
        private IEventLoopGroup InternalChildGroup
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Volatile.Read(ref v_childGroup);
            set => Interlocked.Exchange(ref v_childGroup, value);
        }

        private IChannelHandler v_childHandler;
        private IChannelHandler InternalChildHandler
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Volatile.Read(ref v_childHandler);
            set => Interlocked.Exchange(ref v_childHandler, value);
        }

        public ServerBootstrap()
        {
            _childOptions = new CachedReadConcurrentDictionary<ChannelOption, ChannelOptionValue>(ChannelOptionComparer.Default);
            _childAttrs = new CachedReadConcurrentDictionary<IConstant, AttributeValue>(ConstantComparer.Default);
        }

        ServerBootstrap(ServerBootstrap bootstrap)
            : base(bootstrap)
        {
            InternalChildGroup = bootstrap.InternalChildGroup;
            InternalChildHandler = bootstrap.InternalChildHandler;
            _childOptions = new CachedReadConcurrentDictionary<ChannelOption, ChannelOptionValue>(bootstrap._childOptions, ChannelOptionComparer.Default);
            _childAttrs = new CachedReadConcurrentDictionary<IConstant, AttributeValue>(bootstrap._childAttrs, ConstantComparer.Default);
        }

        /// <summary>
        /// Specifies the <see cref="IEventLoopGroup"/> which is used for the parent (acceptor) and the child (client).
        /// </summary>
        public override ServerBootstrap Group(IEventLoopGroup group) => Group(group, group);

        /// <summary>
        /// Sets the <see cref="IEventLoopGroup"/> for the parent (acceptor) and the child (client). These
        /// <see cref="IEventLoopGroup"/>'s are used to handle all the events and IO for <see cref="IServerChannel"/>
        /// and <see cref="IChannel"/>'s.
        /// </summary>
        public ServerBootstrap Group(IEventLoopGroup parentGroup, IEventLoopGroup childGroup)
        {
            if (childGroup is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.childGroup); }

            base.Group(parentGroup);
            if (InternalChildGroup is object)
            {
                ThrowHelper.ThrowInvalidOperationException_ChildGroupSetAlready();
            }
            InternalChildGroup = childGroup;
            return this;
        }

        /// <summary>
        /// Allows specification of a <see cref="ChannelOption"/> which is used for the <see cref="IChannel"/>
        /// instances once they get created (after the acceptor accepted the <see cref="IChannel"/>). Use a
        /// value of <c>null</c> to remove a previously set <see cref="ChannelOption"/>.
        /// </summary>
        public ServerBootstrap ChildOption<T>(ChannelOption<T> childOption, T value)
        {
            if (childOption is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.childOption); }

            if (value is null)
            {
                //ChannelOptionValue removed;
                _childOptions.TryRemove(childOption, out _);
            }
            else
            {
                _childOptions[childOption] = new ChannelOptionValue<T>(childOption, value);
            }
            return this;
        }

        /// <summary>
        /// Sets the specific <see cref="AttributeKey{T}"/> with the given value on every child <see cref="IChannel"/>.
        /// If the value is <c>null</c>, the <see cref="AttributeKey{T}"/> is removed.
        /// </summary>
        public ServerBootstrap ChildAttribute<T>(AttributeKey<T> childKey, T value)
            where T : class
        {
            if (childKey is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.childKey); }

            if (value is null)
            {
                //AttributeValue removed;
                _childAttrs.TryRemove(childKey, out _);
            }
            else
            {
                _childAttrs[childKey] = new AttributeValue<T>(childKey, value);
            }
            return this;
        }

        /// <summary>
        /// Sets the <see cref="IChannelHandler"/> which is used to serve the request for the <see cref="IChannel"/>'s.
        /// </summary>
        public ServerBootstrap ChildHandler(IChannelHandler childHandler)
        {
            if (childHandler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.childHandler); }

            InternalChildHandler = childHandler;
            return this;
        }

        /// <summary>
        /// Returns the configured <see cref="IEventLoopGroup"/> which will be used for the child channels or <c>null</c>
        /// if none is configured yet.
        /// </summary>
        public IEventLoopGroup ChildGroup() => InternalChildGroup;

        protected override void Init(IChannel channel)
        {
            SetChannelOptions(channel, Options, Logger);

            foreach (AttributeValue e in Attributes)
            {
                e.Set(channel);
            }

            IChannelPipeline p = channel.Pipeline;

            p.AddLast(new ServerChannelInitializer(
                this,
                _childOptions.Values.ToArray(),
                _childAttrs.Values.ToArray())
            );
        }

        public override ServerBootstrap Validate()
        {
            base.Validate();
            if (InternalChildHandler is null)
            {
                ThrowHelper.ThrowInvalidOperationException_ChildHandlerNotYet();
            }
            if (InternalChildGroup is null)
            {
                if (Logger.WarnEnabled) Logger.ChildGroupIsNotSetUsingParentGroupInstead();
                InternalChildGroup = Group();
            }
            return this;
        }

        sealed class ServerChannelInitializer : ChannelInitializer<IChannel>
        {
            private readonly ServerBootstrap _owner;
            private readonly ChannelOptionValue[] _currentChildOptions;
            private readonly AttributeValue[] _currentChildAttrs;

            public ServerChannelInitializer(
                ServerBootstrap owner,
                ChannelOptionValue[] currentChildOptions,
                AttributeValue[] currentChildAttrs)
            {
                _owner = owner;
                _currentChildOptions = currentChildOptions;
                _currentChildAttrs = currentChildAttrs;
            }

            protected override void InitChannel(IChannel channel)
            {
                var pipeline = channel.Pipeline;
                IChannelHandler channelHandler = _owner.Handler();
                if (channelHandler is object)
                {
                    pipeline.AddLast(name: null, handler: channelHandler);
                }

                channel.EventLoop.Execute(new AddServerBootstrapAcceptorTask(
                    _owner, _currentChildOptions, _currentChildAttrs, channel));
            }

            sealed class AddServerBootstrapAcceptorTask : IRunnable
            {
                private readonly ServerBootstrap _owner;
                private readonly ChannelOptionValue[] _currentChildOptions;
                private readonly AttributeValue[] _currentChildAttrs;
                private readonly IChannel _channel;

                public AddServerBootstrapAcceptorTask(
                    ServerBootstrap owner,
                    ChannelOptionValue[] currentChildOptions,
                    AttributeValue[] currentChildAttrs,
                    IChannel channel)
                {
                    _owner = owner;
                    _currentChildOptions = currentChildOptions;
                    _currentChildAttrs = currentChildAttrs;
                    _channel = channel;
                }

                public void Run()
                {
                    _channel.Pipeline.AddLast(new ServerBootstrapAcceptor(_channel,
                        _owner.InternalChildGroup,
                        _owner.InternalChildHandler,
                        _currentChildOptions,
                        _currentChildAttrs)
                    );
                }
            }
        }

        sealed class ServerBootstrapAcceptor : ChannelHandlerAdapter
        {
            private readonly IEventLoopGroup _childGroup;
            private readonly IChannelHandler _childHandler;
            private readonly ChannelOptionValue[] _childOptions;
            private readonly AttributeValue[] _childAttrs;
            private readonly IRunnable _enableAutoReadTask;

            public ServerBootstrapAcceptor(IChannel channel,
                IEventLoopGroup childGroup, IChannelHandler childHandler,
                ChannelOptionValue[] childOptions, AttributeValue[] childAttrs)
            {
                _childGroup = childGroup;
                _childHandler = childHandler;
                _childOptions = childOptions;
                _childAttrs = childAttrs;

                // Task which is scheduled to re-enable auto-read.
                // It's important to create this Runnable before we try to submit it as otherwise the URLClassLoader may
                // not be able to load the class because of the file limit it already reached.
                //
                // See https://github.com/netty/netty/issues/1328
                _enableAutoReadTask = new EnableAutoReadTask(channel);
            }

            public override void ChannelRead(IChannelHandlerContext ctx, object msg)
            {
                var child = (IChannel)msg;

                child.Pipeline.AddLast(name: null, handler: _childHandler);

                SetChannelOptions(child, _childOptions, Logger);

                for (int i = 0; i < _childAttrs.Length; i++)
                {
                    _childAttrs[i].Set(child);
                }

                // todo: async/await instead?
                try
                {
                    _childGroup.RegisterAsync(child).ContinueWith(
                        s_closeAfterRegisterAction, child,
                        TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception ex)
                {
                    ForceClose(child, ex);
                }
            }

            static readonly Action<Task, object> s_closeAfterRegisterAction = (t, s) => CloseAfterRegisterAction(t, s);
            static void CloseAfterRegisterAction(Task future, object state)
            {
                ForceClose((IChannel)state, future.Exception);
            }

            static void ForceClose(IChannel child, Exception ex)
            {
                child.Unsafe.CloseForcibly();
                if (Logger.WarnEnabled) Logger.ChildGroupIsNotSetUsingParentGroupInstead(child, ex);
            }

            public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
            {
                IChannelConfiguration config = ctx.Channel.Configuration;
                if (config.IsAutoRead)
                {
                    // stop accept new connections for 1 second to allow the channel to recover
                    // See https://github.com/netty/netty/issues/1328
                    config.IsAutoRead = false;
                    _ = ctx.Channel.EventLoop.Schedule(_enableAutoReadTask, TimeSpan.FromSeconds(1));
                }
                // still let the ExceptionCaught event flow through the pipeline to give the user
                // a chance to do something with it
                ctx.FireExceptionCaught(cause);
            }

            sealed class EnableAutoReadTask : IRunnable
            {
                private readonly IChannel _channel;

                public EnableAutoReadTask(IChannel channel) => _channel = channel;

                public void Run()
                {
                    _channel.Configuration.IsAutoRead = true;
                }
            }
        }

        public override ServerBootstrap Clone() => new ServerBootstrap(this);

        public override string ToString()
        {
            var buf = StringBuilderManager.Allocate().Append(base.ToString());
            buf.Length -= 1;
            buf.Append(", ");
            var childGroup = InternalChildGroup;
            if (childGroup is object)
            {
                buf.Append("childGroup: ")
                    .Append(childGroup.GetType().Name)
                    .Append(", ");
            }
            buf.Append("childOptions: ")
                .Append(_childOptions.ToDebugString())
                .Append(", ");
            // todo: attrs
            //lock (childAttrs)
            //{
            //    if (!childAttrs.isEmpty())
            //    {
            //        buf.Append("childAttrs: ");
            //        buf.Append(childAttrs);
            //        buf.Append(", ");
            //    }
            //}
            var childHandler = InternalChildHandler;
            if (childHandler is object)
            {
                buf.Append("childHandler: ");
                buf.Append(childHandler);
                buf.Append(", ");
            }
            if (buf[buf.Length - 1] == '(')
            {
                buf.Append(')');
            }
            else
            {
                buf[buf.Length - 2] = ')';
                buf.Length -= 1;
            }

            return StringBuilderManager.ReturnAndFree(buf);
        }
    }
}