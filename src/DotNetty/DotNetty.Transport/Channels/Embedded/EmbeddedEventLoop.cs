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

namespace DotNetty.Transport.Channels.Embedded
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using Thread = DotNetty.Common.Concurrency.XThread;

    sealed class EmbeddedEventLoop : AbstractScheduledEventExecutor, IEventLoop
    {
        readonly Deque<IRunnable> _tasks = new Deque<IRunnable>(2);

        public new IEventLoop GetNext() => this;

        public Task RegisterAsync(IChannel channel) => channel.Unsafe.RegisterAsync(this);

        protected override bool HasTasks => _tasks.NonEmpty;

        public override bool IsShuttingDown => false;

        public override Task TerminationCompletion => ThrowHelper.FromNotSupportedException();

        public override bool IsShutdown => false;

        public override bool IsTerminated => false;

        public new IEventLoopGroup Parent => (IEventLoopGroup)base.Parent;

        protected override IEnumerable<IEventExecutor> GetItems() => new[] { this };

        public new IEnumerable<IEventLoop> Items => new[] { this };

        public override bool IsInEventLoop(Thread thread) => true;

        public override void Execute(IRunnable command)
        {
            if (command is null)
            {
                ThrowHelper.ThrowNullReferenceException_Command();
            }
            _tasks.AddLast​(command);
        }

        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            return ThrowHelper.FromNotSupportedException();
        }

        internal long NextScheduledTask() => NextScheduledTaskNanos();

        internal void RunTasks()
        {
            while (_tasks.TryRemoveFirst(out var task))
            {
                task.Run();
            }
        }

        internal long RunScheduledTasks()
        {
            var time = PreciseTime.NanoTime();
            while (true)
            {
                IRunnable task = PollScheduledTask(time);
                if (task is null)
                {
                    return NextScheduledTaskNanos();
                }
                task.Run();
            }
        }

        internal new void CancelScheduledTasks() => base.CancelScheduledTasks();

        public override bool WaitTermination(TimeSpan timeout)
        {
            return false;
        }
    }
}