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

namespace DotNetty.Handlers.Timeout
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Raises a <see cref="WriteTimeoutException"/> when a write operation cannot finish in a certain period of time.
    /// 
    /// <para>
    /// <example>
    /// 
    /// The connection is closed when a write operation cannot finish in 30 seconds.
    ///
    /// <c>
    /// var bootstrap = new <see cref="DotNetty.Transport.Bootstrapping.ServerBootstrap"/>();
    ///
    /// bootstrap.ChildHandler(new ActionChannelInitializer&lt;ISocketChannel&gt;(channel =>
    /// {
    ///     IChannelPipeline pipeline = channel.Pipeline;
    ///     
    ///     pipeline.AddLast("writeTimeoutHandler", new <see cref="WriteTimeoutHandler"/>(30);
    ///     pipeline.AddLast("myHandler", new MyHandler());
    /// }    
    /// </c>
    /// 
    /// <c>
    /// public class MyHandler : ChannelDuplexHandler 
    /// {
    ///     public override void ExceptionCaught(<see cref="IChannelHandlerContext"/> context, <see cref="Exception"/> exception)
    ///     {
    ///         if(exception is <see cref="WriteTimeoutException"/>) 
    ///         {
    ///             // do somethind
    ///         }
    ///         else
    ///         {
    ///             base.ExceptionCaught(context, cause);
    ///         }
    ///      }
    /// }
    /// </c>
    /// 
    /// </example>
    /// </para>
    /// <see cref="ReadTimeoutHandler"/>
    /// <see cref="IdleStateHandler"/>
    /// </summary>
    public class WriteTimeoutHandler : ChannelHandlerAdapter
    {
        static readonly TimeSpan MinTimeout = TimeSpan.FromMilliseconds(1);
        readonly TimeSpan _timeout;
        bool _closed;

        /// <summary>
        /// A doubly-linked list to track all WriteTimeoutTasks.
        /// </summary>
        readonly LinkedList<WriteTimeoutTask> tasks = new LinkedList<WriteTimeoutTask>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetty.Handlers.Timeout.ReadTimeoutHandler"/> class.
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds.</param>
        public WriteTimeoutHandler(int timeoutSeconds)
            : this(TimeSpan.FromSeconds(timeoutSeconds))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetty.Handlers.Timeout.ReadTimeoutHandler"/> class.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public WriteTimeoutHandler(TimeSpan timeout)
        {
            _timeout = (timeout > TimeSpan.Zero)
                 ? TimeUtil.Max(timeout, MinTimeout)
                 : TimeSpan.Zero;
        }

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            if (_timeout.Ticks > 0)
            {
                promise = promise.Unvoid();
                ScheduleTimeout(context, promise.Task);
            }

            _ = context.WriteAsync(message, promise);
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            LinkedListNode<WriteTimeoutTask> task = tasks.Last;
            while (task is object)
            {
                _ = task.Value.ScheduledTask.Cancel();
                tasks.RemoveLast();
                task = tasks.Last;
            }
        }

        void ScheduleTimeout(IChannelHandlerContext context, Task future)
        {
            // Schedule a timeout.
            var task = new WriteTimeoutTask(context, future, this);

            task.ScheduledTask = context.Executor.Schedule(task, _timeout);

            if (!task.ScheduledTask.Completion.IsCompleted)
            {
                AddWriteTimeoutTask(task);

                // Cancel the scheduled timeout if the flush promise is complete.
                _ = future.ContinueWith(WriteTimeoutTask.OperationCompleteAction, task, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        void AddWriteTimeoutTask(WriteTimeoutTask task)
        {
            _ = tasks.AddLast(task);
        }

        void RemoveWriteTimeoutTask(WriteTimeoutTask task)
        {
            _ = tasks.Remove(task);
        }

        /// <summary>
        /// Is called when a write timeout was detected
        /// </summary>
        /// <param name="context">Context.</param>
        protected virtual void WriteTimedOut(IChannelHandlerContext context)
        {
            if (!_closed)
            {
                _ = context.FireExceptionCaught(WriteTimeoutException.Instance);
                _ = context.CloseAsync();
                _closed = true;
            }
        }

        sealed class WriteTimeoutTask : IRunnable
        {
            readonly WriteTimeoutHandler _handler;
            readonly IChannelHandlerContext _context;
            readonly Task _future;

            public static readonly Action<Task, object> OperationCompleteAction = (t, s) => HandleOperationComplete(t, s);

            public WriteTimeoutTask(IChannelHandlerContext context, Task future, WriteTimeoutHandler handler)
            {
                _context = context;
                _future = future;
                _handler = handler;
            }

            internal static void HandleOperationComplete(Task future, object state)
            {
                var writeTimeoutTask = (WriteTimeoutTask)state;

                // ScheduledTask has already be set when reaching here
                _ = writeTimeoutTask.ScheduledTask.Cancel();
                writeTimeoutTask._handler.RemoveWriteTimeoutTask(writeTimeoutTask);
            }

            public IScheduledTask ScheduledTask { get; set; }

            public void Run()
            {
                // Was not written yet so issue a write timeout
                // The future itself will be failed with a ClosedChannelException once the close() was issued
                // See https://github.com/netty/netty/issues/2159
                if (!_future.IsCompleted)
                {
                    try
                    {
                        _handler.WriteTimedOut(_context);
                    }
                    catch (Exception ex)
                    {
                        _ = _context.FireExceptionCaught(ex);
                    }
                }

                _handler.RemoveWriteTimeoutTask(this);
            }
        }
    }
}

