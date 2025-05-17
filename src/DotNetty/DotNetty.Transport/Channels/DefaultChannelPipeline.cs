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

namespace DotNetty.Transport.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using Thread = DotNetty.Common.Concurrency.XThread;

    /// <summary>
    /// The default <see cref="IChannelPipeline"/> implementation.  It is usually created
    /// by a <see cref="IChannel"/> implementation when the <see cref="IChannel"/> is created.
    /// </summary>
    public class DefaultChannelPipeline : IChannelPipeline
    {
        internal static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<DefaultChannelPipeline>();

        private static readonly Action<object, object> CallHandlerAddedAction = (s, c) => OnCallHandlerAdded(s, c);
        private static readonly Action<object, object> CallHandlerRemovedAction = (s, c) => OnCallHandlerRemoved(s, c);
        private static readonly Action<object, object> DestroyUpAction = (s, c) => OnDestroyUp(s, c);
        private static readonly Action<object, object> DestroyDownAction = (s, c) => OnDestroyDown(s, c);

        private static readonly NameCachesLocal NameCaches = new NameCachesLocal();

        sealed class NameCachesLocal : FastThreadLocal<ConditionalWeakTable<Type, string>>
        {
            protected override ConditionalWeakTable<Type, string> GetInitialValue() => new ConditionalWeakTable<Type, string>();
        }

        private readonly IChannel _channel;
        private readonly VoidChannelPromise _voidPromise;

        private readonly AbstractChannelHandlerContext _head;
        internal readonly AbstractChannelHandlerContext _tail;

        private readonly bool _touch = ResourceLeakDetector.Enabled;

        private Dictionary<IEventExecutorGroup, IEventExecutor> _childExecutors;
        private IMessageSizeEstimatorHandle _estimatorHandle;
        private bool _firstRegistration = true;

        /// <summary>
        /// This is the head of a linked list that is processed by <see cref="CallHandlerAddedForAllHandlers" /> and so
        /// process all the pending <see cref="CallHandlerAdded0" />. We only keep the head because it is expected that
        /// the list is used infrequently and its size is small. Thus full iterations to do insertions is assumed to be
        /// a good compromised to saving memory and tail management complexity.
        /// </summary>
        private PendingHandlerCallback _pendingHandlerCallbackHead;

        /// <summary>
        /// Set to <c>true</c> once the <see cref="AbstractChannel{TChannel, TUnsafe}" /> is registered. Once set to <c>true</c>, the
        /// value will never change.
        /// </summary>
        private bool _registered;

        public DefaultChannelPipeline(IChannel channel)
        {
            if (channel is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.channel); }

            _channel = channel;
            _voidPromise = new VoidChannelPromise(channel, true);

            _tail = new TailContext(this);
            _head = new HeadContext(this)
            {
                Next = _tail
            };
            _tail.Prev = _head;
        }

        internal IMessageSizeEstimatorHandle EstimatorHandle
        {
            get
            {
                var handle = Volatile.Read(ref _estimatorHandle);
                if (handle is null)
                {
                    handle = _channel.Configuration.MessageSizeEstimator.NewHandle();
                    var current = Interlocked.CompareExchange(ref _estimatorHandle, handle, null);
                    if (current is object) { return current; }
                }
                return handle;
            }
        }

        internal object Touch(object msg, AbstractChannelHandlerContext next) => _touch ? ReferenceCountUtil.Touch(msg, next) : msg;

        public IChannel Channel => _channel;

        IEnumerator<IChannelHandler> IEnumerable<IChannelHandler>.GetEnumerator()
        {
            AbstractChannelHandlerContext current = _head.Next;
            while (current is object)
            {
                if (current == _tail) { yield break; }
                yield return current.Handler;
                current = current.Next;
            }
        }

        AbstractChannelHandlerContext NewContext(IEventExecutorGroup group, string name, IChannelHandler handler) => new DefaultChannelHandlerContext(this, GetChildExecutor(group), name, handler);

        AbstractChannelHandlerContext NewContext(IEventExecutor executor, string name, IChannelHandler handler) => new DefaultChannelHandlerContext(this, executor, name, handler);

        IEventExecutor GetChildExecutor(IEventExecutorGroup group)
        {
            if (group is null) { return null; }

            var pinEventExecutor = _channel.Configuration.PinEventExecutorPerGroup;
            if (!pinEventExecutor) { return group.GetNext(); }

            // Use size of 4 as most people only use one extra EventExecutor.
            var executorMap = _childExecutors ?? EnsureExecutorMapCreated();

            // Pin one of the child executors once and remember it so that the same child executor
            // is used to fire events for the same channel.
            if (!executorMap.TryGetValue(group, out var childExecutor))
            {
                childExecutor = group.GetNext();
                executorMap[group] = childExecutor;
            }
            return childExecutor;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Dictionary<IEventExecutorGroup, IEventExecutor> EnsureExecutorMapCreated()
        {
            return _childExecutors = new Dictionary<IEventExecutorGroup, IEventExecutor>(4, ReferenceEqualityComparer.Instance);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IChannelHandler>)this).GetEnumerator();

        public IChannelPipeline AddFirst(string name, IChannelHandler handler) => AddFirst(null, name, handler);

        public IChannelPipeline AddFirst(IEventExecutorGroup group, string name, IChannelHandler handler)
        {
            if (handler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handler); }

            AbstractChannelHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(handler);

                newCtx = NewContext(group, FilterName(name, handler), handler);

                AddFirst0(newCtx);

                // If the registered is false it means that the channel was not registered on an eventLoop yet.
                // In this case we add the context to the pipeline and add a task that will call
                // ChannelHandler.handlerAdded(...) once the channel is registered.
                if (!_registered)
                {
                    newCtx.SetAddPending();
                    CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                var executor = newCtx.Executor;
                if (!executor.InEventLoop)
                {
                    CallHandlerAddedInEventLoop(newCtx, executor);
                    return this;
                }
            }

            CallHandlerAdded0(newCtx);
            return this;
        }

        void AddFirst0(AbstractChannelHandlerContext newCtx)
        {
            var nextCtx = _head.Next;
            newCtx.Prev = _head;
            newCtx.Next = nextCtx;
            _head.Next = newCtx;
            nextCtx.Prev = newCtx;
        }

        public IChannelPipeline AddLast(string name, IChannelHandler handler) => AddLast(null, name, handler);

        public IChannelPipeline AddLast(IEventExecutorGroup group, string name, IChannelHandler handler)
        {
            if (handler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handler); }

            AbstractChannelHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(handler);

                newCtx = NewContext(group, FilterName(name, handler), handler);

                AddLast0(newCtx);

                // If the registered is false it means that the channel was not registered on an eventLoop yet.
                // In this case we add the context to the pipeline and add a task that will call
                // ChannelHandler.handlerAdded(...) once the channel is registered.
                if (!_registered)
                {
                    newCtx.SetAddPending();
                    CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                var executor = newCtx.Executor;
                if (!executor.InEventLoop)
                {
                    CallHandlerAddedInEventLoop(newCtx, executor);
                    return this;
                }
            }
            CallHandlerAdded0(newCtx);
            return this;
        }

        void AddLast0(AbstractChannelHandlerContext newCtx)
        {
            var prev = _tail.Prev;
            newCtx.Prev = prev;
            newCtx.Next = _tail;
            prev.Next = newCtx;
            _tail.Prev = newCtx;
        }

        public IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler) => AddBefore(null, baseName, name, handler);

        public IChannelPipeline AddBefore(IEventExecutorGroup group, string baseName, string name, IChannelHandler handler)
        {
            if (handler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handler); }

            AbstractChannelHandlerContext newCtx;
            AbstractChannelHandlerContext ctx;
            lock (this)
            {
                CheckMultiplicity(handler);
                name = FilterName(name, handler);
                ctx = GetContextOrDie(baseName);

                newCtx = NewContext(group, name, handler);

                AddBefore0(ctx, newCtx);

                // If the registered is false it means that the channel was not registered on an eventloop yet.
                // In this case we add the context to the pipeline and add a task that will call
                // ChannelHandler.handlerAdded(...) once the channel is registered.
                if (!_registered)
                {
                    newCtx.SetAddPending();
                    CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                var executor = newCtx.Executor;
                if (!executor.InEventLoop)
                {
                    CallHandlerAddedInEventLoop(newCtx, executor);
                    return this;
                }
            }
            CallHandlerAdded0(newCtx);
            return this;
        }

        static void AddBefore0(AbstractChannelHandlerContext ctx, AbstractChannelHandlerContext newCtx)
        {
            var ctxPrev = ctx.Prev;
            newCtx.Prev = ctxPrev;
            newCtx.Next = ctx;
            ctxPrev.Next = newCtx;
            ctx.Prev = newCtx;
        }

        public IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler) => AddAfter(null, baseName, name, handler);

        public IChannelPipeline AddAfter(IEventExecutorGroup group, string baseName, string name, IChannelHandler handler)
        {
            if (handler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handler); }

            AbstractChannelHandlerContext newCtx;
            AbstractChannelHandlerContext ctx;
            lock (this)
            {
                CheckMultiplicity(handler);
                name = FilterName(name, handler);
                ctx = GetContextOrDie(baseName);

                newCtx = NewContext(group, name, handler);

                AddAfter0(ctx, newCtx);

                // If the registered is false it means that the channel was not registered on an eventLoop yet.
                // In this case we remove the context from the pipeline and add a task that will call
                // ChannelHandler.handlerRemoved(...) once the channel is registered.
                if (!_registered)
                {
                    newCtx.SetAddPending();
                    CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                var executor = newCtx.Executor;
                if (!executor.InEventLoop)
                {
                    CallHandlerAddedInEventLoop(newCtx, executor);
                    return this;
                }
            }
            CallHandlerAdded0(newCtx);
            return this;
        }

        static void AddAfter0(AbstractChannelHandlerContext ctx, AbstractChannelHandlerContext newCtx)
        {
            newCtx.Prev = ctx;
            var ctxNext = ctx.Next;
            newCtx.Next = ctxNext;
            ctxNext.Prev = newCtx;
            ctx.Next = newCtx;
        }

        public IChannelPipeline AddFirst(IChannelHandler handler) => AddFirst(group: null, name: null, handler: handler);

        public IChannelPipeline AddFirst(params IChannelHandler[] handlers) => AddFirst(null, handlers);

        public IChannelPipeline AddFirst(IEventExecutorGroup group, params IChannelHandler[] handlers)
        {
            if (handlers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handlers); }

            if (0u >= (uint)handlers.Length || handlers[0] is null) { return this; }

            int size;
            for (size = 1; size < handlers.Length; size++)
            {
                if (handlers[size] is null) { break; }
            }

            for (int i = size - 1; i >= 0; i--)
            {
                _ = AddFirst(group: group, name: null, handler: handlers[i]);
            }

            return this;
        }

        public IChannelPipeline AddLast(IChannelHandler handler) => AddLast(group: null, name: null, handler: handler);

        public IChannelPipeline AddLast(params IChannelHandler[] handlers) => AddLast(null, handlers);

        public IChannelPipeline AddLast(IEventExecutorGroup group, params IChannelHandler[] handlers)
        {
            if (handlers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handlers); }

            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                if (h is null) { break; }
                _ = AddLast(group: group, name: null, handler: h);
            }
            return this;
        }

        string GenerateName(IChannelHandler handler)
        {
            ConditionalWeakTable<Type, string> cache = NameCaches.Value;
            Type handlerType = handler.GetType();
            string name = cache.GetValue(handlerType, t => GenerateName0(t));

            // It's not very likely for a user to put more than one handler of the same type, but make sure to avoid
            // any name conflicts.  Note that we don't cache the names generated here.
            if (Context0(name) is object)
            {
                string baseName = name.Substring(0, name.Length - 1); // Strip the trailing '0'.
                for (int i = 1; ; i++)
                {
                    string newName = baseName + i;
                    if (Context0(newName) is null)
                    {
                        name = newName;
                        break;
                    }
                }
            }
            return name;
        }

        static string GenerateName0(Type handlerType) => StringUtil.SimpleClassName(handlerType) + "#0";

        public IChannelPipeline Remove(IChannelHandler handler)
        {
            _ = Remove(GetContextOrDie(handler));
            return this;
        }

        public IChannelHandler Remove(string name) => Remove(GetContextOrDie(name)).Handler;

        public T Remove<T>() where T : class, IChannelHandler => (T)Remove(GetContextOrDie<T>()).Handler;

        public T RemoveIfExists<T>(string name) where T : class, IChannelHandler
        {
            return RemoveIfExists<T>(Context(name));
        }

        public T RemoveIfExists<T>() where T : class, IChannelHandler
        {
            return RemoveIfExists<T>(Context<T>());
        }

        public T RemoveIfExists<T>(IChannelHandler handler) where T : class, IChannelHandler
        {
            return RemoveIfExists<T>(Context(handler));
        }

        T RemoveIfExists<T>(IChannelHandlerContext ctx) where T : class, IChannelHandler
        {
            if (ctx is null)
            {
                return null;
            }
            return (T)Remove((AbstractChannelHandlerContext)ctx).Handler;
        }

        AbstractChannelHandlerContext Remove(AbstractChannelHandlerContext ctx)
        {
            Debug.Assert(ctx != _head && ctx != _tail);

            lock (this)
            {
                AtomicRemoveFromHandlerList(ctx);

                // If the registered is false it means that the channel was not registered on an eventloop yet.
                // In this case we remove the context from the pipeline and add a task that will call
                // ChannelHandler.handlerRemoved(...) once the channel is registered.
                if (!_registered)
                {
                    CallHandlerCallbackLater(ctx, false);
                    return ctx;
                }
                var executor = ctx.Executor;
                if (!executor.InEventLoop)
                {
                    executor.Execute(CallHandlerRemovedAction, this, ctx);
                    return ctx;
                }
            }
            CallHandlerRemoved0(ctx);
            return ctx;
        }

        /// <summary>
        /// Method is synchronized to make the handler removal from the double linked list atomic.
        /// </summary>
        /// <param name="context"></param>
        private void AtomicRemoveFromHandlerList(AbstractChannelHandlerContext context)
        {
            lock (this)
            {
                var prev = context.Prev;
                var next = context.Next;
                prev.Next = next;
                next.Prev = prev;
            }
        }

        public IChannelHandler RemoveFirst()
        {
            var headNext = _head.Next;
            if (headNext == _tail)
            {
                ThrowHelper.ThrowInvalidOperationException_Pipeline();
            }
            return Remove(headNext).Handler;
        }

        public IChannelHandler RemoveLast()
        {
            if (_head.Next == _tail)
            {
                ThrowHelper.ThrowInvalidOperationException_Pipeline();
            }
            return Remove(_tail.Prev).Handler;
        }

        public IChannelPipeline Replace(IChannelHandler oldHandler, string newName, IChannelHandler newHandler)
        {
            _ = Replace(GetContextOrDie(oldHandler), newName, newHandler);
            return this;
        }

        public IChannelHandler Replace(string oldName, string newName, IChannelHandler newHandler) => Replace(GetContextOrDie(oldName), newName, newHandler);

        public T Replace<T>(string newName, IChannelHandler newHandler)
            where T : class, IChannelHandler => (T)Replace(GetContextOrDie<T>(), newName, newHandler);

        IChannelHandler Replace(AbstractChannelHandlerContext ctx, string newName, IChannelHandler newHandler)
        {
            if (newHandler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.newHandler); }
            Debug.Assert(ctx != _head && ctx != _tail);

            AbstractChannelHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(newHandler);
                if (newName is null)
                {
                    newName = GenerateName(newHandler);
                }
                else
                {
                    bool sameName = string.Equals(ctx.Name, newName
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        );
#else
                        , StringComparison.Ordinal);
