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

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// <see cref="AbstractSocketChannel{TChannel, TUnsafe}"/> base class for <see cref="IChannel"/>s that operate on bytes.
    /// </summary>
    public abstract partial class AbstractSocketByteChannel<TChannel, TUnsafe> : AbstractSocketChannel<TChannel, TUnsafe>
        where TChannel : AbstractSocketByteChannel<TChannel, TUnsafe>
        where TUnsafe : AbstractSocketByteChannel<TChannel, TUnsafe>.SocketByteChannelUnsafe, new()
    {
        private static readonly string ExpectedTypes =
            $" (expected: {StringUtil.SimpleClassName<IByteBuffer>()})"; //+ ", " +

        // todo: FileRegion support        
        //typeof(FileRegion).Name + ')';

        private static readonly Action<object> FlushAction = c => OnFlushSync(c);
        private static readonly Action<object, object> ReadCompletedSyncCallback = (u, e) => OnReadCompletedSync(u, e);

        private bool _inputClosedSeenErrorOnRead;

        /// <summary>Create a new instance</summary>
        /// <param name="parent">the parent <see cref="IChannel"/> by which this instance was created. May be <c>null</c></param>
        /// <param name="socket">the underlying <see cref="Socket"/> on which it operates</param>
        protected AbstractSocketByteChannel(IChannel parent, Socket socket)
            : base(parent, socket)
        {
        }

        /// <summary>
        /// Shutdown the input side of the channel.
        /// </summary>
        public abstract Task ShutdownInputAsync();

        public virtual bool IsInputShutdown => false;

        internal bool ShouldBreakReadReady(IChannelConfiguration config)
        {
            return IsInputShutdown && (_inputClosedSeenErrorOnRead || !IsAllowHalfClosure(config));
        }

        private static bool IsAllowHalfClosure(IChannelConfiguration config)
        {
            return config is ISocketChannelConfiguration socketChannelConfiguration &&
                    socketChannelConfiguration.AllowHalfClosure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ScheduleSocketRead()
        {
            var operation = ReadOperation;
            bool pending;
            using (ExecutionContext.IsFlowSuppressed() ? default(AsyncFlowControl?) : ExecutionContext.SuppressFlow())
            {
                pending = Socket.ReceiveAsync(operation);
            }
            if (!pending)
            {
                // todo: potential allocation / non-static field?
                EventLoop.Execute(ReadCompletedSyncCallback, Unsafe, operation);
            }
        }

        static void OnReadCompletedSync(object u, object e) => ((TUnsafe)u).FinishRead((SocketChannelAsyncOperation<TChannel, TUnsafe>)e);

        private static void OnFlushSync(object channel)
        {
            ((TChannel)channel).Unsafe.InternalFlush0();
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            int writeSpinCount = -1;

            while (true)
            {
                object msg = input.Current;
                if (msg is null)
                {
                    // Wrote all messages.
                    break;
                }

                if (msg is IByteBuffer buf)
                {
                    int readableBytes = buf.ReadableBytes;
                    if (0u >= (uint)readableBytes)
                    {
                        input.Remove();
                        continue;
                    }

                    bool scheduleAsync = false;
                    bool done = false;
                    long flushedAmount = 0;
                    if (writeSpinCount == -1)
                    {
                        writeSpinCount = Configuration.WriteSpinCount;
                    }
                    for (int i = writeSpinCount - 1; i >= 0; i--)
                    {
                        int localFlushedAmount = DoWriteBytes(buf);
                        if (0u >= (uint)localFlushedAmount) // todo: check for "sent less than attempted bytes" to avoid unnecessary extra doWriteBytes call?
                        {
                            scheduleAsync = true;
                            break;
                        }

                        flushedAmount += localFlushedAmount;
                        if (!buf.IsReadable())
                        {
                            done = true;
                            break;
                        }
                    }

                    input.Progress(flushedAmount);

                    if (done)
                    {
                        input.Remove();
                    }
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    else if (IncompleteWrite(scheduleAsync, PrepareWriteOperation(buf.UnreadMemory)))
#else
                    else if (IncompleteWrite(scheduleAsync, PrepareWriteOperation(buf.GetIoBuffer())))
#endif
                    {
                        break;
                    }
                } /*else if (msg is FileRegion) { todo: FileRegion support
                FileRegion region = (FileRegion) msg;
                bool done = region.transfered() >= region.count();
                bool scheduleAsync = false;

                if (!done) {
                    long flushedAmount = 0;
                    if (writeSpinCount == -1) {
                        writeSpinCount = config().getWriteSpinCount();
                    }

                    for (int i = writeSpinCount - 1; i >= 0; i--) {
                        long localFlushedAmount = doWriteFileRegion(region);
                        if (localFlushedAmount == 0) {
                            scheduleAsync = true;
                            break;
                        }

                        flushedAmount += localFlushedAmount;
                        if (region.transfered() >= region.count()) {
                            done = true;
                            break;
                        }
                    }

                    input.progress(flushedAmount);
                }

                if (done) {
                    input.remove();
                } else {
                    incompleteWrite(scheduleAsync);
                    break;
                }
            }*/
                else
                {
                    // Should not reach here.
                    ThrowHelper.ThrowInvalidOperationException();
                }
            }
        }

        protected override object FilterOutboundMessage(object msg)
        {
            if (msg is IByteBuffer)
            {
                return msg;
                //IByteBuffer buf = (IByteBuffer) msg;
                //if (buf.isDirect()) {
                //    return msg;
                //}

                //return newDirectBuffer(buf);
            }

            // todo: FileRegion support
            //if (msg is FileRegion) {
            //    return msg;
            //}

            throw GetUnsupportedMsgTypeException(msg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotSupportedException GetUnsupportedMsgTypeException(object msg)
        {
            return new NotSupportedException(
                $"unsupported message type: {msg.GetType().Name} (expected: {StringUtil.SimpleClassName<IByteBuffer>()})");
        }

        protected bool IncompleteWrite(bool scheduleAsync, SocketChannelAsyncOperation<TChannel, TUnsafe> operation)
        {
            // Did not write completely.
            if (scheduleAsync)
            {
                SetState(StateFlags.WriteScheduled);
                bool pending;

                using (ExecutionContext.IsFlowSuppressed() ? default(AsyncFlowControl?) : ExecutionContext.SuppressFlow())
                {
                    pending = Socket.SendAsync(operation);
                }

                if (!pending)
                {
                    Unsafe.FinishWrite(operation);
                }

                return pending;
            }
            else
            {
                // Schedule flush again later so other tasks can be picked up input the meantime
                EventLoop.Execute(FlushAction, this);

                return true;
            }
        }

        // todo: support FileRegion
        ///// <summary>
        // /// Write a {@link FileRegion}
        // *
        // /// @param region        the {@link FileRegion} from which the bytes should be written
        // /// @return amount       the amount of written bytes
        // /// </summary>
        //protected abstract long doWriteFileRegion(FileRegion region);

        /// <summary>
        /// Reads bytes into the given <see cref="IByteBuffer"/> and returns the number of bytes that were read.
        /// </summary>
        /// <param name="buf">The <see cref="IByteBuffer"/> to read bytes into.</param>
        /// <returns>The number of bytes that were read into the buffer.</returns>
        protected abstract int DoReadBytes(IByteBuffer buf);

        /// <summary>
        /// Writes bytes from the given <see cref="IByteBuffer"/> to the underlying <see cref="IChannel"/>.
        /// </summary>
        /// <param name="buf">The <see cref="IByteBuffer"/> from which the bytes should be written.</param>
        /// <returns>The number of bytes that were written from the buffer.</returns>
        protected abstract int DoWriteBytes(IByteBuffer buf);
    }
}