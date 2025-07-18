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

namespace DotNetty.Transport.Libuv
{
    using System.Runtime.CompilerServices;
    using System.Net;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Libuv.Native;

    public abstract partial class NativeChannel<TChannel, TUnsafe> : AbstractChannel<TChannel, TUnsafe>, INativeChannel
        where TChannel : NativeChannel<TChannel, TUnsafe>
        where TUnsafe : NativeChannel<TChannel, TUnsafe>.NativeChannelUnsafe, new()
    {

        internal bool ReadPending;

        private volatile int v_state;

        private IPromise _connectPromise;
        private IScheduledTask _connectCancellationTask;

        protected NativeChannel(IChannel parent) : base(parent)
        {
            v_state = StateFlags.Open;
        }

        public override bool IsOpen => IsInState(StateFlags.Open);

        public override bool IsActive => IsInState(StateFlags.Active);

        protected override bool IsCompatible(IEventLoop eventLoop) => eventLoop is LoopExecutor;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected bool IsInState(int stateToCheck) => (v_state & stateToCheck) == stateToCheck;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected void SetState(int stateToSet) => v_state |= stateToSet;

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

        void DoConnect(EndPoint remoteAddress, EndPoint localAddress)
        {
            ConnectRequest request = null;
            try
            {
                if (localAddress is object)
                {
                    DoBind(localAddress);
                }
                request = new TcpConnect(Unsafe, (IPEndPoint)remoteAddress);
            }
            catch
            {
                request?.Dispose();
                throw;
            }
        }

        void DoFinishConnect() => OnConnected();

        protected override void DoClose()
        {
            var promise = _connectPromise;
            if (promise is object)
            {
                _ = promise.TrySetException(ThrowHelper.GetClosedChannelException()); 
                _connectPromise = null;
            }
        }

        protected virtual void OnConnected()
        {
            SetState(StateFlags.Active);
            _ = CacheLocalAddress();
            _ = CacheRemoteAddress();
        }

        protected abstract void DoStopRead();

        NativeHandle INativeChannel.GetHandle() => GetHandle();
        internal abstract NativeHandle GetHandle();
        bool INativeChannel.IsBound => IsBound;
        internal abstract bool IsBound { get; }

    }
}
