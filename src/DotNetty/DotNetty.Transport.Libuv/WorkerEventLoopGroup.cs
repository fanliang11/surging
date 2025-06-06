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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Libuv.Native;

    public sealed class WorkerEventLoopGroup : AbstractEventExecutorGroup<WorkerEventLoop>, IEventLoopGroup
    {
        private static readonly int DefaultEventLoopThreadCount = Environment.ProcessorCount;
        private static readonly TimeSpan StartTimeout = TimeSpan.FromMilliseconds(500);

        private readonly IEventExecutorChooser<WorkerEventLoop> _chooser;
        private readonly WorkerEventLoop[] _eventLoops;
        private readonly DispatcherEventLoop _dispatcherLoop;

        public override bool IsShutdown => _eventLoops.All(eventLoop => eventLoop.IsShutdown);

        public override bool IsTerminated => _eventLoops.All(eventLoop => eventLoop.IsTerminated);

        public override bool IsShuttingDown => _eventLoops.All(eventLoop => eventLoop.IsShuttingDown);

        public override Task TerminationCompletion { get; }

        IEnumerable<IEventLoop> IEventLoopGroup.Items => _eventLoops;

        public override IEnumerable<IEventExecutor> Items => _eventLoops;

        public override IReadOnlyList<WorkerEventLoop> GetItems() => _eventLoops;

        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup)
            : this(eventLoopGroup, DefaultEventLoopThreadCount)
        {
        }

        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads)
            : this(eventLoopGroup, nThreads, DefaultThreadFactory<WorkerEventLoopGroup>.Instance)
        {
        }


        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IThreadFactory threadFactory)
            : this(eventLoopGroup, nThreads, threadFactory, RejectedExecutionHandlers.Reject())
        {
        }

        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler)
            : this(eventLoopGroup, nThreads, threadFactory, rejectedHandler, LoopExecutor.DefaultBreakoutInterval)
        {
        }

        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : this(eventLoopGroup, nThreads, threadFactory, DefaultEventExecutorChooserFactory<WorkerEventLoop>.Instance, rejectedHandler, breakoutInterval)
        {
        }


        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IRejectedExecutionHandler rejectedHandler)
            : this(eventLoopGroup, nThreads, DefaultThreadFactory<WorkerEventLoopGroup>.Instance, rejectedHandler)
        {
        }


        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IEventExecutorChooserFactory<WorkerEventLoop> chooserFactory)
            : this(eventLoopGroup, nThreads, chooserFactory, RejectedExecutionHandlers.Reject())
        {
        }

        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IEventExecutorChooserFactory<WorkerEventLoop> chooserFactory, IRejectedExecutionHandler rejectedHandler)
            : this(eventLoopGroup, nThreads, chooserFactory, rejectedHandler, LoopExecutor.DefaultBreakoutInterval)
        {
        }

        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IEventExecutorChooserFactory<WorkerEventLoop> chooserFactory, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : this(eventLoopGroup, nThreads, DefaultThreadFactory<WorkerEventLoopGroup>.Instance, chooserFactory, rejectedHandler, breakoutInterval)
        {
        }


        public WorkerEventLoopGroup(DispatcherEventLoopGroup eventLoopGroup, int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<WorkerEventLoop> chooserFactory, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
        {
            if (eventLoopGroup is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.eventLoopGroup); }
            if (chooserFactory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chooserFactory); }
            if ((uint)(nThreads - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(nThreads, ExceptionArgument.nThreads); }

            _dispatcherLoop = eventLoopGroup.Dispatcher;
            PipeName = _dispatcherLoop.PipeName;

            // Wait until the pipe is listening to connect
            _dispatcherLoop.WaitForLoopRun(StartTimeout);

            nThreads = 0u >= (uint)nThreads ? DefaultEventLoopThreadCount : nThreads;

            _eventLoops = new WorkerEventLoop[nThreads];
            var terminationTasks = new Task[nThreads];
            for (int i = 0; i < nThreads; i++)
            {
                WorkerEventLoop eventLoop = null;
                bool success = false;
                try
                {
                    eventLoop = new WorkerEventLoop(this, threadFactory, rejectedHandler, breakoutInterval);
                    success = eventLoop.ConnectTask.Wait(StartTimeout);
                    if (!success)
                    {
                        ThrowHelper.ThrowTimeoutException(PipeName);
                    }
                }
                catch (Exception ex)
                {
                    ThrowHelper.ThrowInvalidOperationException_CreateChild(ex);
                }
                finally
                {
                    if (!success)
                    {
                        Task.WhenAll(_eventLoops.Take(i).Select(loop => loop.ShutdownGracefullyAsync())).Wait();
                    }
                }

                _eventLoops[i] = eventLoop;
                terminationTasks[i] = eventLoop.TerminationCompletion;
            }

            _chooser = chooserFactory.NewChooser(_eventLoops);

            TerminationCompletion = Task.WhenAll(terminationTasks);
        }

        internal string PipeName { get; }

        internal void Accept(NativeHandle handle)
        {
            Debug.Assert(_dispatcherLoop is object);
            _dispatcherLoop.Accept(handle);
        }

        IEventLoop IEventLoopGroup.GetNext() => _chooser.GetNext();

        public override WorkerEventLoop GetNext() => _chooser.GetNext();

        public Task RegisterAsync(IChannel channel)
        {
            var nativeChannel = channel as INativeChannel;
            if (nativeChannel is null)
            {
                ThrowHelper.ThrowArgumentException_RegChannel();
            }

            NativeHandle handle = nativeChannel.GetHandle();
            IntPtr loopHandle = handle.LoopHandle();
            for (int i = 0; i < _eventLoops.Length; i++)
            {
                if (_eventLoops[i].UnsafeLoop.Handle == loopHandle)
                {
                    return _eventLoops[i].RegisterAsync(nativeChannel);
                }
            }

            return ThrowHelper.ThrowInvalidOperationException(loopHandle);
        }

        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            foreach (WorkerEventLoop eventLoop in _eventLoops)
            {
                _ = eventLoop.ShutdownGracefullyAsync(quietPeriod, timeout);
            }
            return TerminationCompletion;
        }

        public override bool WaitTermination(TimeSpan timeout)
        {
            PreciseTimeSpan deadline = PreciseTimeSpan.Deadline(timeout);

            for (int i = 0; i < _eventLoops.Length; i++)
            {
                var executor = _eventLoops[i];
                for (; ; )
                {
                    PreciseTimeSpan timeLeft = deadline - PreciseTimeSpan.FromStart;
                    if (timeLeft <= PreciseTimeSpan.Zero) { goto LoopEnd; }

                    if (executor.WaitTermination(timeLeft.ToTimeSpan())) { break; }
                }
            }
        LoopEnd:
            return IsTerminated;
        }
    }
}