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
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    public class EmbeddedChannel : AbstractChannel<EmbeddedChannel, EmbeddedChannel.WrappingEmbeddedUnsafe>
    {
        private static readonly EndPoint LOCAL_ADDRESS;
        private static readonly EndPoint REMOTE_ADDRESS;

        private static readonly IChannelHandler[] EMPTY_HANDLERS;

        private static readonly IInternalLogger s_logger;

        private static readonly ChannelMetadata METADATA_NO_DISCONNECT;
        private static readonly ChannelMetadata METADATA_DISCONNECT;

        static EmbeddedChannel()
        {
            LOCAL_ADDRESS = new EmbeddedSocketAddress();
            REMOTE_ADDRESS = new EmbeddedSocketAddress();
            EMPTY_HANDLERS = EmptyArray<IChannelHandler>.Instance;
            s_logger = InternalLoggerFactory.GetInstance<EmbeddedChannel>();
            METADATA_NO_DISCONNECT = new ChannelMetadata(false);
            METADATA_DISCONNECT = new ChannelMetadata(true);
        }

        private enum State
        {
            Open,
            Active,
            Closed
        };

        private readonly EmbeddedEventLoop _loop = new EmbeddedEventLoop();

        private readonly Deque<object> _inboundMessages = new Deque<object>();
        private readonly Deque<object> _outboundMessages = new Deque<object>();
        private Exception _lastException;
        private State _state;

        /// <summary>
        ///     Create a new instance with an empty pipeline.
        /// </summary>
        public EmbeddedChannel()
            : this(EmbeddedChannelId.Instance, EMPTY_HANDLERS)
        {
        }

        /// <summary>
        ///     Create a new instance with the pipeline initialized with the specified handlers.
        /// </summary>
        /// <param name="handlers">
        ///     The <see cref="IChannelHandler" />s that will be added to the <see cref="IChannelPipeline" />
        /// </param>
        public EmbeddedChannel(params IChannelHandler[] handlers)
            : this(EmbeddedChannelId.Instance, handlers)
        {
        }
        public EmbeddedChannel(bool hasDisconnect, params IChannelHandler[] handlers)
            : this(EmbeddedChannelId.Instance, hasDisconnect, handlers)
        {
        }

        public EmbeddedChannel(bool hasDisconnect, bool register, params IChannelHandler[] handlers)
            : this(EmbeddedChannelId.Instance, hasDisconnect, register, handlers)
        {
        }

        /// <summary>
        ///     Create a new instance with an empty pipeline with the specified <see cref="IChannelId" />.
        /// </summary>
        /// <param name="channelId">The <see cref="IChannelId" /> of this channel. </param>
        public EmbeddedChannel(IChannelId channelId)
            : this(channelId, EMPTY_HANDLERS)
        {
        }

        public EmbeddedChannel(IChannelId id, params IChannelHandler[] handlers)
            : this(id, false, handlers)
        {
        }

        /// <summary>Create a new instance with the pipeline initialized with the specified handlers.</summary>
        /// <param name="id">The <see cref="IChannelId" /> of this channel.</param>
        /// <param name="hasDisconnect">
        ///     <c>false</c> if this <see cref="IChannel" /> will delegate <see cref="DisconnectAsync()" />
        ///     to <see cref="CloseAsync()" />, <c>true</c> otherwise.
        /// </param>
        /// <param name="handlers">
        ///     The <see cref="IChannelHandler" />s that will be added to the <see cref="IChannelPipeline" />
        /// </param>
        public EmbeddedChannel(IChannelId id, bool hasDisconnect, params IChannelHandler[] handlers)
            : this(id, hasDisconnect, true, handlers)
        {
        }

        public EmbeddedChannel(IChannelId id, bool hasDisconnect, bool register, params IChannelHandler[] handlers)
            : this(null, id, hasDisconnect, register, handlers)
        {
        }

        public EmbeddedChannel(IChannel parent, IChannelId id, bool hasDisconnect, bool register, params IChannelHandler[] handlers)
            : base(parent, id)
        {
            Metadata = GetMetadata(hasDisconnect);
            Configuration = new DefaultChannelConfiguration(this);
            Setup(register, handlers);
        }

        public EmbeddedChannel(IChannelId id, bool hasDisconnect, IChannelConfiguration config, params IChannelHandler[] handlers)
            : base(null, id)
        {
            if (config is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.config); }

            Metadata = GetMetadata(hasDisconnect);
            Configuration = config;
            Setup(true, handlers);
        }

        static ChannelMetadata GetMetadata(bool hasDisconnect) => hasDisconnect ? METADATA_DISCONNECT : METADATA_NO_DISCONNECT;

        void Setup(bool register, params IChannelHandler[] handlers)
        {
            if (handlers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handlers); }

            IChannelPipeline p = Pipeline;
            p.AddLast(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                for (int i = 0; i < handlers.Length; i++)
                {
                    IChannelHandler h = handlers[i];
                    if (h is null) { break; }
                    pipeline.AddLast(h);
                }
            }));

            if (register)
            {
                Task future = _loop.RegisterAsync(this);
                Debug.Assert(future.IsCompleted);
            }
        }

        public void Register()
        {
            Task future = _loop.RegisterAsync(this);
            Debug.Assert(future.IsCompleted);
            if (future.IsFailure())
            {
                throw future.Exception.InnerException;
            }
            Pipeline.AddLast(new LastInboundHandler(this));
        }

        protected sealed override DefaultChannelPipeline NewChannelPipeline() => new EmbeddedChannelPipeline(this);

        public override ChannelMetadata Metadata { get; }

        public override IChannelConfiguration Configuration { get; }

        /// <summary>
        ///     Returns the <see cref="Queue{T}" /> which holds all of the <see cref="object" />s that
        ///     were received by this <see cref="IChannel" />.
        /// </summary>
        public Deque<object> InboundMessages => _inboundMessages;

        /// <summary>
        ///     Returns the <see cref="Queue{T}" /> which holds all of the <see cref="object" />s that
        ///     were written by this <see cref="IChannel" />.
        /// </summary>
        public Deque<object> OutboundMessages => _outboundMessages;

        /// <summary>
        /// Return received data from this <see cref="IChannel"/>.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public T ReadInbound<T>() => (T)ReadInbound();

        /// <summary>
        /// Return received data from this <see cref="IChannel"/>.
        /// </summary>
        public object ReadInbound()
        {
#if DEBUG
            var message = Poll(_inboundMessages);
            if (message is object)
            {
                _ = ReferenceCountUtil.Touch(message, "Caller of readInbound() will handle the message from this point");
            }
            return message;
#else
            return Poll(_inboundMessages);
#endif
        }

        /// <summary>
        /// Read data from the outbound. This may return <c>null</c> if nothing is readable.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public T ReadOutbound<T>() => (T)ReadOutbound();

        /// <summary>
        /// Read data from the outbound. This may return <c>null</c> if nothing is readable.
        /// </summary>
        public object ReadOutbound()
        {
#if DEBUG
            var message = Poll(_outboundMessages);
            if (message is object)
            {
                _ = ReferenceCountUtil.Touch(message, "Caller of readOutbound() will handle the message from this point.");
            }
            return message;
#else
            return Poll(_outboundMessages);
#endif
        }

        protected override EndPoint LocalAddressInternal => IsActive ? LOCAL_ADDRESS : null;

        protected override EndPoint RemoteAddressInternal => IsActive ? REMOTE_ADDRESS : null;

        protected override bool IsCompatible(IEventLoop eventLoop) => eventLoop is EmbeddedEventLoop;

        protected override void DoBind(EndPoint localAddress)
        {
            //NOOP
        }

        protected override void DoRegister() => _state = State.Active;

        protected override void DoDisconnect()
        {
            if (!Metadata.HasDisconnect) { DoClose(); }
        }

        protected override void DoClose() => _state = State.Closed;

        protected override void DoBeginRead()
        {
            //NOOP
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            while (true)
            {
                object msg = input.Current;
                if (msg is null)
                {
                    break;
                }

                ReferenceCountUtil.Retain(msg);
                HandleOutboundMessage(msg);
                input.Remove();
            }
        }

        public override bool IsOpen => _state != State.Closed;

        public override bool IsActive => _state == State.Active;

        /// <summary>
        ///     Run all tasks (which also includes scheduled tasks) that are pending in the <see cref="IEventLoop" />
        ///     for this <see cref="IChannel" />.
        /// </summary>
        public void RunPendingTasks()
        {
            try
            {
                _loop.RunTasks();
            }
            catch (Exception ex)
            {
                RecordException(ex);
            }

            try
            {
                _ = _loop.RunScheduledTasks();
            }
            catch (Exception ex)
            {
                RecordException(ex);
            }
        }

        /// <summary>
        ///     Run all pending scheduled tasks in the <see cref="IEventLoop" /> for this <see cref="IChannel" />.
        /// </summary>
        /// <returns>
        ///     The nanoseconds when the next scheduled task is ready to run. If no other task is
        ///     scheduled then it will return <see cref="PreciseTime.MinusOne" />.
        /// </returns>
        public long RunScheduledPendingTasks()
        {
            try
            {
                return _loop.RunScheduledTasks();
            }
            catch (Exception ex)
            {
                RecordException(ex);
                return _loop.NextScheduledTask();
            }
        }

        /// <summary>
        ///     Write messages to the inbound of this <see cref="IChannel" />
        /// </summary>
        /// <param name="msgs">The messages to be written.</param>
        /// <returns><c>true</c> if the write operation did add something to the inbound buffer</returns>
        public bool WriteInbound(params object[] msgs)
        {
            EnsureOpen();
            if (0u >= (uint)msgs.Length)
            {
                return _inboundMessages.NonEmpty;
            }

            IChannelPipeline p = Pipeline;
            for (int i = 0; i < msgs.Length; i++)
            {
                _ = p.FireChannelRead(msgs[i]);
            }

            FlushInbound(false, VoidPromise());
            return _inboundMessages.NonEmpty;
        }

        public Task WriteOneInbound(object msg) => WriteOneInbound(msg, NewPromise());

        public Task WriteOneInbound(object msg, IPromise promise)
        {
            if (CheckOpen(true))
            {
                _ = Pipeline.FireChannelRead(msg);
            }
            CheckException(promise);
            return promise.Task;
        }

        /// <summary>Flushes the inbound of this <see cref="IChannel"/>. This method is conceptually equivalent to Flush.</summary>
        public EmbeddedChannel FlushInbound()
        {
            FlushInbound(true, VoidPromise());
            return this;
        }

        /// <summary>Flushes the inbound of this <see cref="IChannel"/>. This method is conceptually equivalent to Flush.</summary>
        /// <returns></returns>
        public void FlushInbound(bool recordException, IPromise promise)
        {
            if (CheckOpen(recordException))
            {
                Pipeline.FireChannelReadComplete();
                RunPendingTasks();
            }

            CheckException(promise);
        }

        /// <summary>
        ///     Write messages to the outbound of this <see cref="IChannel" />.
        /// </summary>
        /// <param name="msgs">The messages to be written.</param>
        /// <returns><c>true</c> if the write operation did add something to the inbound buffer</returns>
        public bool WriteOutbound(params object[] msgs)
        {
            EnsureOpen();
            if (0u >= (uint)msgs.Length)
            {
                return _outboundMessages.NonEmpty;
            }

            ThreadLocalObjectList futures = ThreadLocalObjectList.NewInstance(msgs.Length);

            try
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    object m = msgs[i];
                    if (m is null)
                    {
                        break;
                    }
                    futures.Add(WriteAsync(m));
                }

                FlushOutbound0();

                int size = futures.Count;
                for (int i = 0; i < size; i++)
                {
                    var future = (Task)futures[i];
                    if (future.IsCompleted)
                    {
                        RecordException(future);
                    }
                    else
                    {
                        // The write may be delayed to run later by RunPendingTasks()
                        future.ContinueWith(t => RecordException(t), TaskContinuationOptions.ExecuteSynchronously);
                    }
                }

                CheckException();
                return _outboundMessages.NonEmpty;
            }
            finally
            {
                futures.Return();
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        void RecordException(Task future)
        {
            if (future.IsCanceled || future.IsFaulted)
            {
                RecordException(future.Exception);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void RecordException(Exception cause)
        {
            if (_lastException is null)
            {
                _lastException = cause;
            }
            else
            {
                s_logger.Warn("More than one exception was raised. " + "Will report only the first one and log others.", cause);
            }
        }

        /// <summary>
        /// Writes one message to the outbound of this <see cref="IChannel"/> and does not flush it. This
        /// method is conceptually equivalent to WriteAsync.
        /// </summary>
        public Task WriteOneOutbound(object msg) => WriteOneOutbound(msg, NewPromise());

        public Task WriteOneOutbound(object msg, IPromise promise)
        {
            if (CheckOpen(true))
            {
                return WriteAsync(msg, promise);
            }
            CheckException(promise);
            return promise.Task;
        }

        /// <summary>Flushes the outbound of this <see cref="IChannel"/>.
        /// This method is conceptually equivalent to <see cref="Finish()"/>.</summary>
        /// <returns></returns>
        public EmbeddedChannel FlushOutbound()
        {
            if (CheckOpen(true))
            {
                FlushOutbound0();
            }
            CheckException(VoidPromise());
            return this;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        void FlushOutbound0()
        {
            // We need to call RunPendingTasks first as a IChannelHandler may have used IEventLoop.Execute(...) to
            // delay the write on the next event loop run.
            RunPendingTasks();

            Flush();
        }

        /// <summary>
        ///     Mark this <see cref="IChannel" /> as finished. Any further try to write data to it will fail.
        /// </summary>
        /// <returns>bufferReadable returns <c>true</c></returns>
        public bool Finish() => Finish(false);

        /// <summary>
        /// Marks this <see cref="IChannel"/> as finished and releases all pending message in the inbound and outbound
        /// buffer. Any futher try to write data to it will fail.
        /// </summary>
        /// <returns><c>true</c> if any of the used buffers has something left to read, otherwise <c>false</c>.</returns>
        public bool FinishAndReleaseAll() => Finish(true);

        /// <summary>
        /// Marks this <see cref="IChannel"/> as finished. Any futher attempt to write data to it will fail.
        /// </summary>
        /// <param name="releaseAll">If <c>true</c>, all pending messages in the inbound and outbound buffer are released.</param>
        /// <returns><c>true</c> if any of the used buffers has something left to read, otherwise <c>false</c>.</returns>
        bool Finish(bool releaseAll)
        {
            this.CloseSafe();
            try
            {
                CheckException();
                return _inboundMessages.NonEmpty || _outboundMessages.NonEmpty;
            }
            finally
            {
                if (releaseAll)
                {
                    _ = ReleaseAll(_inboundMessages);
                    _ = ReleaseAll(_outboundMessages);
                }
            }
        }

        /// <summary>
        /// Releases all buffered inbound messages.
        /// </summary>
        /// <returns><c>true</c> if any were in the inbound buffer, otherwise <c>false</c>.</returns>
        public bool ReleaseInbound() => ReleaseAll(_inboundMessages);

        /// <summary>
        /// Releases all buffered outbound messages.
        /// </summary>
        /// <returns><c>true</c> if any were in the outbound buffer, otherwise <c>false</c>.</returns>
        public bool ReleaseOutbound() => ReleaseAll(_outboundMessages);

        static bool ReleaseAll(Deque<object> queue)
        {
            if (queue.IsEmpty) { return false; }

            while (queue.TryRemoveFirst(out var msg))
            {
                _ = ReferenceCountUtil.Release(msg);
            }
            return true;
        }

        void FinishPendingTasks(bool cancel)
        {
            RunPendingTasks();
            if (cancel)
            {
                // Cancel all scheduled tasks that are left.
                _loop.CancelScheduledTasks();
            }
        }

        public override Task CloseAsync()
        {
            return CloseAsync(NewPromise());
        }

        public override Task CloseAsync(IPromise promise)
        {
            // We need to call RunPendingTasks() before calling super.CloseAsync() as there may be something in the queue
            // that needs to be run before the actual close takes place.
            RunPendingTasks();
            Task future = base.CloseAsync(promise);

            // Now finish everything else and cancel all scheduled tasks that were not ready set.
            FinishPendingTasks(true);
            return future;
        }

        public override Task DisconnectAsync()
        {
            return DisconnectAsync(NewPromise());
        }

        public override Task DisconnectAsync(IPromise promise)
        {
            Task future = base.DisconnectAsync(promise);
            FinishPendingTasks(!Metadata.HasDisconnect);
            return future;
        }

        /// <summary>
        ///     Check to see if there was any <see cref="Exception" /> and rethrow if so.
        /// </summary>
        public void CheckException(IPromise promise)
        {
            Exception e = _lastException;
            if (e is null)
            {
                promise.TryComplete();
                return;
            }

            _lastException = null;
            if (promise.IsVoid)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            promise.TrySetException(e);
        }

        public void CheckException()
        {
            CheckException(VoidPromise());
        }

        /// <summary>Returns <c>true</c> if the <see cref="IChannel" /> is open and records optionally
        /// an <see cref="Exception" /> if it isn't.</summary>
        /// <param name="recordException"></param>
        /// <returns></returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        bool CheckOpen(bool recordException)
        {
            if (!IsOpen)
            {
                if (recordException)
                {
                    RecordException(ThrowHelper.GetClosedChannelException());
                }
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Ensure the <see cref="IChannel" /> is open and if not throw an exception.
        /// </summary>
        protected void EnsureOpen()
        {
            if (!CheckOpen(true))
            {
                CheckException();
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        static object Poll(Deque<object> queue)
        {
            return queue.TryRemoveFirst(out var result) ? result : null;
        }

        /// <summary>Called for each outbound message.</summary>
        /// <param name="msg"></param>
        protected virtual void HandleOutboundMessage(object msg)
        {
            _outboundMessages.AddLast​(msg);
        }

        /// <summary>Called for each inbound message.</summary>
        /// <param name="msg"></param>
        protected virtual void HandleInboundMessage(object msg)
        {
            _inboundMessages.AddLast​(msg);
        }

        protected override WrappingEmbeddedUnsafe NewUnsafe()
        {
            var @unsafe = new WrappingEmbeddedUnsafe();
            @unsafe.Initialize(this);
            return @unsafe;
        }

        public sealed class EmbeddedUnsafe : AbstractUnsafe
        {
            public EmbeddedUnsafe() //AbstractChannel channel)
                : base() //channel)
            {
            }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress) => TaskUtil.Completed;
        }

        public sealed class WrappingEmbeddedUnsafe : IChannelUnsafe
        {
            private EmbeddedUnsafe _innerUnsafe;
            private EmbeddedChannel _embeddedChannel;

            public WrappingEmbeddedUnsafe() { }

            public void Initialize(IChannel channel)
            {
                _embeddedChannel = (EmbeddedChannel)channel;
                _innerUnsafe = new EmbeddedUnsafe();
                _innerUnsafe.Initialize(_embeddedChannel);
            }

            public IRecvByteBufAllocatorHandle RecvBufAllocHandle => _innerUnsafe.RecvBufAllocHandle;

            public ChannelOutboundBuffer OutboundBuffer => _innerUnsafe.OutboundBuffer;

            public void BeginRead()
            {
                _innerUnsafe.BeginRead();
                _embeddedChannel.RunPendingTasks();
            }

            public async Task RegisterAsync(IEventLoop eventLoop)
            {
                await _innerUnsafe.RegisterAsync(eventLoop);
                _embeddedChannel.RunPendingTasks();
            }

            public async Task BindAsync(EndPoint localAddress)
            {
                await _innerUnsafe.BindAsync(localAddress);
                _embeddedChannel.RunPendingTasks();
            }

            public async Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                await _innerUnsafe.ConnectAsync(remoteAddress, localAddress);
                _embeddedChannel.RunPendingTasks();
            }

            public void Disconnect(IPromise promise)
            {
                _innerUnsafe.Disconnect(promise);
                _embeddedChannel.RunPendingTasks();
            }

            public void Close(IPromise promise)
            {
                _innerUnsafe.Close(promise);
                _embeddedChannel.RunPendingTasks();
            }

            public void CloseForcibly()
            {
                _innerUnsafe.CloseForcibly();
                _embeddedChannel.RunPendingTasks();
            }

            public void Deregister(IPromise promise)
            {
                _innerUnsafe.Deregister(promise);
                _embeddedChannel.RunPendingTasks();
            }

            public void Write(object message, IPromise promise)
            {
                _innerUnsafe.Write(message, promise);
                _embeddedChannel.RunPendingTasks();
            }

            public void Flush()
            {
                _innerUnsafe.Flush();
                _embeddedChannel.RunPendingTasks();
            }

            public IPromise VoidPromise()
            {
                return _innerUnsafe.VoidPromise();
            }
        }

        internal sealed class LastInboundHandler : ChannelHandlerAdapter
        {
            readonly EmbeddedChannel _embeddedChannel;

            public LastInboundHandler(EmbeddedChannel channel)
            {
                _embeddedChannel = channel;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message) => _embeddedChannel.HandleInboundMessage(message);

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => _embeddedChannel.RecordException(exception);
        }

        sealed class EmbeddedChannelPipeline : DefaultChannelPipeline
        {
            readonly EmbeddedChannel _embeddedChannel;

            public EmbeddedChannelPipeline(EmbeddedChannel channel)
                : base(channel)
            {
                _embeddedChannel = channel;
            }

            protected override void OnUnhandledInboundException(Exception cause) => _embeddedChannel.RecordException(cause);

            protected override void OnUnhandledInboundMessage(IChannelHandlerContext ctx, object msg) => _embeddedChannel.HandleInboundMessage(msg);
        }
    }
}