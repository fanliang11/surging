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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels
{
    using System.Net;
    using System.Runtime.CompilerServices;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    partial class AbstractChannelHandlerContext
    {
        #region -- class BindTask --

        sealed class BindTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;
            private readonly IPromise _promise;
            private readonly EndPoint _localAddress;

            public BindTask(AbstractChannelHandlerContext ctx, IPromise promise, EndPoint localAddress)
            {
                _ctx = ctx;
                _promise = promise;
                _localAddress = localAddress;
            }

            public void Run()
            {
                _ctx.InvokeBindAsync(_localAddress).LinkOutcome(_promise);
            }
        }

        #endregion

        #region -- class ConnectTask --

        sealed class ConnectTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;
            private readonly IPromise _promise;
            private readonly EndPoint _remoteAddress;
            private readonly EndPoint _localAddress;

            public ConnectTask(AbstractChannelHandlerContext ctx, IPromise promise, EndPoint remoteAddress, EndPoint localAddress)
            {
                _ctx = ctx;
                _promise = promise;
                _remoteAddress = remoteAddress;
                _localAddress = localAddress;
            }

            public void Run()
            {
                _ctx.InvokeConnectAsync(_remoteAddress, _localAddress).LinkOutcome(_promise);
            }
        }

        #endregion

        #region -- class DisconnectTask --

        sealed class DisconnectTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;
            private readonly IPromise _promise;

            public DisconnectTask(AbstractChannelHandlerContext ctx, IPromise promise)
            {
                _ctx = ctx;
                _promise = promise;
            }

            public void Run()
            {
                _ctx.InvokeDisconnect(_promise);
            }
        }

        #endregion

        #region -- class CloseTask --

        sealed class CloseTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;
            private readonly IPromise _promise;

            public CloseTask(AbstractChannelHandlerContext ctx, IPromise promise)
            {
                _ctx = ctx;
                _promise = promise;
            }

            public void Run()
            {
                _ctx.InvokeClose(_promise);
            }
        }

        #endregion

        #region -- class DeregisterTask --

        sealed class DeregisterTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;
            private readonly IPromise _promise;

            public DeregisterTask(AbstractChannelHandlerContext ctx, IPromise promise)
            {
                _ctx = ctx;
                _promise = promise;
            }

            public void Run()
            {
                _ctx.InvokeDeregister(_promise);
            }
        }

        #endregion

        #region -- class ReadCompleteTask --

        sealed class ReadCompleteTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;

            public ReadCompleteTask(AbstractChannelHandlerContext ctx) => _ctx = ctx;

            public void Run()
            {
                _ctx.InvokeChannelReadComplete();
            }
        }

        #endregion

        #region -- class ReadTask --

        sealed class ReadTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;

            public ReadTask(AbstractChannelHandlerContext ctx) => _ctx = ctx;

            public void Run()
            {
                _ctx.InvokeRead();
            }
        }

        #endregion

        #region -- class WritableStateChangedTask --

        sealed class WritableStateChangedTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;

            public WritableStateChangedTask(AbstractChannelHandlerContext ctx) => _ctx = ctx;

            public void Run()
            {
                _ctx.InvokeChannelWritabilityChanged();
            }
        }

        #endregion

        #region -- class FlushTask --

        sealed class FlushTask : IRunnable
        {
            private readonly AbstractChannelHandlerContext _ctx;

            public FlushTask(AbstractChannelHandlerContext ctx) => _ctx = ctx;

            public void Run()
            {
                _ctx.InvokeFlush();
            }
        }

        #endregion

        #region -- class WriteTask --

        sealed class WriteTask : IRunnable
        {
            private static readonly ThreadLocalPool<WriteTask> Recycler = new ThreadLocalPool<WriteTask>(handle => new WriteTask(handle));

            public static WriteTask NewInstance(AbstractChannelHandlerContext ctx, object msg, IPromise promise, bool flush)
            {
                WriteTask task = Recycler.Take();
                Init(task, ctx, msg, promise, flush);
                return task;
            }

            private static readonly bool EstimateTaskSizeOnSubmit =
                SystemPropertyUtil.GetBoolean("io.netty.transport.estimateSizeOnSubmit", true);

            // Assuming compressed oops, 12 bytes obj header, 4 ref fields and one int field
            private static readonly int WriteTaskOverhead =
                SystemPropertyUtil.GetInt("io.netty.transport.writeTaskSizeOverhead", 32);

            private ThreadLocalPool.Handle _handle;
            private AbstractChannelHandlerContext _ctx;
            private object _msg;
            private IPromise _promise;
            private int _size; // sign bit controls flush

            private static void Init(WriteTask task, AbstractChannelHandlerContext ctx, object msg, IPromise promise, bool flush)
            {
                task._ctx = ctx;
                task._msg = msg;
                task._promise = promise;

                if (EstimateTaskSizeOnSubmit)
                {
                    task._size = ctx._pipeline.EstimatorHandle.Size(msg) + WriteTaskOverhead;
                    ctx._pipeline.IncrementPendingOutboundBytes(task._size);
                }
                else
                {
                    task._size = 0;
                }
                if (flush)
                {
                    task._size |= int.MinValue;
                }
            }

            private WriteTask(ThreadLocalPool.Handle handle)
            {
                _handle = handle;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Run()
            {
                try
                {
                    DecrementPendingOutboundBytes();
                    if (SharedConstants.TooBigOrNegative >= (uint)_size)
                    {
                        _ctx.InvokeWrite(_msg, _promise);
                    }
                    else
                    {
                        _ctx.InvokeWriteAndFlush(_msg, _promise);
                    }
                }
                finally
                {
                    Recycle();
                }
            }

            internal void Cancel()
            {
                try
                {
                    DecrementPendingOutboundBytes();
                }
                finally
                {
                    Recycle();
                }
            }

            void DecrementPendingOutboundBytes()
            {
                if (EstimateTaskSizeOnSubmit)
                {
                    _ctx._pipeline.DecrementPendingOutboundBytes(_size & int.MaxValue);
                }
            }

            void Recycle()
            {
                // Set to null so the GC can collect them directly
                _ctx = null; 
                _msg = null;
                _promise = null; 
                _handle.Release(this);
                //_handle = null;//fanly update
                
            }
        }

        #endregion

        #region -- class ContextTasks --

        sealed class ContextTasks
        {
            private readonly AbstractChannelHandlerContext _ctx;

            public readonly IRunnable InvokeChannelReadCompleteTask;

            public readonly IRunnable InvokeReadTask;

            public readonly IRunnable InvokeChannelWritableStateChangedTask;

            public readonly IRunnable InvokeFlushTask;

            public ContextTasks(AbstractChannelHandlerContext ctx)
            {
                _ctx = ctx;

                InvokeChannelReadCompleteTask = new ReadCompleteTask(ctx);
                InvokeReadTask = new ReadTask(ctx);
                InvokeChannelWritableStateChangedTask = new WritableStateChangedTask(ctx);
                InvokeFlushTask = new FlushTask(ctx);
            }

        }

        #endregion
    }
}
