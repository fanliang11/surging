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
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    abstract partial class AbstractChannelHandlerContext : IChannelHandlerContext, IResourceLeakHint
    {
        private AbstractChannelHandlerContext v_next;
        internal AbstractChannelHandlerContext Next
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Volatile.Read(ref v_next);
            set => Interlocked.Exchange(ref v_next, value);
        }

        private AbstractChannelHandlerContext v_prev;
        internal AbstractChannelHandlerContext Prev
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Volatile.Read(ref v_prev);
            set => Interlocked.Exchange(ref v_prev, value);
        }

        private readonly int _executionMask;

        internal readonly DefaultChannelPipeline _pipeline;
        private readonly bool _ordered;

        // Will be set to null if no child executor should be used, otherwise it will be set to the
        // child executor.
        internal readonly IEventExecutor _executor;

        // Lazily instantiated tasks used to trigger events to a handler with different executor.
        // There is no need to make this volatile as at worse it will just create a few more instances then needed.
        private ContextTasks _invokeTasks;
        private ContextTasks InvokeTasks
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _invokeTasks ?? EnsureTastsCreated();
        }

        private int v_handlerState = HandlerState.Init;

        protected AbstractChannelHandlerContext(DefaultChannelPipeline pipeline, IEventExecutor executor,
            string name, int executionMask)
        {
            if (pipeline is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.pipeline); }
            if (name is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.name); }

            _pipeline = pipeline;
            Name = name;
            _executor = executor;
            _executionMask = executionMask;
            // Its ordered if its driven by the EventLoop or the given Executor is an instanceof OrderedEventExecutor.
            _ordered = executor is null || executor is IOrderedEventExecutor;
        }

        public IChannel Channel => _pipeline.Channel;

        public IChannelPipeline Pipeline => _pipeline;

        public IByteBufferAllocator Allocator => Channel.Allocator;

        public abstract IChannelHandler Handler { get; }

        /// <summary>
        ///     Makes best possible effort to detect if <see cref="IChannelHandler.HandlerAdded(IChannelHandlerContext)" /> was
        ///     called
        ///     yet. If not return <c>false</c> and if called or could not detect return <c>true</c>.
        ///     If this method returns <c>true</c> we will not invoke the <see cref="IChannelHandler" /> but just forward the
        ///     event.
        ///     This is needed as <see cref="DefaultChannelPipeline" /> may already put the <see cref="IChannelHandler" /> in the
        ///     linked-list
        ///     but not called <see cref="IChannelHandler.HandlerAdded(IChannelHandlerContext)" />
        /// </summary>
        private bool InvokeHandler
        {
            [MethodImpl(InlineMethod.AggressiveInlining)]
            get
            {
                // Store in local variable to reduce volatile reads.
                var thisState = Volatile.Read(ref v_handlerState);
                return 0u >= (uint)(thisState - HandlerState.AddComplete) ||
                    (!_ordered && 0u >= (uint)(thisState - HandlerState.AddPending));
            }
        }

        [Obsolete("Please use IsRemoved instead.")]
        public bool Removed => IsRemoved;

        public bool IsRemoved => 0u >= (uint)(Volatile.Read(ref v_handlerState) - HandlerState.RemoveComplete);

        internal bool SetAddComplete()
        {
            var prevState = Volatile.Read(ref v_handlerState);
            int oldState;
            do
            {
                if (0u >= (uint)(prevState - HandlerState.RemoveComplete)) { return false; }
                oldState = prevState;
                // Ensure we never update when the handlerState is REMOVE_COMPLETE already.
                // oldState is usually ADD_PENDING but can also be REMOVE_COMPLETE when an EventExecutor is used that is not
                // exposing ordering guarantees.
                prevState = Interlocked.CompareExchange(ref v_handlerState, HandlerState.AddComplete, prevState);
            } while ((uint)(prevState - oldState) > 0u);
            return true;
        }

        internal void SetRemoved() => Interlocked.Exchange(ref v_handlerState, HandlerState.RemoveComplete);

        internal void SetAddPending()
        {
            var updated = HandlerState.Init == Interlocked.CompareExchange(ref v_handlerState, HandlerState.AddPending, HandlerState.Init);
            Debug.Assert(updated); // This should always be true as it MUST be called before setAddComplete() or setRemoved().
        }

        internal void CallHandlerAdded()
        {
            // We must call setAddComplete before calling handlerAdded. Otherwise if the handlerAdded method generates
            // any pipeline events ctx.handler() will miss them because the state will not allow it.
            if (SetAddComplete())
            {
                Handler.HandlerAdded(this);
            }
        }

        internal void CallHandlerRemoved()
        {
            try
            {
                // Only call handlerRemoved(...) if we called handlerAdded(...) before.
                if (Volatile.Read(ref v_handlerState) == HandlerState.AddComplete)
                {
                    Handler.HandlerRemoved(this);
                }
            }
            finally
            {
                // Mark the handler as removed in any case.
                SetRemoved();
            }
        }

        public IEventExecutor Executor => _executor ?? Channel.EventLoop;

        public string Name { get; }

        public IAttribute<T> GetAttribute<T>(AttributeKey<T> key)
            where T : class
        {
            return Channel.GetAttribute(key);
        }

        public bool HasAttribute<T>(AttributeKey<T> key)
            where T : class
        {
            return Channel.HasAttribute(key);
        }
        public IChannelHandlerContext FireChannelRegistered()
        {
            InvokeChannelRegistered(FindContextInbound(SkipFlags.ChannelRegistered));
            return this;
        }

        internal static void InvokeChannelRegistered(AbstractChannelHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelRegistered();
            }
            else
            {
                nextExecutor.Execute(InvokeChannelRegisteredAction, next);
            }
        }

        void InvokeChannelRegistered()
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.ChannelRegistered(this);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireChannelRegistered();
            }
        }

        public IChannelHandlerContext FireChannelUnregistered()
        {
            InvokeChannelUnregistered(FindContextInbound(SkipFlags.ChannelUnregistered));
            return this;
        }

        internal static void InvokeChannelUnregistered(AbstractChannelHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelUnregistered();
            }
            else
            {
                nextExecutor.Execute(InvokeChannelUnregisteredAction, next);
            }
        }

        void InvokeChannelUnregistered()
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.ChannelUnregistered(this);
                }
                catch (Exception t)
                {
                    InvokeExceptionCaught(t);
                }
            }
            else
            {
                _ = FireChannelUnregistered();
            }
        }

        public IChannelHandlerContext FireChannelActive()
        {
            InvokeChannelActive(FindContextInbound(SkipFlags.ChannelActive));
            return this;
        }

        internal static void InvokeChannelActive(AbstractChannelHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelActive();
            }
            else
            {
                nextExecutor.Execute(InvokeChannelActiveAction, next);
            }
        }

        void InvokeChannelActive()
        {
            if (InvokeHandler)
            {
                try
                {
                    (Handler).ChannelActive(this);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireChannelActive();
            }
        }

        public IChannelHandlerContext FireChannelInactive()
        {
            InvokeChannelInactive(FindContextInbound(SkipFlags.ChannelInactive));
            return this;
        }

        internal static void InvokeChannelInactive(AbstractChannelHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelInactive();
            }
            else
            {
                nextExecutor.Execute(InvokeChannelInactiveAction, next);
            }
        }

        void InvokeChannelInactive()
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.ChannelInactive(this);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireChannelInactive();
            }
        }

        public virtual IChannelHandlerContext FireExceptionCaught(Exception cause)
        {
            InvokeExceptionCaught(FindContextInbound(SkipFlags.ExceptionCaught), cause);
            return this;
        }

        internal static void InvokeExceptionCaught(AbstractChannelHandlerContext next, Exception cause)
        {
            if (cause is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.cause); }

            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeExceptionCaught(cause);
            }
            else
            {
                try
                {
                    nextExecutor.Execute(InvokeExceptionCaughtAction, next, cause);
                }
                catch (Exception t)
                {
                    var logger = DefaultChannelPipeline.Logger;
                    if (logger.WarnEnabled)
                    {
                        logger.FailedToSubmitAnExceptionCaughtEvent(t);
                        logger.TheExceptionCaughtEventThatWasFailedToSubmit(cause);
                    }
                }
            }
        }

        void InvokeExceptionCaught(Exception cause)
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.ExceptionCaught(this, cause);
                }
                catch (Exception t)
                {
                    var logger = DefaultChannelPipeline.Logger;
                    if (logger.WarnEnabled)
                    {
                        logger.FailedToSubmitAnExceptionCaughtEvent(t);
                        logger.ExceptionCaughtMethodWhileHandlingTheFollowingException(cause);
                    }
                }
            }
            else
            {
                _ = FireExceptionCaught(cause);
            }
        }

        public IChannelHandlerContext FireUserEventTriggered(object evt)
        {
            InvokeUserEventTriggered(FindContextInbound(SkipFlags.UserEventTriggered), evt);
            return this;
        }

        internal static void InvokeUserEventTriggered(AbstractChannelHandlerContext next, object evt)
        {
            if (evt is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.evt); }
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeUserEventTriggered(evt);
            }
            else
            {
                nextExecutor.Execute(InvokeUserEventTriggeredAction, next, evt);
            }
        }

        void InvokeUserEventTriggered(object evt)
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.UserEventTriggered(this, evt);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireUserEventTriggered(evt);
            }
        }

        public IChannelHandlerContext FireChannelRead(object msg)
        {
            InvokeChannelRead(FindContextInbound(SkipFlags.ChannelRead), msg);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvokeChannelRead(AbstractChannelHandlerContext next, object msg)
        {
            if (msg is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.msg); }

            object m = next._pipeline.Touch(msg, next);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelRead(m);
            }
            else
            {
                nextExecutor.Execute(InvokeChannelReadAction, next, msg);
            }
        }

        void InvokeChannelRead(object msg)
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.ChannelRead(this, msg);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireChannelRead(msg);
            }
        }

        public IChannelHandlerContext FireChannelReadComplete()
        {
            InvokeChannelReadComplete(FindContextInbound(SkipFlags.ChannelReadComplete));
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvokeChannelReadComplete(AbstractChannelHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelReadComplete();
            }
            else
            {
                var tasks = next.InvokeTasks;
                nextExecutor.Execute(tasks.InvokeChannelReadCompleteTask);
            }
        }

        void InvokeChannelReadComplete()
        {
            if (InvokeHandler)
            {
                try
                {
                    (Handler).ChannelReadComplete(this);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireChannelReadComplete();
            }
        }

        public IChannelHandlerContext FireChannelWritabilityChanged()
        {
            InvokeChannelWritabilityChanged(FindContextInbound(SkipFlags.ChannelWritabilityChanged));
            return this;
        }

        internal static void InvokeChannelWritabilityChanged(AbstractChannelHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelWritabilityChanged();
            }
            else
            {
                var tasks = next.InvokeTasks;
                nextExecutor.Execute(tasks.InvokeChannelWritableStateChangedTask);
            }
        }

        void InvokeChannelWritabilityChanged()
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.ChannelWritabilityChanged(this);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = FireChannelWritabilityChanged();
            }
        }

        public Task BindAsync(EndPoint localAddress)
        {
            if (localAddress is null) { return ThrowHelper.FromArgumentNullException(DotNetty.Transport.ExceptionArgument.localAddress); }
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Bind);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                return next.InvokeBindAsync(localAddress);
            }
            else
            {
                var promise = nextExecutor.NewPromise();
                _ = SafeExecuteOutbound(nextExecutor, new BindTask(next, promise, localAddress), promise, null, false);
                return promise.Task;
            }
        }

        Task InvokeBindAsync(EndPoint localAddress)
        {
            if (InvokeHandler)
            {
                try
                {
                    return Handler.BindAsync(this, localAddress);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }

            return BindAsync(localAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress) => ConnectAsync(remoteAddress, null);

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Connect);
            if (remoteAddress is null) { return ThrowHelper.FromArgumentNullException(DotNetty.Transport.ExceptionArgument.remoteAddress); }
            // todo: check for cancellation

            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                return next.InvokeConnectAsync(remoteAddress, localAddress);
            }
            else
            {
                var promise = nextExecutor.NewPromise();
                _ = SafeExecuteOutbound(nextExecutor, new ConnectTask(next, promise, remoteAddress, localAddress), promise, null, false);
                return promise.Task;

            }
        }

        Task InvokeConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            if (InvokeHandler)
            {
                try
                {
                    return Handler.ConnectAsync(this, remoteAddress, localAddress);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }

            return ConnectAsync(remoteAddress, localAddress);
        }

        public Task DisconnectAsync() => DisconnectAsync(NewPromise());

        public Task DisconnectAsync(IPromise promise)
        {
            if (!Channel.Metadata.HasDisconnect)
            {
                // Translate disconnect to close if the channel has no notion of disconnect-reconnect.
                // So far, UDP/IP is the only transport that has such behavior.
                return CloseAsync(promise);
            }
            if (IsNotValidPromise(promise, false))
            {
                // cancelled
                return promise.Task;
            }

            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Disconnect);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeDisconnect(promise);
            }
            else
            {
                _ = SafeExecuteOutbound(nextExecutor, new DisconnectTask(next, promise), promise, null, false);
            }

            return promise.Task;
        }

        void InvokeDisconnect(IPromise promise)
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.Disconnect(this, promise);
                }
                catch (Exception ex)
                {
                    NotifyOutboundHandlerException(ex, promise);
                }
            }
            else
            {
                _ = DisconnectAsync(promise);
            }
        }

        public Task CloseAsync() => CloseAsync(NewPromise());

        public Task CloseAsync(IPromise promise)
        {
            if (IsNotValidPromise(promise, false))
            {
                // cancelled
                return promise.Task;
            }

            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Close);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeClose(promise);
            }
            else
            {
                _ = SafeExecuteOutbound(nextExecutor, new CloseTask(next, promise), promise, null, false);
            }

            return promise.Task;
        }

        void InvokeClose(IPromise promise)
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.Close(this, promise);
                }
                catch (Exception ex)
                {
                    NotifyOutboundHandlerException(ex, promise);
                }
            }
            else
            {
                _ = CloseAsync(promise);
            }
        }

        public Task DeregisterAsync() => DeregisterAsync(NewPromise());

        public Task DeregisterAsync(IPromise promise)
        {
            if (IsNotValidPromise(promise, false))
            {
                // cancelled
                return promise.Task;
            }

            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Deregister);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeDeregister(promise);
            }
            else
            {
                _ = SafeExecuteOutbound(nextExecutor, new DeregisterTask(next, promise), promise, null, false);
            }

            return promise.Task;
        }

        void InvokeDeregister(IPromise promise)
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.Deregister(this, promise);
                }
                catch (Exception ex)
                {
                    NotifyOutboundHandlerException(ex, promise);
                }
            }
            else
            {
                _ = DeregisterAsync(promise);
            }
        }
         
        public IChannelHandlerContext Read()
        {
            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Read);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeRead();
            }
            else
            {
                var tasks = next.InvokeTasks;
                nextExecutor.Execute(tasks.InvokeReadTask);
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InvokeRead()
        {
            if (InvokeHandler)
            {
                try
                {
                    Handler.Read(this);
                }
                catch (Exception ex)
                {
                    InvokeExceptionCaught(ex);
                }
            }
            else
            {
                _ = Read();
            }
        }

        public Task WriteAsync(object msg) => WriteAsync(msg, NewPromise());

        public Task WriteAsync(object msg, IPromise promise)
        {
            Write(msg, false, promise);
            return promise.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InvokeWrite(object msg, IPromise promise)
        {
            if (InvokeHandler)
            {
                InvokeWrite0(msg, promise);
            }
            else
            {
                _ = WriteAsync(msg, promise);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InvokeWrite0(object msg, IPromise promise)
        {
            try
            {
                Handler.Write(this, msg, promise);
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, promise);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChannelHandlerContext Flush()
        {
            AbstractChannelHandlerContext next = FindContextOutbound(SkipFlags.Flush);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeFlush();
            }
            else
            {
                var tasks = next.InvokeTasks;
                _ = SafeExecuteOutbound(nextExecutor, tasks.InvokeFlushTask, VoidPromise(), null, false);
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InvokeFlush()
        {
            if (InvokeHandler)
            {
                InvokeFlush0();
            }
            else
            {
                _ = Flush();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InvokeFlush0()
        {
            try
            {
                Handler.Flush(this);
            }
            catch (Exception ex)
            {
                InvokeExceptionCaught(ex);
            }
        }

        public Task WriteAndFlushAsync(object message) => WriteAndFlushAsync(message, NewPromise());

        public Task WriteAndFlushAsync(object message, IPromise promise)
        {
            Write(message, true, promise);
            return promise.Task;
        }

        void InvokeWriteAndFlush(object msg, IPromise promise)
        {
            if (InvokeHandler)
            {
                InvokeWrite0(msg, promise);
                InvokeFlush0();
            }
            else
            {
                _ = WriteAndFlushAsync(msg, promise);
            }
        }

        void Write(object msg, bool flush, IPromise promise)
        {
            if (msg is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.msg); }

            try
            {
                if (IsNotValidPromise(promise, true))
                {
                    _ = ReferenceCountUtil.Release(msg);
                    // cancelled
                    return;
                }
            }
            catch (Exception)
            {
                _ = ReferenceCountUtil.Release(msg);
                throw;
            }

            AbstractChannelHandlerContext next = FindContextOutbound(
                flush ? SkipFlags.WriteAndFlush : SkipFlags.Write);
            object m = _pipeline.Touch(msg, next);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                if (flush)
                {
                    next.InvokeWriteAndFlush(m, promise);
                }
                else
                {
                    next.InvokeWrite(m, promise);
                }
            }
            else
            {
                var task = WriteTask.NewInstance(next, m, promise, flush);
                if (!SafeExecuteOutbound(nextExecutor, task, promise, msg, !flush))
                {
                    // We failed to submit the WriteTask. We need to cancel it so we decrement the pending bytes
                    // and put it back in the Recycler for re-use later.
                    //
                    // See https://github.com/netty/netty/issues/8343.
                    task.Cancel();
                }
            }
        }

        private static void NotifyOutboundHandlerException(Exception cause, IPromise promise)
        {
            // Only log if the given promise is not of type VoidChannelPromise as tryFailure(...) is expected to return
            // false.
            PromiseNotificationUtil.TryFailure(promise, cause, promise.IsVoid ? null : DefaultChannelPipeline.Logger);
        }

        public IPromise NewPromise() => new DefaultPromise();//fanly update

        public IPromise NewPromise(object state) => new DefaultPromise(state);

        public IPromise VoidPromise() => Channel.VoidPromise();

        static Task ComposeExceptionTask(Exception cause) => TaskUtil.FromException(cause);

        AbstractChannelHandlerContext FindContextInbound(int mask)
        {
            AbstractChannelHandlerContext ctx = this;
            IEventExecutor currentExecutor = Executor;
            do
            {
                ctx = ctx.Next;
            }
            while (SkipContext(ctx, currentExecutor, mask, SkipFlags.OnlyInbound));
            return ctx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        AbstractChannelHandlerContext FindContextOutbound(int mask)
        {
            AbstractChannelHandlerContext ctx = this;
            IEventExecutor currentExecutor = Executor;
            do
            {
                ctx = ctx.Prev;
            }
            while (SkipContext(ctx, currentExecutor, mask, SkipFlags.OnlyOutbound));
            return ctx;
        }

        private static bool SkipContext(
                AbstractChannelHandlerContext ctx, IEventExecutor currentExecutor, int mask, int onlyMask)
        {
            // Ensure we correctly handle MASK_EXCEPTION_CAUGHT which is not included in the MASK_EXCEPTION_CAUGHT
            return (0u >= (uint)(ctx._executionMask & (onlyMask | mask))) ||
                    // We can only skip if the EventExecutor is the same as otherwise we need to ensure we offload
                    // everything to preserve ordering.
                    //
                    // See https://github.com/netty/netty/issues/10067
                    (ctx.Executor == currentExecutor && 0u >= (uint)(ctx._executionMask & mask));
        }

        static bool SafeExecuteOutbound(IEventExecutor executor, IRunnable task,
            IPromise promise, object msg, bool lazy)
        {
            try
            {
                if (lazy && executor is AbstractEventExecutor eventExecutor)
                {
                    eventExecutor.LazyExecute(task);
                }
                else
                {
                    executor.Execute(task);
                }
                return true;
            }
            catch (Exception cause)
            {
                try
                {
                    _ = promise.TrySetException(cause);
                }
                finally
                {
                    _ = ReferenceCountUtil.Release(msg);
                }
                return false;
            }
        }

        public string ToHintString() => $"\'{Name}\' will handle the message from this point.";

        public override string ToString() => $"{typeof(IChannelHandlerContext).Name} ({Name}, {Channel})";

        static bool IsNotValidPromise(IPromise promise, bool allowVoidPromise)
        {
            if (promise is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.promise); }

            if (promise.IsCompleted)
            {
                // Check if the promise was cancelled and if so signal that the processing of the operation
                // should not be performed.
                //
                // See https://github.com/netty/netty/issues/2349
                if (promise.IsCanceled) { return true; }

                ThrowHelper.ThrowArgumentException_PromiseAlreadyCompleted(promise);
            }

            if (!allowVoidPromise && promise.IsVoid)
            {
                ThrowHelper.ThrowArgumentException_VoidPromiseIsNotAllowed();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ContextTasks EnsureTastsCreated()
        {
            return _invokeTasks = new ContextTasks(this);
        }
    }
}
