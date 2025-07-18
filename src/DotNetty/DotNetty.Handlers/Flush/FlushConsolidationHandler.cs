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

namespace DotNetty.Handlers.Flush
{
    using System;
    using System.Threading;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// <see cref="ChannelDuplexHandler"/> which consolidates <see cref="IChannel.Flush"/> / <see cref="IChannelHandlerContext.Flush"/>
    /// operations (which also includes
    /// <see cref="IChannel.WriteAndFlushAsync(object)"/> / <see cref="IChannel.WriteAndFlushAsync(object, Common.Concurrency.IPromise)"/> and
    /// {@link ChannelOutboundInvoker#writeAndFlush(Object)} /
    /// {@link ChannelOutboundInvoker#writeAndFlush(Object, ChannelPromise)}).
    /// <para>
    /// Flush operations are generally speaking expensive as these may trigger a syscall on the transport level. Thus it is
    /// in most cases (where write latency can be traded with throughput) a good idea to try to minimize flush operations
    /// as much as possible.
    /// </para>
    /// <para>
    /// If a read loop is currently ongoing, {@link #flush(ChannelHandlerContext)} will not be passed on to the next
    /// {@link ChannelOutboundHandler} in the {@link ChannelPipeline}, as it will pick up any pending flushes when
    /// {@link #channelReadComplete(ChannelHandlerContext)} is triggered.
    /// If no read loop is ongoing, the behavior depends on the {@code consolidateWhenNoReadInProgress} constructor argument:
    /// <ul>
    ///     <li>if {@code false}, flushes are passed on to the next handler directly;</li>
    ///     <li>if {@code true}, the invocation of the next handler is submitted as a separate task on the event loop. Under
    ///     high throughput, this gives the opportunity to process other flushes before the task gets executed, thus
    ///     batching multiple flushes into one.</li>
    /// </ul>
    /// If {@code explicitFlushAfterFlushes} is reached the flush will be forwarded as well (whether while in a read loop, or
    /// while batching outside of a read loop).
    /// </para>
    /// If the <see cref="IChannel"/> becomes non-writable it will also try to execute any pending flush operations.
    /// <para>
    /// The <see cref="FlushConsolidationHandler"/> should be put as first <see cref="IChannelHandler"/> in the
    /// <see cref="IChannelPipeline"/> to have the best effect.
    /// </para>
    /// </summary>
    public class FlushConsolidationHandler : ChannelDuplexHandler
    {
        /// <summary>
        /// The default number of flushes after which a flush will be forwarded to downstream handlers (whether while in a
        /// read loop, or while batching outside of a read loop).
        /// </summary>
        public const int DefaultExplicitFlushAfterFlushes = 256;

        private readonly int _explicitFlushAfterFlushes;
        private readonly bool _consolidateWhenNoReadInProgress;
        private int _flushPendingCount;
        private bool _readInProgress;
        private IChannelHandlerContext _ctx;
        private CancellationTokenSource _nextScheduledFlushCts;

        /// <summary>
        /// Create new instance which explicit flush after <see cref="DefaultExplicitFlushAfterFlushes"/> pending flush
        /// operations at the latest.
        /// </summary>
        public FlushConsolidationHandler()
            : this(DefaultExplicitFlushAfterFlushes, false)
        {
        }

        /// <summary>Create new instance which doesn't consolidate flushes when no read is in progress.</summary>
        /// <param name="explicitFlushAfterFlushes">the number of flushes after which an explicit flush will be done.</param>
        public FlushConsolidationHandler(int explicitFlushAfterFlushes)
            : this(explicitFlushAfterFlushes, false)
        {
        }

        /// <summary>Create new instance.</summary>
        /// <param name="explicitFlushAfterFlushes">the number of flushes after which an explicit flush will be done.</param>
        /// <param name="consolidateWhenNoReadInProgress">whether to consolidate flushes even when no read loop is currently
        /// ongoing.</param>
        public FlushConsolidationHandler(int explicitFlushAfterFlushes, bool consolidateWhenNoReadInProgress)
        {
            if ((uint)(explicitFlushAfterFlushes - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(explicitFlushAfterFlushes, ExceptionArgument.explicitFlushAfterFlushes);
            }
            _explicitFlushAfterFlushes = explicitFlushAfterFlushes;
            _consolidateWhenNoReadInProgress = consolidateWhenNoReadInProgress;
        }

        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            _ctx = ctx;
        }

