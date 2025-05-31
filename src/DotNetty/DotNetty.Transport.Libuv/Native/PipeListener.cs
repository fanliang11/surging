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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// IPC pipe server to hand out handles to different libuv loops.
    /// </summary>
    sealed class PipeListener : PipeHandle
    {
        private const int DefaultPipeBacklog = 128;
        private static readonly uv_watcher_cb ConnectionCallback = (h, s) => OnConnectionCallback(h, s);

        private readonly Action<Pipe, int> _onReadAction;
        private readonly List<Pipe> _pipes;
        private readonly WindowsApi _windowsApi;
        private int _requestId;

        public PipeListener(Loop loop, bool ipc) : base(loop, ipc)
        {
            _onReadAction = (p, s) => OnRead(p, s);

            _pipes = new List<Pipe>();
            _windowsApi = new WindowsApi();
            _requestId = 0;
        }

        public void Listen(string name, int backlog = DefaultPipeBacklog)
        {
            Debug.Assert(backlog > 0);

            Validate();
            int result = NativeMethods.uv_pipe_bind(Handle, name);
            NativeMethods.ThrowIfError(result);

            result = NativeMethods.uv_listen(Handle, backlog, ConnectionCallback);
            NativeMethods.ThrowIfError(result);
        }

        internal void Shutdown()
        {
            Pipe[] handles = _pipes.ToArray();
            _pipes.Clear();

            foreach (Pipe pipe in handles)
            {
                pipe.CloseHandle();
            }

            CloseHandle();
        }

        internal void DispatchHandle(NativeHandle handle)
        {
            if (0u >= (uint)_pipes.Count)
            {
                ThrowHelper.ThrowInvalidOperationException_Dispatch();
            }

            int id = Interlocked.Increment(ref _requestId);
            Pipe pipe = _pipes[Math.Abs(id % _pipes.Count)];

            _windowsApi.DetachFromIOCP(handle);
            pipe.Send(handle);
        }

        private unsafe void OnConnectionCallback(int status)
        {
            Pipe client = null;
            try
            {
                if (status < 0)
                {
                    throw NativeMethods.CreateError((uv_err_code)status);
                }
                else
                {
                    IntPtr loopHandle = ((uv_stream_t*)Handle)->loop;
                    var loop = GetTarget<Loop>(loopHandle);

                    client = new Pipe(loop, true); // IPC pipe
                    int result = NativeMethods.uv_accept(Handle, client.Handle);
                    NativeMethods.ThrowIfError(result);

                    _pipes.Add(client);
                    client.ReadStart(_onReadAction);
                }
            }
            catch (Exception exception)
            {
                client?.CloseHandle();
                if (Logger.WarnEnabled) Logger.FailedToSendServerHandleToClient(exception);
            }
        }

        private void OnRead(Pipe pipe, int status)
        {
            // The server connection is never meant to read anything back
            // it is only used for passing handles over to different loops
            // Therefore the only message should come back is EOF
            if (status >= 0)
            {
                return;
            }

            _windowsApi.Dispose();
            _ = _pipes.Remove(pipe);
            pipe.CloseHandle();

            if (status != NativeMethods.EOF)
            {
                OperationException error = NativeMethods.CreateError((uv_err_code)status);
                if (Logger.WarnEnabled) Logger.ReadError(error);
            }
        }

        private static void OnConnectionCallback(IntPtr handle, int status)
        {
            var server = GetTarget<PipeListener>(handle);
            server.OnConnectionCallback(status);
        }
    }
}
