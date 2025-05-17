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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Triggers an <see cref="IdleStateEvent"/> when a <see cref="IChannel"/> has not performed
    /// read, write, or both operation for a while.
    /// 
    /// <para>
    /// 
    /// <h3>Supported idle states</h3>
    /// <table border="1">
    ///     <tr>
    ///         <th>Property</th><th>Meaning</th>
    ///     </tr>
    ///     <tr>
    ///         <td><code>readerIdleTime</code></td>
    ///         <td>an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.ReaderIdle"/>
    ///             will be triggered when no read was performed for the specified period of
    ///             time.  Specify <code>0</code> to disable.
    ///         </td>
    ///     </tr>
    ///     <tr>
    ///         <td><code>writerIdleTime</code></td>
    ///         <td>an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.WriterIdle"/>
    ///             will be triggered when no write was performed for the specified period of
    ///             time.  Specify <code>0</code> to disable.</td>
    ///     </tr>
    ///     <tr>
    ///         <td><code>allIdleTime</code></td>
    ///         <td>an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.AllIdle"/>
    ///             will be triggered when neither read nor write was performed for the
    ///             specified period of time.  Specify <code>0</code> to disable.</td>
    ///     </tr>
    /// </table>
    /// </para>
    /// 
    /// <para>
    /// 
    /// <example>
    ///
    /// An example that sends a ping message when there is no outbound traffic
    /// for 30 seconds.  The connection is closed when there is no inbound traffic
    /// for 60 seconds.
    ///
    /// <c>
    /// var bootstrap = new <see cref="DotNetty.Transport.Bootstrapping.ServerBootstrap"/>();
    ///
    /// bootstrap.ChildHandler(new ActionChannelInitializer&lt;ISocketChannel&gt;(channel =>
    /// {
    ///     IChannelPipeline pipeline = channel.Pipeline;
    ///     
    ///     pipeline.AddLast("idleStateHandler", new <see cref="IdleStateHandler"/>(60, 30, 0);
    ///     pipeline.AddLast("myHandler", new MyHandler());
    /// }    
    /// </c>
    /// 
    /// Handler should handle the <see cref="IdleStateEvent"/>  triggered by <see cref="IdleStateHandler"/>.
    /// 
    /// <c>
    /// public class MyHandler : ChannelDuplexHandler 
    /// {
    ///     public override void UserEventTriggered(<see cref="IChannelHandlerContext"/> context, <see cref="object"/> evt)
    ///     {
    ///         if(evt is <see cref="IdleStateEvent"/>) 
    ///         {
    ///             <see cref="IdleStateEvent"/> e = (<see cref="IdleStateEvent"/>) evt;
    ///             if (e.State == <see cref="IdleState"/>.ReaderIdle) 
    ///             {
    ///                 ctx.close();
    ///             } 
    ///             else if(e.State == <see cref="IdleState"/>.WriterIdle) 
    ///             {
    ///                 ctx.writeAndFlush(new PingMessage());
    ///             }
    ///          }
    ///      }
    /// }
    /// </c>
    /// </example>
    /// </para>
    /// 
    /// <seealso cref="ReadTimeoutHandler"/>
    /// <seealso cref="WriteTimeoutHandler"/>
    /// </summary>
    public partial class IdleStateHandler : ChannelDuplexHandler
    {
        static readonly TimeSpan MinTimeout = TimeSpan.FromMilliseconds(1);
        static readonly Action<Task, object> writeListener = (t, s) => WrappingWriteListener(t, s);

        readonly bool _observeOutput;
        readonly TimeSpan _readerIdleTime;
        readonly TimeSpan _writerIdleTime;
        readonly TimeSpan _allIdleTime;

        IScheduledTask _readerIdleTimeout;
        TimeSpan _lastReadTime;
        bool _firstReaderIdleEvent = true;

        IScheduledTask _writerIdleTimeout;
        TimeSpan _lastWriteTime;
        bool _firstWriterIdleEvent = true;

        IScheduledTask _allIdleTimeout;
        bool _firstAllIdleEvent = true;

        // 0 - none, 1 - initialized, 2 - destroyed
        byte _state;
        bool _reading;

        TimeSpan _lastChangeCheckTimeStamp;
        int _lastMessageHashCode;
        long _lastPendingWriteBytes;
        long _lastFlushProgress;

        static readonly Action<object, object> ReadTimeoutAction = (h, c) => WrappingHandleReadTimeout(h, c); // WrapperTimeoutHandler(HandleReadTimeout);
        static readonly Action<object, object> WriteTimeoutAction = (h, c) => WrappingHandleWriteTimeout(h, c); // WrapperTimeoutHandler(HandleWriteTimeout);
        static readonly Action<object, object> AllTimeoutAction = (h, c) => WrappingHandleAllTimeout(h, c); // WrapperTimeoutHandler(HandleAllTimeout);

        /// <summary>
        /// Initializes a new instance firing <see cref="IdleStateEvent"/>s.
        /// </summary>
        /// <param name="readerIdleTimeSeconds">
        ///     an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.ReaderIdle"/>
        ///     will be triggered when no read was performed for the specified
        ///     period of time.  Specify <code>0</code> to disable.
        /// </param>
        /// <param name="writerIdleTimeSeconds">
        ///     an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.WriterIdle"/>
        ///     will be triggered when no write was performed for the specified
        ///     period of time.  Specify <code>0</code> to disable.
        /// </param>
        /// <param name="allIdleTimeSeconds">
        ///     an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.AllIdle"/>
        ///     will be triggered when neither read nor write was performed for
        ///     the specified period of time.  Specify <code>0</code> to disable.
        /// </param>
        public IdleStateHandler(
            int readerIdleTimeSeconds,
            int writerIdleTimeSeconds,
            int allIdleTimeSeconds)
            : this(TimeSpan.FromSeconds(readerIdleTimeSeconds),
                   TimeSpan.FromSeconds(writerIdleTimeSeconds),
                   TimeSpan.FromSeconds(allIdleTimeSeconds))
        {
        }

        /// <summary>
        /// <see cref="IdleStateHandler.IdleStateHandler(bool, TimeSpan, TimeSpan, TimeSpan)"/>
        /// </summary>
        public IdleStateHandler(TimeSpan readerIdleTime, TimeSpan writerIdleTime, TimeSpan allIdleTime)
            : this(false, readerIdleTime, writerIdleTime, allIdleTime)
        {
        }

        /// <summary>
        /// Initializes a new instance firing <see cref="IdleStateEvent"/>s.
        /// </summary>
        /// <param name="observeOutput">
        ///     whether or not the consumption of <code>bytes</code> should be taken into
        ///     consideration when assessing write idleness. The default is <c>false</c>.
        /// </param>
        /// <param name="readerIdleTime">
        ///     an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.ReaderIdle"/>
        ///     will be triggered when no read was performed for the specified
        ///     period of time.  Specify <see cref="TimeSpan.Zero"/> to disable.
        /// </param>
        /// <param name="writerIdleTime">
        ///     an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.WriterIdle"/>
        ///     will be triggered when no write was performed for the specified
        ///     period of time.  Specify <see cref="TimeSpan.Zero"/> to disable.
        /// </param>
        /// <param name="allIdleTime">
        ///     an <see cref="IdleStateEvent"/> whose state is <see cref="IdleState.AllIdle"/>
        ///     will be triggered when neither read nor write was performed for
        ///     the specified period of time.  Specify <see cref="TimeSpan.Zero"/> to disable.
        /// </param>
        public IdleStateHandler(bool observeOutput,
            TimeSpan readerIdleTime, TimeSpan writerIdleTime, TimeSpan allIdleTime)
        {
            _observeOutput = observeOutput;

            _readerIdleTime = readerIdleTime > TimeSpan.Zero
                ? TimeUtil.Max(readerIdleTime, IdleStateHandler.MinTimeout)
                : TimeSpan.Zero;

            _writerIdleTime = writerIdleTime > TimeSpan.Zero
                ? TimeUtil.Max(writerIdleTime, IdleStateHandler.MinTimeout)
                : TimeSpan.Zero;

            _allIdleTime = allIdleTime > TimeSpan.Zero
                ? TimeUtil.Max(allIdleTime, IdleStateHandler.MinTimeout)
                : TimeSpan.Zero;
        }

        /// <summary>
        /// Return the readerIdleTime that was given when instance this class in milliseconds.
        /// </summary>
        /// <returns>The reader idle time in millis.</returns>
        public TimeSpan ReaderIdleTime
        {
            get { return _readerIdleTime; }
        }

        /// <summary>
        /// Return the writerIdleTime that was given when instance this class in milliseconds.
        /// </summary>
        /// <returns>The writer idle time in millis.</returns>
        public TimeSpan WriterIdleTime
        {
            get { return _writerIdleTime; }
        }

        /// <summary>
        /// Return the allIdleTime that was given when instance this class in milliseconds.
        /// </summary>
        /// <returns>The all idle time in millis.</returns>
        public TimeSpan AllIdleTime
        {
            get { return _allIdleTime; }
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            if (context.Channel.IsActive && context.Channel.IsRegistered)
            {
                // channelActive() event has been fired already, which means this.channelActive() will
                // not be invoked. We have to initialize here instead.
                Initialize(context);
            }
            else
            {
                // channelActive() event has not been fired yet.  this.channelActive() will be invoked
                // and initialization will occur there.
            }
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            Destroy();
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            // Initialize early if channel is active already.
            if (context.Channel.IsActive)
            {
                Initialize(context);
            }

            base.ChannelRegistered(context);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            // This method will be invoked only if this handler was added
            // before channelActive() event is fired.  If a user adds this handler
            // after the channelActive() event, initialize() will be called by beforeAdd().
            Initialize(context);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Destroy();
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (_readerIdleTime.Ticks > 0 || _allIdleTime.Ticks > 0)
            {
                _reading = true;
                _firstReaderIdleEvent = _firstAllIdleEvent = true;
            }

            _ = context.FireChannelRead(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            if ((_readerIdleTime.Ticks > 0 || _allIdleTime.Ticks > 0) && _reading)
            {
                _lastReadTime = Ticks();
                _reading = false;
            }

            _ = context.FireChannelReadComplete();
        }

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            if (_writerIdleTime.Ticks > 0 || _allIdleTime.Ticks > 0)
            {
                Task task = context.WriteAsync(message, promise.Unvoid());
                _ = task.ContinueWith(writeListener, this, TaskContinuationOptions.ExecuteSynchronously);
            }
            else
            {
                _ = context.WriteAsync(message, promise);
            }
        }

        void Initialize(IChannelHandlerContext context)
        {
            // Avoid the case where destroy() is called before scheduling timeouts.
            // See: https://github.com/netty/netty/issues/143
            switch (_state)
            {
                case 1:
                case 2:
                    return;
            }

            _state = 1;
            InitOutputChanged(context);

            _lastReadTime = _lastWriteTime = Ticks();
            if (_readerIdleTime.Ticks > 0)
            {
                _readerIdleTimeout = Schedule(context, ReadTimeoutAction, this, context,
                    _readerIdleTime);
            }

            if (_writerIdleTime.Ticks > 0)
            {
                _writerIdleTimeout = Schedule(context, WriteTimeoutAction, this, context,
                    _writerIdleTime);
            }

            if (_allIdleTime.Ticks > 0)
            {
                _allIdleTimeout = Schedule(context, AllTimeoutAction, this, context,
                    _allIdleTime);
            }
        }

        /// <summary>
        /// This method is visible for testing!
        /// </summary>
        /// <returns></returns>
        internal virtual TimeSpan Ticks() => TimeUtil.GetSystemTime();

        /// <summary>
        /// This method is visible for testing!
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="task"></param>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        internal virtual IScheduledTask Schedule(IChannelHandlerContext ctx, Action<object, object> task,
            object context, object state, TimeSpan delay)
        {
            return ctx.Executor.Schedule(task, context, state, delay);
        }

        void Destroy()
        {
            _state = 2;

            if (_readerIdleTimeout is object)
            {
                _ = _readerIdleTimeout.Cancel();
                _readerIdleTimeout = null;
            }

            if (_writerIdleTimeout is object)
            {
                _ = _writerIdleTimeout.Cancel();
                _writerIdleTimeout = null;
            }

            if (_allIdleTimeout is object)
            {
                _ = _allIdleTimeout.Cancel();
                _allIdleTimeout = null;
            }
        }

        /// <summary>
        /// Is called when an <see cref="IdleStateEvent"/> should be fired. This implementation calls
        /// <see cref="IChannelHandlerContext.FireUserEventTriggered(object)"/>.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="stateEvent">Evt.</param>
        protected virtual void ChannelIdle(IChannelHandlerContext context, IdleStateEvent stateEvent)
        {
            _ = context.FireUserEventTriggered(stateEvent);
        }

        /// <summary>
        /// Returns a <see cref="IdleStateEvent"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="first"></param>
        /// <returns></returns>
        protected virtual IdleStateEvent NewIdleStateEvent(IdleState state, bool first)
        {
            switch (state)
            {
                case IdleState.AllIdle:
                    return first ? IdleStateEvent.FirstAllIdleStateEvent : IdleStateEvent.AllIdleStateEvent;
                case IdleState.ReaderIdle:
                    return first ? IdleStateEvent.FirstReaderIdleStateEvent : IdleStateEvent.ReaderIdleStateEvent;
                case IdleState.WriterIdle:
                    return first ? IdleStateEvent.FirstWriterIdleStateEvent : IdleStateEvent.WriterIdleStateEvent;
                default:
                    ThrowHelper.ThrowArgumentException_IdleState(state, first); return null;
            }
        }

        /// <summary>
        /// <see cref="HasOutputChanged(IChannelHandlerContext, bool)"/>
        /// </summary>
        /// <param name="ctx"></param>
        private void InitOutputChanged(IChannelHandlerContext ctx)
        {
            if (_observeOutput)
            {
                ChannelOutboundBuffer buf = ctx.Channel.Unsafe.OutboundBuffer;

                if (buf is object)
                {
                    _lastMessageHashCode = RuntimeHelpers.GetHashCode(buf.Current);
                    _lastPendingWriteBytes = buf.TotalPendingWriteBytes;
                    _lastFlushProgress = buf.CurrentProgress();
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> if and only if the <see cref="IdleStateHandler.IdleStateHandler(bool, TimeSpan, TimeSpan, TimeSpan)"/>
        /// was constructed
        /// with <code>observeOutput</code> enabled and there has been an observed change in the
        /// <see cref="ChannelOutboundBuffer"/> between two consecutive calls of this method.
        /// https://github.com/netty/netty/issues/6150
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="first"></param>
        /// <returns></returns>
        private bool HasOutputChanged(IChannelHandlerContext ctx, bool first)
        {
            if (_observeOutput)
            {

                // We can take this shortcut if the ChannelPromises that got passed into write()
                // appear to complete. It indicates "change" on message level and we simply assume
                // that there's change happening on byte level. If the user doesn't observe channel
                // writability events then they'll eventually OOME and there's clearly a different
                // problem and idleness is least of their concerns.
                if (_lastChangeCheckTimeStamp != _lastWriteTime)
                {
                    _lastChangeCheckTimeStamp = _lastWriteTime;

                    // But this applies only if it's the non-first call.
                    if (!first)
                    {
                        return true;
                    }
                }

                ChannelOutboundBuffer buf = ctx.Channel.Unsafe.OutboundBuffer;

                if (buf is object)
                {
                    int messageHashCode = RuntimeHelpers.GetHashCode(buf.Current);
                    long pendingWriteBytes = buf.TotalPendingWriteBytes;

                    if (messageHashCode != _lastMessageHashCode || pendingWriteBytes != _lastPendingWriteBytes)
                    {
                        _lastMessageHashCode = messageHashCode;
                        _lastPendingWriteBytes = pendingWriteBytes;

                        if (!first)
                        {
                            return true;
                        }
                    }

                    long flushProgress = buf.CurrentProgress();
                    if (flushProgress != _lastFlushProgress)
                    {
                        _lastFlushProgress = flushProgress;

                        if (!first)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //static Action<object, object> WrapperTimeoutHandler(Action<IdleStateHandler, IChannelHandlerContext> action)
        //{
        //    return (handler, ctx) =>
        //    {
        //        var self = (IdleStateHandler)handler; // instead of this
        //        var context = (IChannelHandlerContext)ctx;

        //        if (!context.Channel.Open)
        //        {
        //            return;
        //        }

        //        action(self, context);
        //    };
        //}

        static void HandleReadTimeout(IdleStateHandler self, IChannelHandlerContext context)
        {
            TimeSpan nextDelay = self._readerIdleTime;

            if (!self._reading)
            {
                nextDelay -= self.Ticks() - self._lastReadTime;
            }

            if (nextDelay.Ticks <= 0)
            {
                // Reader is idle - set a new timeout and notify the callback.
                self._readerIdleTimeout =
                    self.Schedule(context, ReadTimeoutAction, self, context,
                        self._readerIdleTime);

                bool first = self._firstReaderIdleEvent;
                self._firstReaderIdleEvent = false;

                try
                {
                    IdleStateEvent stateEvent = self.NewIdleStateEvent(IdleState.ReaderIdle, first);
                    self.ChannelIdle(context, stateEvent);
                }
                catch (Exception ex)
                {
                    _ = context.FireExceptionCaught(ex);
                }
            }
            else
            {
                // Read occurred before the timeout - set a new timeout with shorter delay.
                self._readerIdleTimeout = self.Schedule(context, ReadTimeoutAction, self, context,
                    nextDelay);
            }
        }

        static void HandleWriteTimeout(IdleStateHandler self, IChannelHandlerContext context)
        {
            TimeSpan lastWriteTime = self._lastWriteTime;
            TimeSpan nextDelay = self._writerIdleTime - (self.Ticks() - lastWriteTime);

            if (nextDelay.Ticks <= 0)
            {
                // Writer is idle - set a new timeout and notify the callback.
                self._writerIdleTimeout = self.Schedule(context, WriteTimeoutAction, self, context,
                    self._writerIdleTime);

                bool first = self._firstWriterIdleEvent;
                self._firstWriterIdleEvent = false;

                try
                {
                    if (self.HasOutputChanged(context, first))
                    {
                        return;
                    }

                    IdleStateEvent stateEvent = self.NewIdleStateEvent(IdleState.WriterIdle, first);
                    self.ChannelIdle(context, stateEvent);
                }
                catch (Exception ex)
                {
                    _ = context.FireExceptionCaught(ex);
                }
            }
            else
            {
                // Write occurred before the timeout - set a new timeout with shorter delay.
                self._writerIdleTimeout = self.Schedule(context, WriteTimeoutAction, self, context, nextDelay);
            }
        }

        static void HandleAllTimeout(IdleStateHandler self, IChannelHandlerContext context)
        {
            TimeSpan nextDelay = self._allIdleTime;
            if (!self._reading)
            {
                nextDelay -= self.Ticks() - TimeUtil.Max(self._lastReadTime, self._lastWriteTime);
            }

            if (nextDelay.Ticks <= 0)
            {
                // Both reader and writer are idle - set a new timeout and
                // notify the callback.
                self._allIdleTimeout = self.Schedule(context, AllTimeoutAction, self, context,
                    self._allIdleTime);

                bool first = self._firstAllIdleEvent;
                self._firstAllIdleEvent = false;

                try
                {
                    if (self.HasOutputChanged(context, first))
                    {
                        return;
                    }

                    IdleStateEvent stateEvent = self.NewIdleStateEvent(IdleState.AllIdle, first);
                    self.ChannelIdle(context, stateEvent);
                }
                catch (Exception ex)
                {
                    _ = context.FireExceptionCaught(ex);
                }
            }
            else
            {
                // Either read or write occurred before the timeout - set a new
                // timeout with shorter delay.
                self._allIdleTimeout = self.Schedule(context, AllTimeoutAction, self, context, nextDelay);
            }
        }
    }
}

