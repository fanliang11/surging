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
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels.Sockets;

    public sealed class ChannelOutboundBuffer
    {
        // Assuming a 64-bit JVM:
        //  - 16 bytes object header
        //  - 6 reference fields
        //  - 2 long fields
        //  - 2 int fields
        //  - 1 boolean field
        //  - padding
        internal static readonly int ChannelOutboundBufferEntryOverhead =
                SystemPropertyUtil.GetInt("io.netty.transport.outboundBufferEntrySizeOverhead", 96);

        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ChannelOutboundBuffer>();

        private static readonly ThreadLocalByteBufferList NioBuffers = new ThreadLocalByteBufferList();

        private readonly IChannel _channel;

        // Entry(flushedEntry) --> ... Entry(unflushedEntry) --> ... Entry(tailEntry)
        //
        // The Entry that is the first in the linked-list structure that was flushed
        private Entry _flushedEntry;
        // The Entry which is the first unflushed in the linked-list structure
        private Entry _unflushedEntry;
        // The Entry which represents the tail of the buffer
        private Entry _tailEntry;
        // The number of flushed entries that are not written yet
        private int _flushed;

        private int _nioBufferCount;
        private long _nioBufferSize;

        private bool _inFail;

        private long v_totalPendingSize;

        private int v_unwritable;

        private IRunnable v_fireChannelWritabilityChangedTask;

        internal ChannelOutboundBuffer(IChannel channel)
        {
            _channel = channel;
        }

        /// <summary>
        /// Adds the given message to this <see cref="ChannelOutboundBuffer"/>. The given
        /// <see cref="IPromise"/> will be notified once the message was written.
        /// </summary>
        /// <param name="msg">The message to add to the buffer.</param>
        /// <param name="size">The size of the message.</param>
        /// <param name="promise">The <see cref="IPromise"/> to notify once the message is written.</param>
        public void AddMessage(object msg, int size, IPromise promise)
        {
            Entry entry = Entry.NewInstance(msg, size, Total(msg), promise);
            if (_tailEntry is null)
            {
                _flushedEntry = null;
            }
            else
            {
                Entry tail = _tailEntry;
                tail.Next = entry;
            }
            _tailEntry = entry;
            if (_unflushedEntry is null)
            {
                _unflushedEntry = entry;
            }

            // increment pending bytes after adding message to the unflushed arrays.
            // See https://github.com/netty/netty/issues/1619
            IncrementPendingOutboundBytes(entry.PendingSize, false);
        }

        /// <summary>
        /// Add a flush to this <see cref="ChannelOutboundBuffer"/>. This means all previous added messages are marked
        /// as flushed and so you will be able to handle them.
        /// </summary>
        public void AddFlush()
        {
            // There is no need to process all entries if there was already a flush before and no new messages
            // where added in the meantime.
            //
            // See https://github.com/netty/netty/issues/2577
            Entry entry = _unflushedEntry;
            if (entry is object)
            {
                if (_flushedEntry is null)
                {
                    // there is no flushedEntry yet, so start with the entry
                    _flushedEntry = entry;
                }
                do
                {
                    _flushed++;
                    if (!entry.Promise.SetUncancellable())
                    {
                        // Was cancelled so make sure we free up memory and notify about the freed bytes
                        int pending = entry.Cancel();
                        DecrementPendingOutboundBytes(pending, false, true);
                    }
                    entry = entry.Next;
                }
                while (entry is object);

                // All flushed so reset unflushedEntry
                _unflushedEntry = null;
            }
        }

        /// <summary>
        /// Increments the number of pending bytes which will be written at some point.
        /// This method is thread-safe!
        /// </summary>
        /// <param name="size">The number of bytes to increment the count by.</param>
        internal void IncrementPendingOutboundBytes(long size) => IncrementPendingOutboundBytes(size, true);

        void IncrementPendingOutboundBytes(long size, bool invokeLater)
        {
            if (0ul >= (ulong)size)
            {
                return;
            }

            long newWriteBufferSize = Interlocked.Add(ref v_totalPendingSize, size);
            if (newWriteBufferSize > _channel.Configuration.WriteBufferHighWaterMark)
            {
                SetUnwritable(invokeLater);
            }
        }

        /// <summary>
        /// Decrements the number of pending bytes which will be written at some point.
        /// This method is thread-safe!
        /// </summary>
        /// <param name="size">The number of bytes to decrement the count by.</param>
        internal void DecrementPendingOutboundBytes(long size) => DecrementPendingOutboundBytes(size, true, true);

        void DecrementPendingOutboundBytes(long size, bool invokeLater, bool notifyWritability)
        {
            if (0ul >= (ulong)size)
            {
                return;
            }

            long newWriteBufferSize = Interlocked.Add(ref v_totalPendingSize, -size);
            if (notifyWritability && newWriteBufferSize <= _channel.Configuration.WriteBufferLowWaterMark)
            {
                SetWritable(invokeLater);
            }
        }

        private static long Total(object msg) => msg switch
        {
            IByteBuffer buf => buf.ReadableBytes,
            IFileRegion fileRegion => fileRegion.Count,
            IByteBufferHolder byteBufferHolder => byteBufferHolder.Content.ReadableBytes,
            _ => -1L,
        };

        /// <summary>
        /// Returns the current message to write, or <c>null</c> if nothing was flushed before and so is ready to be
        /// written.
        /// </summary>
        public object Current => _flushedEntry?.Message;

        /// <summary>
        /// Return the current message flush progress.
        /// </summary>
        /// <returns><c>0</c> if nothing was flushed before for the current message or there is no current message</returns>
        public long CurrentProgress()
        {
            Entry entry = _flushedEntry;
            return entry is null ? 0 : entry.Progress;
        }

        /// <summary>
        /// Notify the <see cref="IPromise"/> of the current message about writing progress.
        /// </summary>
        public void Progress(long amount)
        {
            // TODO: support progress report?
            Entry e = _flushedEntry;
            Debug.Assert(e is object);
            var p = e.Promise;
            long progress = e.Progress + amount;
            e.Progress = progress;
            //if (p is ChannelProgressivePromise)
            //{
            //    ((ChannelProgressivePromise)p).tryProgress(progress, e.Total);
            //}
        }

        /// <summary>
        /// Removes the current message, marks its <see cref="IPromise"/> as complete, and returns
        /// <c>true</c>. If no flushed message exists at the time this method is called, it returns <c>false</c> to
        /// signal that no more messages are ready to be handled.
        /// </summary>
        /// <returns><c>true</c> if a message existed and was removed, otherwise <c>false</c>.</returns>
        public bool Remove()
        {
            Entry e = _flushedEntry;
            if (e is null)
            {
                ClearNioBuffers();
                return false;
            }
            object msg = e.Message;

            IPromise promise = e.Promise;
            int size = e.PendingSize;

            RemoveEntry(e);

            if (!e.Cancelled)
            {
                // only release message, notify and decrement if it was not canceled before.
                ReferenceCountUtil.SafeRelease(msg);
                SafeSuccess(promise);
                DecrementPendingOutboundBytes(size, false, true);
            }

            // recycle the entry
            e.Recycle();//fanly update
          // _flushedEntry = null;//fanly update
            return true;
        }

        /// <summary>
        /// Removes the current message, marks its <see cref="IPromise"/> as complete using the given
        /// <see cref="Exception"/>, and returns <c>true</c>. If no flushed message exists at the time this method is
        /// called, it returns <c>false</c> to signal that no more messages are ready to be handled.
        /// </summary>
        /// <param name="cause">The <see cref="Exception"/> causing the message to be removed.</param>
        /// <returns><c>true</c> if a message existed and was removed, otherwise <c>false</c>.</returns>
        public bool Remove(Exception cause) => Remove0(cause, true);

        bool Remove0(Exception cause, bool notifyWritability)
        {
            Entry e = _flushedEntry;
            if (e is null)
            {
                ClearNioBuffers();
                return false;
            }
            object msg = e.Message;

            IPromise promise = e.Promise;
            int size = e.PendingSize;

            RemoveEntry(e);

            if (!e.Cancelled)
            {
                // only release message, fail and decrement if it was not canceled before.
                ReferenceCountUtil.SafeRelease(msg);

                SafeFail(promise, cause);
                DecrementPendingOutboundBytes(size, false, notifyWritability);
            }

            // recycle the entry
            e.Recycle();

            return true;
        }

        void RemoveEntry(Entry e)
        {
            if (0u >= (uint)(--_flushed))
            {
                // processed everything
                _flushedEntry = null;
                if (e == _tailEntry)
                {
                    _tailEntry = null;
                    _unflushedEntry = null;
                }
            }
            else
            {
                _flushedEntry = e.Next;
            }
        }

        /// <summary>
        /// Removes the fully written entries and updates the reader index of the partially written entry.
        /// This operation assumes all messages in this buffer are <see cref="IByteBuffer"/> instances.
        /// </summary>
        /// <param name="writtenBytes">The number of bytes that have been written so far.</param>
        public void RemoveBytes(long writtenBytes)
        {
            while (true)
            {
                object msg = Current;
                if (msg is not IByteBuffer buf)
                {
                    Debug.Assert(writtenBytes == 0);
                    break;
                }

                int readerIndex = buf.ReaderIndex;
                int readableBytes = buf.WriterIndex - readerIndex;

                if (readableBytes <= writtenBytes)
                {
                    if (writtenBytes != 0)
                    {
                        Progress(readableBytes);
                        writtenBytes -= readableBytes;
                    }
                    _ = Remove();
                }
                else
                {
                    // readableBytes > writtenBytes
                    if (writtenBytes != 0)
                    {
                        // Invalid nio buffer cache for partial writen, see https://github.com/Azure/DotNetty/issues/422
                        _flushedEntry.Buffer =null;
                        _flushedEntry.Buffers = null;

                        _ = buf.SetReaderIndex(readerIndex + (int)writtenBytes);
                        Progress(writtenBytes);
                    }
                    break;
                }
            }
            ClearNioBuffers();
        }

        /// <summary>
        /// Clears all ByteBuffer from the array so these can be GC'ed.
        /// See https://github.com/netty/netty/issues/3837
        /// </summary>
        void ClearNioBuffers()
        {
            var count = _nioBufferCount;
            if (count > 0)
            {
                _nioBufferCount = 0;
                NioBuffers.Value.Clear();
            }
        }

        /// <summary>
        /// Returns a list of direct ArraySegment&lt;byte&gt;, if the currently pending messages are made of
        /// <see cref="IByteBuffer"/> instances only. <see cref="NioBufferSize"/> will return the total number of
        /// readable bytes of these buffers.
        /// <para>
        /// Note that the returned array is reused and thus should not escape
        /// <see cref="AbstractChannel{TChannel, TUnsafe}.DoWrite(ChannelOutboundBuffer)"/>. Refer to
        /// <see cref="TcpSocketChannel{TChannel}.DoWrite(ChannelOutboundBuffer)"/> for an example.
        /// </para>
        /// </summary>
        /// <returns>A list of ArraySegment&lt;byte&gt; buffers.</returns>
        public List<ArraySegment<byte>> GetSharedBufferList() => GetSharedBufferList(int.MaxValue, int.MaxValue);

        /// <summary>
        /// Returns a list of direct ArraySegment&lt;byte&gt;, if the currently pending messages are made of
        /// <see cref="IByteBuffer"/> instances only. <see cref="NioBufferSize"/> will return the total number of
        /// readable bytes of these buffers.
        /// <para>
        /// Note that the returned array is reused and thus should not escape
        /// <see cref="AbstractChannel{TChannel, TUnsafe}.DoWrite(ChannelOutboundBuffer)"/>. Refer to
        /// <see cref="TcpSocketChannel{TChannel}.DoWrite(ChannelOutboundBuffer)"/> for an example.
        /// </para>
        /// </summary>
        /// <param name="maxCount">The maximum amount of buffers that will be added to the return value.</param>
        /// <param name="maxBytes">A hint toward the maximum number of bytes to include as part of the return value. Note that this value maybe exceeded because we make a best effort to include at least 1 <see cref="IByteBuffer"/> in the return value to ensure write progress is made.</param>
        /// <returns>A list of ArraySegment&lt;byte&gt; buffers.</returns>
        public List<ArraySegment<byte>> GetSharedBufferList(int maxCount, long maxBytes)
        {
            Debug.Assert(maxCount > 0);
            Debug.Assert(maxBytes > 0);

            long ioBufferSize = 0;
            int nioBufferCount = 0;
            InternalThreadLocalMap threadLocalMap = InternalThreadLocalMap.Get();

            List<ArraySegment<byte>> nioBuffers = NioBuffers.Get(threadLocalMap);
            Entry entry = _flushedEntry;
            while (IsFlushedEntry(entry) && entry.Message is IByteBuffer buf)
            {
                if (!entry.Cancelled)
                {
                    int readerIndex = buf.ReaderIndex;
                    int readableBytes = buf.WriterIndex - readerIndex;

                    if (readableBytes > 0)
                    {
                        if (maxBytes - readableBytes < ioBufferSize && nioBufferCount != 0)
                        {
                            // If the nioBufferSize + readableBytes will overflow maxBytes, and there is at least one entry
                            // we stop populate the ByteBuffer array. This is done for 2 reasons:
                            // 1. bsd/osx don't allow to write more bytes then Integer.MAX_VALUE with one writev(...) call
                            // and so will return 'EINVAL', which will raise an IOException. On Linux it may work depending
                            // on the architecture and kernel but to be safe we also enforce the limit here.
                            // 2. There is no sense in putting more data in the array than is likely to be accepted by the
                            // OS.
                            //
                            // See also:
                            // - https://www.freebsd.org/cgi/man.cgi?query=write&sektion=2
                            // - http://linux.die.net/man/2/writev
                            break;
                        }
                        ioBufferSize += readableBytes;
                        int count = entry.Count;
                        if ((uint)count > SharedConstants.TooBigOrNegative) // == -1
                        {
                            entry.Count = count = buf.IoBufferCount;
                        }
                        if (0u >= (uint)(count - 1))
                        {
                            ArraySegment<byte>? nioBuf = entry.Buffer;//fanly update
                            if (nioBuf is null)
                            {
                                // cache ByteBuffer as it may need to create a new ByteBuffer instance if its a
                                // derived buffer
                                entry.Buffer = nioBuf = buf.GetIoBuffer(readerIndex, readableBytes);
                            }
                            nioBuffers.Add(nioBuf.Value);
                            nioBufferCount++;
                        }
                        else
                        {
                            // The code exists in an extra method to ensure the method is not too big to inline as this
                            // branch is not very likely to get hit very frequently.
                            nioBufferCount = GetSharedBufferList(entry, buf, nioBuffers, nioBufferCount, maxCount);
                        }
                        if ((uint)nioBufferCount >= (uint)maxCount)
                        {
                            break;
                        }
                    }
                }
                entry = entry.Next;
            }
            _nioBufferCount = nioBufferCount;
            _nioBufferSize = ioBufferSize;
            return nioBuffers;
        }

        private static int GetSharedBufferList(Entry entry, IByteBuffer buf, List<ArraySegment<byte>> nioBuffers, int nioBufferCount, int maxCount)
        {
            ArraySegment<byte>[] nioBufs = entry.Buffers;
            if (nioBufs is null)
            {
                // cached ByteBuffers as they may be expensive to create in terms
                // of Object allocation
                entry.Buffers = nioBufs = buf.GetIoBuffers();
            }
            for (int i = 0; i < nioBufs.Length && nioBufferCount < maxCount; i++)
            {
                ArraySegment<byte> nioBuf = nioBufs[i];
                if (nioBuf.Array is null)
                {
                    break;
                }
                else if (0u >= (uint)nioBuf.Count)
                {
                    continue;
                }
                nioBuffers.Add(nioBuf);
                nioBufferCount++;
            }
            return nioBufferCount;
        }

        /// <summary>
        /// Returns the number of <see cref="IByteBuffer"/> that can be written out of the <see cref="IByteBuffer"/> array that was
        /// obtained via <see cref="GetSharedBufferList()"/>. This method <strong>MUST</strong> be called after <see cref="GetSharedBufferList()"/>
        /// was called.
        /// </summary>
        public int NioBufferCount => _nioBufferCount;

        /// <summary>
        /// Returns the number of bytes that can be written out of the <see cref="IByteBuffer"/> array that was
        /// obtained via <see cref="GetSharedBufferList()"/>. This method <strong>MUST</strong> be called after
        /// <see cref="GetSharedBufferList()"/> was called..
        /// </summary>
        public long NioBufferSize => _nioBufferSize;

        /// <summary>
        /// Returns <c>true</c> if and only if the total number of pending bytes (<see cref="TotalPendingWriteBytes"/>)
        /// did not exceed the write watermark of the <see cref="IChannel"/> and no user-defined writability flag
        /// (<see cref="SetUserDefinedWritability(int, bool)"/>) has been set to <c>false</c>.
        /// </summary>
        public bool IsWritable => 0u >= (uint)Volatile.Read(ref v_unwritable);

        /// <summary>
        /// Returns <c>true</c> if and only if the user-defined writability flag at the specified index is set to
        /// <c>true</c>.
        /// </summary>
        /// <param name="index">The index to check for user-defined writability.</param>
        /// <returns>
        /// <c>true</c> if the user-defined writability flag at the specified index is set to <c>true</c>.
        /// </returns>
        public bool GetUserDefinedWritability(int index) => 0u >= (uint)(Volatile.Read(ref v_unwritable) & WritabilityMask(index));

        /// <summary>
        /// Sets a user-defined writability flag at the specified index.
        /// </summary>
        /// <param name="index">The index where a writability flag should be set.</param>
        /// <param name="writable">Whether to set the index as writable or not.</param>
        public void SetUserDefinedWritability(int index, bool writable)
        {
            if (writable)
            {
                SetUserDefinedWritability(index);
            }
            else
            {
                ClearUserDefinedWritability(index);
            }
        }

        void SetUserDefinedWritability(int index)
        {
            int mask = ~WritabilityMask(index);
            var prevValue = Volatile.Read(ref v_unwritable);
            while (true)
            {
                int oldValue = prevValue;
                int newValue = prevValue & mask;
                prevValue = Interlocked.CompareExchange(ref v_unwritable, newValue, prevValue);
                if (prevValue == oldValue)
                {
                    if (prevValue != 0 && 0u >= (uint)newValue)
                    {
                        FireChannelWritabilityChanged(true);
                    }
                    break;
                }
            }
        }

        void ClearUserDefinedWritability(int index)
        {
            int mask = WritabilityMask(index);
            var prevValue = Volatile.Read(ref v_unwritable);
            while (true)
            {
                int oldValue = prevValue;
                int newValue = prevValue | mask;
                prevValue = Interlocked.CompareExchange(ref v_unwritable, newValue, prevValue);
                if (prevValue == oldValue)
                {
                    if (0u >= (uint)prevValue && newValue != 0)
                    {
                        FireChannelWritabilityChanged(true);
                    }
                    break;
                }
            }
        }

        private const uint c_writabilityMaskDiff = 31u - 1u;
        [MethodImpl(InlineMethod.AggressiveInlining)]
        static int WritabilityMask(int index)
        {
            if ((uint)(index - 1) <= c_writabilityMaskDiff)
            {
                return 1 << index;
            }
            return ThrowHelper.FromInvalidOperationException_WritabilityMask(index); // index < 1 || index > 31
        }

        void SetWritable(bool invokeLater)
        {
            var prevValue = Volatile.Read(ref v_unwritable);
            while (true)
            {
                int oldValue = prevValue;
                int newValue = prevValue & ~1;
                prevValue = Interlocked.CompareExchange(ref v_unwritable, newValue, prevValue);
                if (prevValue == oldValue)
                {
                    if (prevValue != 0 && 0u >= (uint)newValue)
                    {
                        FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }

        void SetUnwritable(bool invokeLater)
        {
            var prevValue = Volatile.Read(ref v_unwritable);
            while (true)
            {
                int oldValue = prevValue;
                int newValue = prevValue | 1;
                prevValue = Interlocked.CompareExchange(ref v_unwritable, newValue, prevValue);
                if (prevValue == oldValue)
                {
                    if (0u >= (uint)prevValue && newValue != 0)
                    {
                        FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }

        void FireChannelWritabilityChanged(bool invokeLater)
        {
            IChannelPipeline pipeline = _channel.Pipeline;
            if (invokeLater)
            {
                var task = Volatile.Read(ref v_fireChannelWritabilityChangedTask) ?? EnsureFireChannelWritabilityChangedTaskCreated(pipeline);
                _channel.EventLoop.Execute(task);
            }
            else
            {
                _ = pipeline.FireChannelWritabilityChanged();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private FireChannelWritabilityChangedTask EnsureFireChannelWritabilityChangedTaskCreated(IChannelPipeline pipeline)
        {
            var task = new FireChannelWritabilityChangedTask(pipeline);
            Interlocked.Exchange(ref v_fireChannelWritabilityChangedTask, task);
            return task;
        }

        sealed class FireChannelWritabilityChangedTask : IRunnable
        {
            private readonly IChannelPipeline _pipeline;

            public FireChannelWritabilityChangedTask(IChannelPipeline pipeline)
            {
                _pipeline = pipeline;
            }

            public void Run()
            {
                _pipeline.FireChannelWritabilityChanged();
            }
        }

        /// <summary>
        /// Returns the number of flushed messages in this <see cref="ChannelOutboundBuffer"/>.
        /// </summary>
        public int Size => _flushed;

        /// <summary>
        /// Returns <c>true</c> if there are flushed messages in this <see cref="ChannelOutboundBuffer"/>, otherwise
        /// <c>false</c>.
        /// </summary>
        public bool IsEmpty => 0u >= (uint)_flushed;

        public void FailFlushed(Exception cause, bool notify)
        {
            // Make sure that this method does not reenter.  A listener added to the current promise can be notified by the
            // current thread in the tryFailure() call of the loop below, and the listener can trigger another fail() call
            // indirectly (usually by closing the channel.)
            //
            // See https://github.com/netty/netty/issues/1501
            if (_inFail)
            {
                return;
            }

            try
            {
                _inFail = true;
                while (true)
                {
                    if (!Remove0(cause, notify))
                    {
                        break;
                    }
                }
            }
            finally
            {
                _inFail = false;
            }
        }

        sealed class CloseChannelTask : IRunnable
        {
            private readonly ChannelOutboundBuffer _buf;
            private readonly Exception _cause;
            private readonly bool _allowChannelOpen;

            public CloseChannelTask(ChannelOutboundBuffer buf, Exception cause, bool allowChannelOpen)
            {
                _buf = buf;
                _cause = cause;
                _allowChannelOpen = allowChannelOpen;
            }

            public void Run() => _buf.Close(_cause, _allowChannelOpen);
        }

        internal void Close(Exception cause, bool allowChannelOpen)
        {
            if (_inFail)
            {
                _channel.EventLoop.Execute(new CloseChannelTask(this, cause, allowChannelOpen));
                return;
            }

            _inFail = true;

            if (!allowChannelOpen && _channel.IsOpen)
            {
                ThrowHelper.ThrowInvalidOperationException_Close0();
            }

            if (!IsEmpty)
            {
                ThrowHelper.ThrowInvalidOperationException_Close1();
            }

            // Release all unflushed messages.
            try
            {
                Entry e = _unflushedEntry;
                while (e is object)
                {
                    // Just decrease; do not trigger any events via DecrementPendingOutboundBytes()
                    int size = e.PendingSize;
                    _ = Interlocked.Add(ref v_totalPendingSize, -size);

                    if (!e.Cancelled)
                    {
                        ReferenceCountUtil.SafeRelease(e.Message);
                        SafeFail(e.Promise, cause);
                    }
                    e = e.RecycleAndGetNext();
                }
            }
            finally
            {
                _inFail = false;
            }
            ClearNioBuffers();
        }

        internal void Close(ClosedChannelException cause) => Close(cause, false);

        static void SafeSuccess(IPromise promise)
        {
            // Only log if the given promise is not of type VoidChannelPromise as trySuccess(...) is expected to return
            // false.
            PromiseNotificationUtil.TrySuccess(promise, promise.IsVoid ? null : Logger);
        }

        static void SafeFail(IPromise promise, Exception cause)
        {
            // Only log if the given promise is not of type VoidChannelPromise as tryFailure(...) is expected to return
            // false.
            PromiseNotificationUtil.TryFailure(promise, cause, promise.IsVoid ? null : Logger);
        }

        public long TotalPendingWriteBytes => Volatile.Read(ref v_totalPendingSize);

        /// <summary>
        /// Gets the number of bytes that can be written before <see cref="IsWritable"/> returns <c>false</c>.
        /// This quantity will always be non-negative. If <see cref="IsWritable"/> is already <c>false</c>, then 0 is
        /// returned.
        /// </summary>
        /// <returns>
        /// The number of bytes that can be written before <see cref="IsWritable"/> returns <c>false</c>.
        /// </returns>
        public long BytesBeforeUnwritable
        {
            get
            {
                long bytes = _channel.Configuration.WriteBufferHighWaterMark - Volatile.Read(ref v_totalPendingSize);
                // If bytes is negative we know we are not writable, but if bytes is non-negative we have to check writability.
                // Note that totalPendingSize and isWritable() use different volatile variables that are not synchronized
                // together. totalPendingSize will be updated before isWritable().
                return bytes > 0 && IsWritable ? bytes : 0;
            }
        }

        /// <summary>
        /// Gets the number of bytes that must be drained from the underlying buffer before <see cref="IsWritable"/>
        /// returns <c>true</c>. This quantity will always be non-negative. If <see cref="IsWritable"/> is already
        /// <c>true</c>, then 0 is returned.
        /// </summary>
        /// <returns>
        /// The number of bytes that can be written before <see cref="IsWritable"/> returns <c>true</c>.
        /// </returns>
        public long BytesBeforeWritable
        {
            get
            {
                long bytes = Volatile.Read(ref v_totalPendingSize) - _channel.Configuration.WriteBufferLowWaterMark;
                // If bytes is negative we know we are writable, but if bytes is non-negative we have to check writability.
                // Note that totalPendingSize and isWritable() use different volatile variables that are not synchronized
                // together. totalPendingSize will be updated before isWritable().
                return bytes > 0 && !IsWritable ? bytes : 0;
            }
        }

        /// <summary>
        /// Calls <see cref="IMessageProcessor.ProcessMessage"/> for each flushed message in this
        /// <see cref="ChannelOutboundBuffer"/> until <see cref="IMessageProcessor.ProcessMessage"/> returns
        /// <c>false</c> or there are no more flushed messages to process.
        /// </summary>
        /// <param name="processor">
        /// The <see cref="IMessageProcessor"/> intance to use to process each flushed message.
        /// </param>
        public void ForEachFlushedMessage(IMessageProcessor processor)
        {
            if (processor is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.processor); }

            Entry entry = _flushedEntry;
            if (entry is null)
            {
                return;
            }

            do
            {
                if (!entry.Cancelled)
                {
                    if (!processor.ProcessMessage(entry.Message))
                    {
                        return;
                    }
                }
                entry = entry.Next;
            }
            while (IsFlushedEntry(entry));
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        bool IsFlushedEntry(Entry e) => e is object && e != _unflushedEntry;

        public interface IMessageProcessor
        {
            /// <summary>
            /// Will be called for each flushed message until it either there are no more flushed messages or this method returns <c>false</c>.
            /// </summary>
            /// <param name="msg">The message to process.</param>
            /// <returns><c>true</c> if the given message was successfully processed, otherwise <c>false</c>.</returns>
            bool ProcessMessage(object msg);
        }

        sealed class Entry
        {
            static readonly ThreadLocalPool<Entry> Pool = new ThreadLocalPool<Entry>(h => new Entry(h));

            private  ThreadLocalPool.Handle _handle;
            public Entry Next;
            public object Message;
            public ArraySegment<byte>[] Buffers;
            public ArraySegment<byte>? Buffer;
            public IPromise Promise;
            public long Progress;
            public long Total;
            public int PendingSize;
            public int Count = -1;
            public bool Cancelled;

            Entry(ThreadLocalPool.Handle handle)
            {
                _handle = handle;
            }

            public static Entry NewInstance(object msg, int size, long total, IPromise promise)
            {
                Entry entry = Pool.Take();
                entry.Message = msg;
                entry.PendingSize = size + ChannelOutboundBufferEntryOverhead;
                entry.Total = total;
                entry.Promise = promise;
                return entry;
            }

            public int Cancel()
            {
                if (!Cancelled)
                {
                    Cancelled = true;
                    int pSize = PendingSize;

                    // release message and replace with an empty buffer
                    ReferenceCountUtil.SafeRelease(Message);
                    Message = Unpooled.Empty;

                    PendingSize = 0;
                    Total = 0L;
                    Progress = 0L;
                    Buffers = null;
                    Buffer = null;
                    return pSize;
                }
                return 0;
            }

            public void Recycle()
            {
                Next = null;
                Buffers = null;
                Buffer = null;
                Message = null;
                Promise = null;
                Progress = 0L;
                Total = 0L;
                PendingSize = 0;
                Count = -1;
                Cancelled = false;
                //_handle = null;//fanly update
                 _handle.Release(this);
            }

            public Entry RecycleAndGetNext()
            {
                Entry next = Next;
                Recycle();
                return next;
            }
        }

        sealed class ThreadLocalByteBufferList : FastThreadLocal<List<ArraySegment<byte>>>
        {
            protected override List<ArraySegment<byte>> GetInitialValue() => new List<ArraySegment<byte>>(1024);
        }
    }
}