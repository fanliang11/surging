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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Common.Concurrency;
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
    using System.Runtime.InteropServices;
#endif

    public abstract partial class AbstractSocketChannel<TChannel, TUnsafe> : AbstractChannel<TChannel, TUnsafe>
        where TChannel : AbstractSocketChannel<TChannel, TUnsafe>
        where TUnsafe : AbstractSocketChannel<TChannel, TUnsafe>.AbstractSocketUnsafe, new()
    {
        internal bool ReadPending;

        protected readonly Socket Socket;

        private SocketChannelAsyncOperation<TChannel, TUnsafe> _readOperation;
        private SocketChannelAsyncOperation<TChannel, TUnsafe> _writeOperation;
        private volatile int v_state;

        private IPromise _connectPromise;
        private IScheduledTask _connectCancellationTask;

        protected AbstractSocketChannel(IChannel parent, Socket socket)
            : base(parent)
        {
            Socket = socket;
            v_state = StateFlags.Open;

            try
            {
                Socket.Blocking = false;
            }
            catch (SocketException ex)
            {
                try
                {
                    socket.Dispose();
                }
                catch (SocketException ex2)
                {
                    if (Logger.WarnEnabled)
                    {
                        Logger.FailedToCloseAPartiallyInitializedSocket(ex2);
                    }
                }

                ThrowHelper.ThrowChannelException_FailedToEnterNonBlockingMode(ex);
            }
        }

        public override bool IsOpen
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => IsInState(StateFlags.Open);
        }

        public override bool IsActive
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => IsInState(StateFlags.Active);
        }

        /// <summary>
        ///     Set read pending to <c>false</c>.
        /// </summary>
        protected internal void ClearReadPending()
        {
            if (IsRegistered)
            {
                IEventLoop eventLoop = EventLoop;
                if (eventLoop.InEventLoop)
                {
                    ClearReadPending0();
                }
                else
                {
                    eventLoop.Execute(ClearReadPendingAction, this);
                }
            }
            else
            {
                // Best effort if we are not registered yet clear ReadPending. This happens during channel initialization.
                // NB: We only set the boolean field instead of calling ClearReadPending0(), because the SelectionKey is
                // not set yet so it would produce an assertion failure.
                ReadPending = false;
            }
        }

        void ClearReadPending0() => ReadPending = false;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected void SetState(int stateToSet) => v_state |= stateToSet;

        /// <returns>state before modification</returns>
        protected int ResetState(int stateToReset)
        {
            var oldState = v_state;
            if ((oldState & stateToReset) != 0)
            {
                v_state = oldState & ~stateToReset;
            }
            return oldState;
        }

        protected bool TryResetState(int stateToReset)
        {
            var oldState = v_state;
            if ((oldState & stateToReset) != 0)
            {
                v_state = oldState & ~stateToReset;
                return true;
            }
            return false;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected bool IsInState(int stateToCheck) => 0u >= (uint)((v_state & stateToCheck) - stateToCheck);

        protected SocketChannelAsyncOperation<TChannel, TUnsafe> ReadOperation
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _readOperation ?? EnsureReadOperationCreated();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private SocketChannelAsyncOperation<TChannel, TUnsafe> EnsureReadOperationCreated()
        {
            lock (this)
            {
                if (_readOperation is null)
                {
                    _readOperation = new SocketChannelAsyncOperation<TChannel, TUnsafe>((TChannel)this, true);
                }
            }
            return _readOperation;
        }

        private SocketChannelAsyncOperation<TChannel, TUnsafe> WriteOperation
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _writeOperation ?? EnsureWriteOperationCreated();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private SocketChannelAsyncOperation<TChannel, TUnsafe> EnsureWriteOperationCreated()
        {
            lock (this)
            {
                if (_writeOperation is null)
                {
                    _writeOperation = new SocketChannelAsyncOperation<TChannel, TUnsafe>((TChannel)this, false);
                }
            }
            return _writeOperation;
        }

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        protected SocketChannelAsyncOperation<TChannel, TUnsafe> PrepareWriteOperation(in ReadOnlyMemory<byte> buffer)
        {
            var operation = WriteOperation;
            operation.SetBuffer(MemoryMarshal.AsMemory(buffer));
            return operation;
        }
#else
        protected SocketChannelAsyncOperation<TChannel, TUnsafe> PrepareWriteOperation(in ArraySegment<byte> buffer)
        {
            var operation = WriteOperation;
            operation.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
            return operation;
        }
#endif

        protected SocketChannelAsyncOperation<TChannel, TUnsafe> PrepareWriteOperation(IList<ArraySegment<byte>> buffers)
        {
            var operation = WriteOperation;
            operation.BufferList = buffers;
            return operation;
        }

        protected void ResetWriteOperation()
        {
            var operation = _writeOperation;

            Debug.Assert(operation is object);

            if (operation.BufferList is null)
            {
                operation.SetBuffer(null, 0, 0);
            }
            else
            {
                operation.BufferList = null;
            }
        }

        /// <remarks>PORT NOTE: matches behavior of NioEventLoop.processSelectedKey</remarks>
        static void OnIoCompleted(object sender, SocketAsyncEventArgs args)
        {
            var operation = (SocketChannelAsyncOperation<TChannel, TUnsafe>)args;
            var channel = operation.Channel;
            var @unsafe = channel.Unsafe;
            IEventLoop eventLoop = channel.EventLoop;
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishRead(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ReadCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Connect:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishConnect(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ConnectCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishRead(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ReadCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendTo:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishWrite(operation);
                    }
                    else
                    {
                        eventLoop.Execute(WriteCallbackAction, @unsafe, operation);
                    }
                    break;
                default:
                    // todo: think of a better way to comm exception
                    ThrowHelper.ThrowArgumentException_TheLastOpCompleted(); break;
            }
        }


        protected override bool IsCompatible(IEventLoop eventLoop) => true;

        protected override void DoBeginRead()
        {
            if (!IsOpen) { return; }

            // 这儿不检测
            //// Channel.read() or ChannelHandlerContext.read() was called
            //final SelectionKey selectionKey = this.selectionKey;
            //if (!selectionKey.isValid())
            //{
            //    return;
            //}

            ReadPending = true;

            if (!IsInState(StateFlags.ReadScheduled))
            {
                v_state |= StateFlags.ReadScheduled;
                ScheduleSocketRead();
            }
        }

        protected abstract void ScheduleSocketRead();

        /// <summary>
        ///     Connect to the remote peer
        /// </summary>
        protected abstract bool DoConnect(EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        ///     Finish the connect
        /// </summary>
        protected abstract void DoFinishConnect(SocketChannelAsyncOperation<TChannel, TUnsafe> operation);

        protected override void DoClose()
        {
            var promise = _connectPromise;
            if (promise is object)
            {
                // Use TrySetException() instead of SetException() to avoid the race against cancellation due to timeout.
                _ = promise.TrySetException(ThrowHelper.GetClosedChannelException());
                _connectPromise = null;
            }

            IScheduledTask cancellationTask = _connectCancellationTask;
            if (cancellationTask is object)
            {
                _ = cancellationTask.Cancel();
                _connectCancellationTask = null;
            }

            var readOp = _readOperation;
            if (readOp is object)
            {
                readOp.Dispose();
                _readOperation = null;
            }

            var writeOp = _writeOperation;
            if (writeOp is object)
            {
                writeOp.Dispose();
                _writeOperation = null;
            }
        }
    }
}