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

namespace DotNetty.Transport.Libuv
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;

    public sealed class DispatcherEventLoopGroup : AbstractEventExecutorGroup<DispatcherEventLoop>, IEventLoopGroup
    {
        private readonly DispatcherEventLoop _dispatcherEventLoop;
        private readonly DispatcherEventLoop[] _eventLoops;

        public DispatcherEventLoopGroup()
        {
            _dispatcherEventLoop = new DispatcherEventLoop(this);
            _eventLoops = new[] { _dispatcherEventLoop };
        }

        public override bool IsShutdown => _dispatcherEventLoop.IsShutdown;

        public override bool IsTerminated => _dispatcherEventLoop.IsTerminated;

        public override bool IsShuttingDown => _dispatcherEventLoop.IsShuttingDown;

        public override Task TerminationCompletion => _dispatcherEventLoop.TerminationCompletion;

        internal DispatcherEventLoop Dispatcher => _dispatcherEventLoop;

        public override IEnumerable<IEventExecutor> Items => _eventLoops;

        IEnumerable<IEventLoop> IEventLoopGroup.Items => _eventLoops;

        public override IReadOnlyList<DispatcherEventLoop> GetItems() => _eventLoops;

        IEventLoop IEventLoopGroup.GetNext() => _dispatcherEventLoop;

        public override DispatcherEventLoop GetNext() => _dispatcherEventLoop;

        public Task RegisterAsync(IChannel channel) => _dispatcherEventLoop.RegisterAsync(channel);

        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            _ = _dispatcherEventLoop.ShutdownGracefullyAsync(quietPeriod, timeout);
            return TerminationCompletion;
        }

        public override bool WaitTermination(TimeSpan timeout)
        {
            return _dispatcherEventLoop.TerminationCompletion.Wait(timeout);
        }
    }
}