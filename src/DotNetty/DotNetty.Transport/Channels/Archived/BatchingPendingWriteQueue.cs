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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    /// <summary>
    ///     A queue of write operations which are pending for later execution. It also updates the
    ///     <see cref="IChannel.IsWritable">writability</see> of the associated <see cref="IChannel" />, so that
    ///     the pending write operations are also considered to determine the writability.
    /// </summary>
    public sealed class BatchingPendingWriteQueue
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<BatchingPendingWriteQueue>();

        readonly IChannelHandlerContext _ctx;
        readonly int _maxSize;
        readonly ChannelOutboundBuffer _buffer;
        readonly IMessageSizeEstimatorHandle _estimatorHandle;

        // head and tail pointers for the linked-list structure. If empty head and tail are null.
        PendingWrite _head;
        PendingWrite _tail;
        int _size;

        public BatchingPendingWriteQueue(IChannelHandlerContext ctx, int maxSize)
        {
            if (ctx is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.ctx); }

            _ctx = ctx;
            _maxSize = maxSize;
            _buffer = ctx.Channel.Unsafe.OutboundBuffer;
            _estimatorHandle = ctx.Channel.Configuration.MessageSizeEstimator.NewHandle();
        }

        /// <summary>Returns <c>true</c> if there are no pending write operations left in this queue.</summary>
        public bool IsEmpty
        {
            get
            {
                Debug.Assert(_ctx.Executor.InEventLoop);

                return _head is null;
            }
        }

        /// <summary>Returns the number of pending write operations.</summary>
        public int Size
        {
            get
            {
                Debug.Assert(_ctx.Executor.InEventLoop);

                return _size;
            }
        }

        private int GetSize(object msg)
        {
            // It is possible for writes to be triggered from removeAndFailAll(). To preserve ordering,
            // we should add them to the queue and let removeAndFailAll() fail them later.
            int messageSize = _estimatorHandle.Size(msg);
            if (messageSize < 0)
            {
                // Size may be unknown so just use 0
                messageSize = 0;
            }
            return messageSize + PendingWriteQueue.PendingWriteOverhead;
        }

        /// <summary>Add the given <c>msg</c> and returns <see cref="Task" /> for completion of processing <c>msg</c>.</summary>
        public void Add(object msg, IPromise promise)
        {
            Debug.Assert(_ctx.Executor.InEventLoop);
            if (msg is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.msg); }

            int messageSize = GetSize(msg);

            PendingWrite currentTail = _tail;
            if (currentTail is object)
            {
                bool canBundle = CanBatch(msg, messageSize, currentTail.Size);
                if (canBundle)
                {
                    currentTail.Add(msg, messageSize);
                    if (!promise.IsVoid)
                    {
                        currentTail.Promise.Task.LinkOutcome(promise);
                    }
                    return;
                }
            }

            PendingWrite write;
            if (promise.IsVoid || promise is SimplePromiseAggregator)
            {
                var headPromise = _ctx.NewPromise();
                headPromise.Task.LinkOutcome(promise);
                write = PendingWrite.NewInstance(msg, messageSize, headPromise);
            }
            else
            {
                write = PendingWrite.NewInstance(msg, messageSize, promise);
            }
            if (currentTail is null)
            {
                _tail = _head = write;
            }
            else
            {
                currentTail.Next = write;
                _tail = write;
            }
            _size++;
            // We need to guard against null as channel.Unsafe.OutboundBuffer may returned null
            // if the channel was already closed when constructing the PendingWriteQueue.
            // See https://github.com/netty/netty/issues/3967
            _buffer?.IncrementPendingOutboundBytes(messageSize);
        }

        /// <summary>
        ///     Remove all pending write operation and fail them with the given <see cref="Exception" />. The messages will be
        ///     released
        ///     via <see cref="ReferenceCountUtil.SafeRelease(object)" />.
        /// </summary>
        public void RemoveAndFailAll(Exception cause)
        {
            Debug.Assert(_ctx.Executor.InEventLoop);
            if (cause is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.cause); }

            // It is possible for some of the failed promises to trigger more writes. The new writes
            // will "revive" the queue, so we need to clean them up until the queue is empty.
            for (PendingWrite write = _head; write is object; write = _head)
            {
                _head = _tail = null;
                _size = 0;
                while (write is object)
                {
                    PendingWrite next = write.Next;
                    ReferenceCountUtil.SafeRelease(write.Messages);
                    IPromise promise = write.Promise;
                    Recycle(write, false);
                    Util.SafeSetFailure(promise, cause, Logger);
                    write = next;
                }
            }
            AssertEmpty();
        }

        /// <summary>
        ///     Remove a pending write operation and fail it with the given <see cref="Exception" />. The message will be released
        ///     via
        ///     <see cref="ReferenceCountUtil.SafeRelease(object)" />.
        /// </summary>
        public void RemoveAndFail(Exception cause)
        {
            Debug.Assert(_ctx.Executor.InEventLoop);
            if (cause is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.cause); }

            PendingWrite write = _head;

            if (write is null)
            {
                return;
            }
            ReleaseMessages(write.Messages);
            IPromise promise = write.Promise;
            Util.SafeSetFailure(promise, cause, Logger);
            Recycle(write, true);
        }

        /// <summary>
        ///     Remove all pending write operation and performs them via
        ///     <see cref="IChannelHandlerContext.WriteAsync(object, IPromise)" />.
        /// </summary>
        /// <returns>
        ///     <see cref="Task" /> if something was written and <c>null</c> if the <see cref="BatchingPendingWriteQueue" />
        ///     is empty.
        /// </returns>
        public Task RemoveAndWriteAllAsync()
        {
            Debug.Assert(_ctx.Executor.InEventLoop);

            if (IsEmpty) { return null; }

            var p = _ctx.NewPromise();
            PromiseCombiner combiner = new PromiseCombiner(_ctx.Executor);
            try
            {
                // It is possible for some of the written promises to trigger more writes. The new writes
                // will "revive" the queue, so we need to write them up until the queue is empty.
                for (PendingWrite write = _head; write is object; write = _head)
                {
                    _head = _tail = null;
                    _size = 0;

                    while (write is object)
                    {
                        PendingWrite next = write.Next;
                        object msg = write.Messages;
                        IPromise promise = write.Promise;
                        Recycle(write, false);
                        if (!promise.IsVoid) { combiner.Add(promise.Task); }
                        _ = _ctx.WriteAsync(msg, promise);
                        write = next;
                    }
                }
                combiner.Finish(p);
            }
            catch (Exception exc)
            {
                p.SetException(exc);
            }
            AssertEmpty();
            return p.Task;
        }

        [Conditional("DEBUG")]
        void AssertEmpty() => Debug.Assert(_tail is null && _head is null && _size == 0);

        /// <summary>
        ///     Removes a pending write operation and performs it via
        ///     <see cref="IChannelHandlerContext.WriteAsync(object, IPromise)"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="Task" /> if something was written and <c>null</c> if the <see cref="BatchingPendingWriteQueue" />
        ///     is empty.
        /// </returns>
        public Task RemoveAndWriteAsync()
        {
            Debug.Assert(_ctx.Executor.InEventLoop);

            PendingWrite write = _head;
            if (write is null)
            {
                return null;
            }
            object msg = write.Messages;
            IPromise promise = write.Promise;
            Recycle(write, true);
            return _ctx.WriteAsync(msg, promise);
        }

        /// <summary>
        ///     Removes a pending write operation and release it's message via <see cref="ReferenceCountUtil.SafeRelease(object)"/>.
        /// </summary>
        /// <returns><see cref="IPromise" /> of the pending write or <c>null</c> if the queue is empty.</returns>
        public IPromise Remove()
        {
            Debug.Assert(_ctx.Executor.InEventLoop);

            PendingWrite write = _head;
            if (write is null)
            {
                return null;
            }
            IPromise promise = write.Promise;
            ReferenceCountUtil.SafeRelease(write.Messages);
            Recycle(write, true);
            return promise;
        }

        /// <summary>
        ///     Return the current message or <c>null</c> if empty.
        /// </summary>
        public List<object> Current
        {
            get
            {
                Debug.Assert(_ctx.Executor.InEventLoop);

                return _head?.Messages;
            }
        }

        public long? CurrentSize
        {
            get
            {
                Debug.Assert(_ctx.Executor.InEventLoop);

                return _head?.Size;
            }
        }

        bool CanBatch(object message, int size, long currentBatchSize)
        {
            if (size < 0)
            {
                return false;
            }

            if (currentBatchSize + size > _maxSize)
            {
                return false;
            }

            return true;
        }

        void Recycle(PendingWrite write, bool update)
        {
            PendingWrite next = write.Next;
            long writeSize = write.Size;

            if (update)
            {
                if (next is null)
                {
                    // Handled last PendingWrite so rest head and tail
                    // Guard against re-entrance by directly reset
                    _head = _tail = null;
                    _size = 0;
                }
                else
                {
                    _head = next;
                    _size--;
                    Debug.Assert(_size > 0);
                }
            }

            write.Recycle();
            // We need to guard against null as channel.unsafe().outboundBuffer() may returned null
            // if the channel was already closed when constructing the PendingWriteQueue.
            // See https://github.com/netty/netty/issues/3967
            _buffer?.DecrementPendingOutboundBytes(writeSize);
        }

        static void ReleaseMessages(List<object> messages)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                ReferenceCountUtil.SafeRelease(messages[i]);
            }
        }

        /// <summary>Holds all meta-data and construct the linked-list structure.</summary>
        sealed class PendingWrite
        {
            static readonly ThreadLocalPool<PendingWrite> Pool = new ThreadLocalPool<PendingWrite>(handle => new PendingWrite(handle));

            readonly ThreadLocalPool.Handle _handle;
            public PendingWrite Next;
            public long Size;
            public IPromise Promise;
            public readonly List<object> Messages;

            PendingWrite(ThreadLocalPool.Handle handle)
            {
                Messages = new List<object>();
                _handle = handle;
            }

            public static PendingWrite NewInstance(object msg, int size, IPromise promise)
            {
                PendingWrite write = Pool.Take();
                write.Add(msg, size);
                write.Promise = promise;
                return write;
            }

            public void Add(object msg, int size)
            {
                Messages.Add(msg);
                Size += size;
            }

            public void Recycle()
            {
                Size = 0;
                Next = null;
                Messages.Clear();
                Promise = null;
                _handle.Release(this);
            }
        }
    }
}