#endif
                    if (!sameName)
                    {
                        CheckDuplicateName(newName);
                    }
                }

                newCtx = NewContext(ctx._executor, newName, newHandler);

                Replace0(ctx, newCtx);

                // If the registered is false it means that the channel was not registered on an eventloop yet.
                // In this case we replace the context in the pipeline
                // and add a task that will signal handler it was added or removed
                // once the channel is registered.
                if (!_registered)
                {
                    CallHandlerCallbackLater(newCtx, true);
                    CallHandlerCallbackLater(ctx, false);
                    return ctx.Handler;
                }

                var executor = ctx.Executor;
                if (!executor.InEventLoop)
                {
                    executor.Execute(() =>
                    {
                        // Indicate new handler was added first (i.e. before old handler removed)
                        // because "removed" will trigger ChannelRead() or Flush() on newHandler and
                        // those event handlers must be called after handler was signaled "added".
                        CallHandlerAdded0(newCtx);
                        CallHandlerRemoved0(ctx);
                    });
                    return ctx.Handler;
                }
            }
            // Indicate new handler was added first (i.e. before old handler removed)
            // because "removed" will trigger ChannelRead() or Flush() on newHandler and
            // those event handlers must be called after handler was signaled "added".
            CallHandlerAdded0(newCtx);
            CallHandlerRemoved0(ctx);
            return ctx.Handler;
        }

        static void Replace0(AbstractChannelHandlerContext oldCtx, AbstractChannelHandlerContext newCtx)
        {
            var prev = oldCtx.Prev;
            var next = oldCtx.Next;
            newCtx.Prev = prev;
            newCtx.Next = next;

            // Finish the replacement of oldCtx with newCtx in the linked list.
            // Note that this doesn't mean events will be sent to the new handler immediately
            // because we are currently at the event handler thread and no more than one handler methods can be invoked
            // at the same time (we ensured that in replace().)
            prev.Next = newCtx;
            next.Prev = newCtx;

            // update the reference to the replacement so forward of buffered content will work correctly
            oldCtx.Prev = newCtx;
            oldCtx.Next = newCtx;
        }

        static void CheckMultiplicity(IChannelHandler handler)
        {
            if (handler is ChannelHandlerAdapter adapter)
            {
                ChannelHandlerAdapter h = adapter;
                if (!h.IsSharable && h.Added)
                {
                    ThrowHelper.ThrowChannelPipelineException(h);
                }
                h.Added = true;
            }
        }

        void CallHandlerAdded0(AbstractChannelHandlerContext ctx)
        {
            try
            {
                ctx.CallHandlerAdded();
            }
            catch (Exception ex)
            {
                bool removed = false;
                try
                {
                    AtomicRemoveFromHandlerList(ctx);
                    ctx.CallHandlerRemoved();
                    removed = true;
                }
                catch (Exception ex2)
                {
                    if (Logger.WarnEnabled)
                    {
                        Logger.FailedToRemoveAHandler(ctx, ex2);
                    }
                }

                if (removed)
                {
                    _ = FireExceptionCaught(ThrowHelper.GetChannelPipelineException_HandlerAddedThrowRemovedExc(ctx, ex));
                }
                else
                {
                    _ = FireExceptionCaught(ThrowHelper.GetChannelPipelineException_HandlerAddedThrowAlsoFailedToRemovedExc(ctx, ex));
                }
            }
        }

        void CallHandlerRemoved0(AbstractChannelHandlerContext ctx)
        {
            // Notify the complete removal.
            try
            {
                ctx.CallHandlerRemoved();
            }
            catch (Exception ex)
            {
                _ = FireExceptionCaught(ThrowHelper.GetChannelPipelineException_HandlerRemovedThrowExc(ctx, ex));
            }
        }

        internal void InvokeHandlerAddedIfNeeded()
        {
            Debug.Assert(_channel.EventLoop.InEventLoop);
            if (_firstRegistration)
            {
                _firstRegistration = false;
                // We are now registered to the EventLoop. It's time to call the callbacks for the ChannelHandlers,
                // that were added before the registration was done.
                CallHandlerAddedForAllHandlers();
            }
        }

        public IChannelHandler First()
        {
            var first = _head.Next;
            return first == _tail ? null : first.Handler;
        }

        public IChannelHandlerContext FirstContext()
        {
            var first = _head.Next;
            return first == _tail ? null : first;
        }

        public IChannelHandler Last()
        {
            var last = _tail.Prev;
            return last == _head ? null : last.Handler;
        }

        public IChannelHandlerContext LastContext()
        {
            var last = _tail.Prev;
            return last == _head ? null : last;
        }

        public IChannelHandler Get(string name) => Context(name)?.Handler;

        public T Get<T>() where T : class, IChannelHandler => (T)Context<T>()?.Handler;

        public IChannelHandlerContext Context(string name)
        {
            if (name is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            return Context0(name);
        }

        public IChannelHandlerContext Context(IChannelHandler handler)
        {
            if (handler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handler); }

            var ctx = _head.Next;
            while (true)
            {
                if (ctx is null)
                {
                    return null;
                }

                if (ctx.Handler == handler)
                {
                    return ctx;
                }

                ctx = ctx.Next;
            }
        }

        public IChannelHandlerContext Context<T>() where T : class, IChannelHandler
        {
            var ctx = _head.Next;
            while (true)
            {
                if (ctx is null)
                {
                    return null;
                }
                if (ctx.Handler is T)
                {
                    return ctx;
                }
                ctx = ctx.Next;
            }
        }

        /// <summary>
        /// Returns the string representation of this pipeline.
        /// </summary>
        public sealed override string ToString()
        {
            var buf = StringBuilderManager.Allocate()
                .Append(GetType().Name)
                .Append('{');
            AbstractChannelHandlerContext ctx = _head.Next;
            while (true)
            {
                if (ctx == _tail)
                {
                    break;
                }

                _ = buf.Append('(')
                    .Append(ctx.Name)
                    .Append(" = ")
                    .Append(ctx.Handler.GetType().Name)
                    .Append(')');

                ctx = ctx.Next;
                if (ctx == _tail)
                {
                    break;
                }

                _ = buf.Append(", ");
            }
            _ = buf.Append('}');
            return StringBuilderManager.ReturnAndFree(buf);
        }

        public IChannelPipeline FireChannelRegistered()
        {
            AbstractChannelHandlerContext.InvokeChannelRegistered(_head);
            return this;
        }

        public IChannelPipeline FireChannelUnregistered()
        {
            AbstractChannelHandlerContext.InvokeChannelUnregistered(_head);
            return this;
        }

        /// <summary>
        /// Removes all handlers from the pipeline one by one from tail (exclusive) to head (exclusive) to trigger
        /// <see cref="IChannelHandler.HandlerRemoved"/>. Note that we traverse up the pipeline <see cref="DestroyUp"/>
        /// before traversing down <see cref="DestroyDown"/> so that the handlers are removed after all events are
        /// handled.
        /// See: https://github.com/netty/netty/issues/3156
        /// </summary>
        void Destroy()
        {
            lock (this)
            {
                DestroyUp(_head.Next, false);
            }
        }

        void DestroyUp(AbstractChannelHandlerContext ctx, bool inEventLoop)
        {
            var currentThread = Thread.CurrentThread;
            var tailContext = _tail;
            while (true)
            {
                if (ctx == tailContext)
                {
                    DestroyDown(currentThread, tailContext.Prev, inEventLoop);
                    break;
                }

                IEventExecutor executor = ctx.Executor;
                if (!inEventLoop && !executor.IsInEventLoop(currentThread))
                {
                    executor.Execute(DestroyUpAction, this, ctx);
                    break;
                }

                ctx = ctx.Next;
                inEventLoop = false;
            }
        }

        void DestroyDown(Thread currentThread, AbstractChannelHandlerContext ctx, bool inEventLoop)
        {
            // We have reached at tail; now traverse backwards.
            var headContext = _head;
            while (true)
            {
                if (ctx == headContext)
                {
                    break;
                }

                IEventExecutor executor = ctx.Executor;
                if (inEventLoop || executor.IsInEventLoop(currentThread))
                {
                    AtomicRemoveFromHandlerList(ctx);
                    CallHandlerRemoved0(ctx);
                }
                else
                {
                    executor.Execute(DestroyDownAction, this, ctx);
                    break;
                }

                ctx = ctx.Prev;
                inEventLoop = false;
            }
        }

        public IChannelPipeline FireChannelActive()
        {
            AbstractChannelHandlerContext.InvokeChannelActive(_head);
            return this;
        }

        public IChannelPipeline FireChannelInactive()
        {
            AbstractChannelHandlerContext.InvokeChannelInactive(_head);
            return this;
        }

        public IChannelPipeline FireExceptionCaught(Exception cause)
        {
            AbstractChannelHandlerContext.InvokeExceptionCaught(_head, cause);
            return this;
        }

        public IChannelPipeline FireUserEventTriggered(object evt)
        {
            AbstractChannelHandlerContext.InvokeUserEventTriggered(_head, evt);
            return this;
        }

        public IChannelPipeline FireChannelRead(object msg)
        {
            AbstractChannelHandlerContext.InvokeChannelRead(_head, msg);
            return this;
        }

        public IChannelPipeline FireChannelReadComplete()
        {
            AbstractChannelHandlerContext.InvokeChannelReadComplete(_head);
            return this;
        }

        public IChannelPipeline FireChannelWritabilityChanged()
        {
            AbstractChannelHandlerContext.InvokeChannelWritabilityChanged(_head);
            return this;
        }

        public Task BindAsync(EndPoint localAddress) => _tail.BindAsync(localAddress);

        public Task ConnectAsync(EndPoint remoteAddress) => _tail.ConnectAsync(remoteAddress);

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress) => _tail.ConnectAsync(remoteAddress, localAddress);

        public Task DisconnectAsync() => _tail.DisconnectAsync();

        public Task DisconnectAsync(IPromise promise) => _tail.DisconnectAsync(promise);

        public Task CloseAsync() => _tail.CloseAsync();

        public Task CloseAsync(IPromise promise) => _tail.CloseAsync(promise);

        public Task DeregisterAsync() => _tail.DeregisterAsync();

        public Task DeregisterAsync(IPromise promise) => _tail.DeregisterAsync(promise);

        public IChannelPipeline Read()
        {
            _ = _tail.Read();
            return this;
        }

        public Task WriteAsync(object msg) => _tail.WriteAsync(msg);

        public Task WriteAsync(object msg, IPromise promise) => _tail.WriteAsync(msg, promise);

        public IChannelPipeline Flush()
        {
            _ = _tail.Flush();
            return this;
        }

        public Task WriteAndFlushAsync(object msg) => _tail.WriteAndFlushAsync(msg);

        public Task WriteAndFlushAsync(object msg, IPromise promise) => _tail.WriteAndFlushAsync(msg, promise);

        public IPromise NewPromise() => new DefaultPromise();

        public IPromise NewPromise(object state) => new DefaultPromise(state);

        public IPromise VoidPromise() => _voidPromise;

        string FilterName(string name, IChannelHandler handler)
        {
            if (name is null)
            {
                return GenerateName(handler);
            }
            CheckDuplicateName(name);
            return name;
        }

        void CheckDuplicateName(string name)
        {
            if (Context0(name) is object)
            {
                ThrowHelper.ThrowArgumentException_DuplicateHandler(name);
            }
        }

        AbstractChannelHandlerContext Context0(string name)
        {
            var context = _head.Next;
            while (context != _tail)
            {
                if (string.Equals(context.Name, name
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    ))
#else
                    , StringComparison.Ordinal))
