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

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    partial class AbstractSocketChannel<TChannel, TUnsafe>
    {
        internal static readonly EventHandler<SocketAsyncEventArgs> IoCompletedCallback = (s, a) => OnIoCompleted(s, a);
        private static readonly Action<object, object> ConnectCallbackAction = (u, e) => OnConnectCompletedSync(u, e);
        private static readonly Action<object, object> ReadCallbackAction = (u, e) => OnReadCompletedSync(u, e);
        private static readonly Action<object, object> WriteCallbackAction = (u, e) => OnWriteCompletedSync(u, e);
        private static readonly Action<object> ClearReadPendingAction = c => OnClearReadPending(c);
        private static readonly Action<object, object> ConnectTimeoutAction = (c, a) => OnConnectTimeout(c, a);
        private static readonly Action<Task, object> CloseSafeOnCompleteAction = (t, s) => OnCloseSafeOnComplete(t, s);

        private static void OnConnectCompletedSync(object u, object e) => ((ISocketChannelUnsafe)u).FinishConnect((SocketChannelAsyncOperation<TChannel, TUnsafe>)e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OnReadCompletedSync(object u, object e) => ((ISocketChannelUnsafe)u).FinishRead((SocketChannelAsyncOperation<TChannel, TUnsafe>)e);

        private static void OnWriteCompletedSync(object u, object e) => ((ISocketChannelUnsafe)u).FinishWrite((SocketChannelAsyncOperation<TChannel, TUnsafe>)e);

        private static void OnClearReadPending(object channel) => ((TChannel)channel).ClearReadPending0();

        private static void OnConnectTimeout(object c, object a)
        {
            var self = (TChannel)c;
            // todo: call Socket.CancelConnectAsync(...)
            var promise = self._connectPromise;
            var cause = new ConnectTimeoutException("connection timed out: " + a.ToString());
            if (promise is object && promise.TrySetException(cause))
            {
                self.CloseSafe();
            }
        }

        private static  void OnCloseSafeOnComplete(Task t, object s)
        {
            var c = (TChannel)s;
            c._connectCancellationTask?.Cancel();
            c._connectPromise = null;
            c.CloseSafe(); 
        }
    }
}