        public override void Flush(IChannelHandlerContext ctx)
        {
            if (_readInProgress)
            {
                // If there is still a read in progress we are sure we will see a channelReadComplete(...) call. Thus
                // we only need to flush if we reach the explicitFlushAfterFlushes limit.
                if (++_flushPendingCount == _explicitFlushAfterFlushes)
                {
                    FlushNow(ctx);
                }
            }
            else if (_consolidateWhenNoReadInProgress)
            {
                // Flush immediately if we reach the threshold, otherwise schedule
                if (++_flushPendingCount == _explicitFlushAfterFlushes)
                {
                    FlushNow(ctx);
                }
                else
                {
                    ScheduleFlush(ctx);
                }
            }
            else
            {
                // Always flush directly
                FlushNow(ctx);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            // This may be the last event in the read loop, so flush now!
            ResetReadAndFlushIfNeeded(ctx);
            ctx.FireChannelReadComplete();
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            _readInProgress = true;
            _ = ctx.FireChannelRead(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            // To ensure we not miss to flush anything, do it now.
            ResetReadAndFlushIfNeeded(ctx);
            _ = ctx.FireExceptionCaught(cause);
        }

        public override void Disconnect(IChannelHandlerContext ctx, IPromise promise)
        {
            // Try to flush one last time if flushes are pending before disconnect the channel.
            ResetReadAndFlushIfNeeded(ctx);
            _ = ctx.DisconnectAsync(promise);
        }

        public override void Close(IChannelHandlerContext ctx, IPromise promise)
        {
            // Try to flush one last time if flushes are pending before close the channel.
            ResetReadAndFlushIfNeeded(ctx);
            _ = ctx.CloseAsync(promise);
        }

        public override void ChannelWritabilityChanged(IChannelHandlerContext ctx)
        {
            if (!ctx.Channel.IsWritable)
            {
                // The writability of the channel changed to false, so flush all consolidated flushes now to free up memory.
                FlushIfNeeded(ctx);
            }
            _ = ctx.FireChannelWritabilityChanged();
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            FlushIfNeeded(ctx);
        }

        private void ResetReadAndFlushIfNeeded(IChannelHandlerContext ctx)
        {
            _readInProgress = false;
            FlushIfNeeded(ctx);
        }

        private void FlushIfNeeded(IChannelHandlerContext ctx)
        {
            if (_flushPendingCount > 0)
            {
                FlushNow(ctx);
            }
        }

        private void FlushNow(IChannelHandlerContext ctx)
        {
            CancelScheduledFlush();
            _flushPendingCount = 0;
            ctx.Flush();
        }

        private void ScheduleFlush(IChannelHandlerContext ctx)
        {
            if (_nextScheduledFlushCts is null)
            {
                var cts = new CancellationTokenSource();
                var flushTask = new FlushTask(this, cts);
                // Run as soon as possible, but still yield to give a chance for additional writes to enqueue.
                ctx.Channel.EventLoop.Execute(flushTask);
                _nextScheduledFlushCts = cts;
            }
        }

        private void CancelScheduledFlush()
        {
            var cts = _nextScheduledFlushCts;
            if (cts is object)
            {
                _nextScheduledFlushCts = null;
                cts.Cancel();
            }
        }

        private void DoFlush()
        {
            if (_flushPendingCount > 0 && !_readInProgress)
            {
                _flushPendingCount = 0;
                _nextScheduledFlushCts = null;
                _ctx.Flush();
            } // else we'll flush when the read completes
        }

        sealed class FlushTask : IRunnable
        {
            private readonly FlushConsolidationHandler _owner;
            private readonly CancellationTokenSource _cts;

            public FlushTask(FlushConsolidationHandler owner, CancellationTokenSource cts)
            {
                _owner = owner;
                _cts = cts;
            }

            public void Run()
            {
                if (_cts.IsCancellationRequested) { return; }
                _owner.DoFlush();
            }
        }
    }
}