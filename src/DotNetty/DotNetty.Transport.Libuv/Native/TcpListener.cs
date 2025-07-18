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

namespace DotNetty.Transport.Libuv.Native
{
    using System;
    using System.Diagnostics;

    sealed class TcpListener : TcpHandle
    {
        static readonly uv_watcher_cb ConnectionCallback = (h, s) => OnConnectionCallback(h, s);

        IServerNativeUnsafe _nativeUnsafe;

        public TcpListener(Loop loop, uint flags) : base(loop, flags)
        {
        }

        public void Listen(IServerNativeUnsafe channel, int backlog)
        {
            Debug.Assert(channel is object && _nativeUnsafe is null);
            Debug.Assert(backlog > 0);

            int result = NativeMethods.uv_listen(Handle, backlog, ConnectionCallback);
            NativeMethods.ThrowIfError(result);

            _nativeUnsafe = channel;
        }

        unsafe void OnConnectionCallback(int status)
        {
            NativeHandle client = null;
            Exception error = null;
            try
            {
                if (status < 0)
                {
                    error = NativeMethods.CreateError((uv_err_code)status);
                }
                else
                {
                    IntPtr loopHandle = ((uv_stream_t*)Handle)->loop;
                    var loop = GetTarget<Loop>(loopHandle);

                    client = new Tcp(loop);
                    int result = NativeMethods.uv_accept(Handle, client.Handle);
                    if (result < 0)
                    {
                        error = NativeMethods.CreateError((uv_err_code)result);
                    }
                }
            }
            catch (Exception exception)
            {
                error = exception;
            }

            _nativeUnsafe.Accept(new RemoteConnection(client, error));
        }

        static void OnConnectionCallback(IntPtr handle, int status)
        {
            var server = GetTarget<TcpListener>(handle);
            server.OnConnectionCallback(status);
        }

        protected override void OnClosed()
        {
            _nativeUnsafe = null;
            base.OnClosed();
        }
    }
}