#endif
                {
                    return context;
                }
                context = context.Next;
            }
            return null;
        }

        AbstractChannelHandlerContext GetContextOrDie(string name)
        {
            var ctx = (AbstractChannelHandlerContext)Context(name);
            if (ctx is null)
            {
                ThrowHelper.ThrowArgumentException_Context(name);
            }

            return ctx;
        }

        AbstractChannelHandlerContext GetContextOrDie(IChannelHandler handler)
        {
            var ctx = (AbstractChannelHandlerContext)Context(handler);
            if (ctx is null)
            {
                ThrowHelper.ThrowArgumentException_Context(handler);
            }

            return ctx;
        }

        AbstractChannelHandlerContext GetContextOrDie<T>() where T : class, IChannelHandler
        {
            var ctx = (AbstractChannelHandlerContext)Context<T>();
            if (ctx is null)
            {
                ThrowHelper.ThrowArgumentException_Context<T>();
            }

            return ctx;
        }

        void CallHandlerAddedForAllHandlers()
        {
            PendingHandlerCallback pendingHandlerCallbackHead;
            lock (this)
            {
                Debug.Assert(!_registered);

                // This Channel itself was registered.
                _registered = true;

                pendingHandlerCallbackHead = _pendingHandlerCallbackHead;
                // Null out so it can be GC'ed.
                _pendingHandlerCallbackHead = null;
            }

            // This must happen outside of the synchronized(...) block as otherwise handlerAdded(...) may be called while
            // holding the lock and so produce a deadlock if handlerAdded(...) will try to add another handler from outside
            // the EventLoop.
            PendingHandlerCallback task = pendingHandlerCallbackHead;
            while (task is object)
            {
                task.Execute();
                task = task.Next;
            }
        }

        void CallHandlerCallbackLater(AbstractChannelHandlerContext ctx, bool added)
        {
            Debug.Assert(!_registered);

            PendingHandlerCallback task = added ? (PendingHandlerCallback)new PendingHandlerAddedTask(this, ctx) : new PendingHandlerRemovedTask(this, ctx);
            PendingHandlerCallback pending = _pendingHandlerCallbackHead;
            if (pending is null)
            {
                _pendingHandlerCallbackHead = task;
            }
            else
            {
                // Find the tail of the linked-list.
                while (pending.Next is object)
                {
                    pending = pending.Next;
                }
                pending.Next = task;
            }
        }

        /// <summary>
        /// Called once an <see cref="Exception" /> hits the end of the <see cref="IChannelPipeline" /> without being
        /// handled by the user in <see cref="IChannelHandler.ExceptionCaught(IChannelHandlerContext, Exception)" />.
        /// </summary>
        protected virtual void OnUnhandledInboundException(Exception cause)
        {
            try
            {
                Logger.AnExceptionCaughtEventWasFired(cause);
            }
            finally
            {
                _ = ReferenceCountUtil.Release(cause);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void CallHandlerAddedInEventLoop(AbstractChannelHandlerContext newCtx, IEventExecutor executor)
        {
            newCtx.SetAddPending();
            executor.Execute(CallHandlerAddedAction, this, newCtx);
        }

        /// <summary>
        /// Called once the <see cref="IChannelHandler.ChannelActive(IChannelHandlerContext)" /> event hit
        /// the end of the <see cref="IChannelPipeline" />.
        /// </summary>
        protected virtual void OnUnhandledInboundChannelActive()
        {
        }

        /// <summary>
        /// Called once the <see cref="IChannelHandler.ChannelInactive(IChannelHandlerContext)" /> event hit
        /// the end of the <see cref="IChannelPipeline" />.
        /// </summary>
        protected virtual void OnUnhandledInboundChannelInactive()
        {
        }

        /// <summary>
        /// Called once a message hits the end of the <see cref="IChannelPipeline" /> without being handled by the user
        /// in <see cref="IChannelHandler.ChannelRead(IChannelHandlerContext, object)" />. This method is responsible
        /// for calling <see cref="ReferenceCountUtil.Release(object)" /> on the given msg at some point.
        /// </summary>
        protected virtual void OnUnhandledInboundMessage(object msg)
        {
#if DEBUG
            try
            {
                if (Logger.DebugEnabled) Logger.DiscardedInboundMessage(msg);
            }
            finally
            {
#endif
                _ = ReferenceCountUtil.Release(msg);
#if DEBUG
            }
#endif
        }

        protected virtual void OnUnhandledInboundMessage(IChannelHandlerContext ctx, object msg)
        {
            OnUnhandledInboundMessage(msg);
#if DEBUG
            if (Logger.DebugEnabled) { Logger.DiscardedInboundMessage(ctx); }
#endif
        }

        /// <summary>
        /// Called once the <see cref="IChannelHandler.ChannelReadComplete(IChannelHandlerContext)" /> event hit
        /// the end of the <see cref="IChannelPipeline" />.
        /// </summary>
        protected virtual void OnUnhandledInboundChannelReadComplete()
        {
        }

        /// <summary>
        /// Called once an user event hit the end of the <see cref="IChannelPipeline" /> without been handled by the user
        /// in <see cref="IChannelHandler.UserEventTriggered(IChannelHandlerContext, object)" />. This method is responsible
        /// to call <see cref="ReferenceCountUtil.Release(object)" /> on the given event at some point.
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void OnUnhandledInboundUserEventTriggered(object evt)
        {
            // This may not be a configuration error and so don't log anything.
            // The event may be superfluous for the current pipeline configuration.
            _ = ReferenceCountUtil.Release(evt);
        }

        /// <summary>
        /// Called once the <see cref="IChannelHandler.ChannelWritabilityChanged(IChannelHandlerContext)" /> event hit
        /// the end of the <see cref="IChannelPipeline" />.
        /// </summary>
        protected virtual void OnUnhandledChannelWritabilityChanged()
        {
        }

        internal protected virtual void IncrementPendingOutboundBytes(long size)
        {
            ChannelOutboundBuffer buffer = _channel.Unsafe.OutboundBuffer;
            if (buffer is object)
            {
                buffer.IncrementPendingOutboundBytes(size);
            }
        }

        internal protected virtual void DecrementPendingOutboundBytes(long size)
        {
            ChannelOutboundBuffer buffer = _channel.Unsafe.OutboundBuffer;
            if (buffer is object)
            {
                buffer.DecrementPendingOutboundBytes(size);
            }
        }

        private static void OnCallHandlerAdded(object self, object ctx)
        {
            ((DefaultChannelPipeline)self).CallHandlerAdded0((AbstractChannelHandlerContext)ctx);
        }

        private static void OnCallHandlerRemoved(object self, object ctx)
        {
            ((DefaultChannelPipeline)self).CallHandlerRemoved0((AbstractChannelHandlerContext)ctx);
        }

        private static void OnDestroyUp(object self, object ctx)
        {
            ((DefaultChannelPipeline)self).DestroyUp((AbstractChannelHandlerContext)ctx, true);
        }

        private static void OnDestroyDown(object self, object ctx)
        {
            ((DefaultChannelPipeline)self).DestroyDown(Thread.CurrentThread, (AbstractChannelHandlerContext)ctx, true);
        }

        sealed class TailContext : AbstractChannelHandlerContext, IChannelHandler
        {
            private static readonly string TailName = GenerateName0(typeof(TailContext));
            private static readonly int s_skipFlags = CalculateSkipPropagationFlags(typeof(TailContext));

            public TailContext(DefaultChannelPipeline pipeline)
                : base(pipeline, null, TailName, s_skipFlags)
            {
                _ = SetAddComplete();
            }

            public override IChannelHandler Handler => this;

            public void ChannelRegistered(IChannelHandlerContext context) { /* NOOP */ }

            public void ChannelUnregistered(IChannelHandlerContext context) { /* NOOP */ }

            public void ChannelActive(IChannelHandlerContext context) => _pipeline.OnUnhandledInboundChannelActive();

            public void ChannelInactive(IChannelHandlerContext context) => _pipeline.OnUnhandledInboundChannelInactive();

            public void ChannelWritabilityChanged(IChannelHandlerContext context) => _pipeline.OnUnhandledChannelWritabilityChanged();

            public void HandlerAdded(IChannelHandlerContext context) { /* NOOP */ }

            public void HandlerRemoved(IChannelHandlerContext context) { /* NOOP */ }

            public void UserEventTriggered(IChannelHandlerContext context, object evt) => _pipeline.OnUnhandledInboundUserEventTriggered(evt);

            public void ExceptionCaught(IChannelHandlerContext context, Exception exception) => _pipeline.OnUnhandledInboundException(exception);

            public void ChannelRead(IChannelHandlerContext context, object message) => _pipeline.OnUnhandledInboundMessage(context, message);

            public void ChannelReadComplete(IChannelHandlerContext context) => _pipeline.OnUnhandledInboundChannelReadComplete();

            [Skip]
            public void Deregister(IChannelHandlerContext context, IPromise promise) => context.DeregisterAsync(promise);

            [Skip]
            public void Disconnect(IChannelHandlerContext context, IPromise promise) => context.DisconnectAsync(promise);

            [Skip]
            public void Close(IChannelHandlerContext context, IPromise promise) => context.CloseAsync(promise);

            [Skip]
            public void Read(IChannelHandlerContext context) => context.Read();

            [Skip]
            public void Write(IChannelHandlerContext ctx, object message, IPromise promise) => ctx.WriteAsync(message, promise);

            [Skip]
            public void Flush(IChannelHandlerContext context) => context.Flush();

            [Skip]
            public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress) => context.BindAsync(localAddress);

            [Skip]
            public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress) => context.ConnectAsync(remoteAddress, localAddress);
        }

        sealed class HeadContext : AbstractChannelHandlerContext, IChannelHandler
        {
            private static readonly string HeadName = GenerateName0(typeof(HeadContext));
            private static readonly int s_skipFlags = CalculateSkipPropagationFlags(typeof(HeadContext));

            private readonly IChannelUnsafe _channelUnsafe;

            public HeadContext(DefaultChannelPipeline pipeline)
                : base(pipeline, null, HeadName, s_skipFlags)
            {
                _channelUnsafe = pipeline.Channel.Unsafe;
                _ = SetAddComplete();
            }

            public override IChannelHandler Handler => this;

            public void HandlerAdded(IChannelHandlerContext context) { /* NOOP */ }

            public void HandlerRemoved(IChannelHandlerContext context) { /* NOOP */ }

            public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress) => _channelUnsafe.BindAsync(localAddress);

            public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress) => _channelUnsafe.ConnectAsync(remoteAddress, localAddress);

            public void Disconnect(IChannelHandlerContext context, IPromise promise) => _channelUnsafe.Disconnect(promise);

            public void Close(IChannelHandlerContext context, IPromise promise) => _channelUnsafe.Close(promise);

            public void Deregister(IChannelHandlerContext context, IPromise promise) => _channelUnsafe.Deregister(promise);

            public void Read(IChannelHandlerContext context) => _channelUnsafe.BeginRead();

            public void Write(IChannelHandlerContext context, object message, IPromise promise) => _channelUnsafe.Write(message, promise);

            public void Flush(IChannelHandlerContext context) => _channelUnsafe.Flush();

            public void ExceptionCaught(IChannelHandlerContext ctx, Exception exception) => ctx.FireExceptionCaught(exception);

            public void ChannelRegistered(IChannelHandlerContext context)
            {
                _pipeline.InvokeHandlerAddedIfNeeded();
                _ = context.FireChannelRegistered();
            }

            public void ChannelUnregistered(IChannelHandlerContext context)
            {
                _ = context.FireChannelUnregistered();

                // Remove all handlers sequentially if channel is closed and unregistered.
                if (!_pipeline._channel.IsOpen)
                {
                    _pipeline.Destroy();
                }
            }

            public void ChannelActive(IChannelHandlerContext context)
            {
                _ = context.FireChannelActive();

                ReadIfIsAutoRead();
            }

            public void ChannelInactive(IChannelHandlerContext context) => context.FireChannelInactive();

            public void ChannelRead(IChannelHandlerContext ctx, object msg) => ctx.FireChannelRead(msg);

            public void ChannelReadComplete(IChannelHandlerContext ctx)
            {
                _ = ctx.FireChannelReadComplete();

                ReadIfIsAutoRead();
            }

            [MethodImpl(InlineMethod.AggressiveInlining)]
            private void ReadIfIsAutoRead()
            {
                var channel = _pipeline._channel;
                if (channel.Configuration.IsAutoRead)
                {
                    _ = channel.Read();
                }
            }

            public void UserEventTriggered(IChannelHandlerContext context, object evt) => context.FireUserEventTriggered(evt);

            public void ChannelWritabilityChanged(IChannelHandlerContext context) => context.FireChannelWritabilityChanged();
        }

        abstract class PendingHandlerCallback : IRunnable
        {
            protected readonly DefaultChannelPipeline Pipeline;
            protected readonly AbstractChannelHandlerContext Ctx;
            internal PendingHandlerCallback Next;

            protected PendingHandlerCallback(DefaultChannelPipeline pipeline, AbstractChannelHandlerContext ctx)
            {
                Pipeline = pipeline;
                Ctx = ctx;
            }

            public abstract void Run();

            internal abstract void Execute();
        }

        sealed class PendingHandlerAddedTask : PendingHandlerCallback
        {
            public PendingHandlerAddedTask(DefaultChannelPipeline pipeline, AbstractChannelHandlerContext ctx)
                : base(pipeline, ctx)
            {
            }

            public override void Run() => Pipeline.CallHandlerAdded0(Ctx);

            internal override void Execute()
            {
                IEventExecutor executor = Ctx.Executor;
                if (executor.InEventLoop)
                {
                    Pipeline.CallHandlerAdded0(Ctx);
                }
                else
                {
                    try
                    {
                        executor.Execute(this);
                    }
                    catch (RejectedExecutionException e)
                    {
                        if (Logger.WarnEnabled)
                        {
                            Logger.CannotInvokeHandlerAddedAsTheIEventExecutorRejectedIt(executor, Ctx, e);
                        }
                        Pipeline.AtomicRemoveFromHandlerList(Ctx);
                        Ctx.SetRemoved();
                    }
                }
            }
        }

        sealed class PendingHandlerRemovedTask : PendingHandlerCallback
        {
            public PendingHandlerRemovedTask(DefaultChannelPipeline pipeline, AbstractChannelHandlerContext ctx)
                : base(pipeline, ctx)
            {
            }

            public override void Run() => Pipeline.CallHandlerRemoved0(Ctx);

            internal override void Execute()
            {
                IEventExecutor executor = Ctx.Executor;
                if (executor.InEventLoop)
                {
                    Pipeline.CallHandlerRemoved0(Ctx);
                }
                else
                {
                    try
                    {
                        executor.Execute(this);
                    }
                    catch (RejectedExecutionException e)
                    {
                        if (Logger.WarnEnabled)
                        {
                            Logger.CannotInvokeHandlerRemovedAsTheIEventExecutorRejectedIt(executor, Ctx, e);
                        }
                        // remove0(...) was call before so just call AbstractChannelHandlerContext.setRemoved().
                        Ctx.SetRemoved();
                    }
                }
            }
        }
    }
}