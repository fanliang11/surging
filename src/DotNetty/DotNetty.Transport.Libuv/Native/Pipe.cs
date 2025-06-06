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
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;

    /// <summary>
    /// IPC pipe for recieving handles from different libuv loops
    /// </summary>
    sealed unsafe class Pipe : PipeHandle
    {
        static readonly uv_alloc_cb AllocateCallback = OnAllocateCallback;
        static readonly uv_read_cb ReadCallback = OnReadCallback;

        readonly Scratch _scratch;

        Action<Pipe, int> _readCallback;

        internal Pipe(Loop loop, bool ipc) : base(loop, ipc)
        {
            _scratch = new Scratch();
        }

        public void ReadStart(Action<Pipe, int> readAction)
        {
            Validate();

            int result = NativeMethods.uv_read_start(Handle, AllocateCallback, ReadCallback);
            NativeMethods.ThrowIfError(result);
            _readCallback = readAction;
        }

        public void ReadStop()
        {
            if (Handle == IntPtr.Zero)
            {
                return;
            }

            // This function is idempotent and may be safely called on a stopped stream.
            int result = NativeMethods.uv_read_stop(Handle);
            NativeMethods.ThrowIfError(result);
        }

        void OnReadCallback(int status) => _readCallback(this, status);

        internal Tcp GetPendingHandle()
        {
            Tcp client = null;

            IntPtr loopHandle = ((uv_stream_t*)Handle)->loop;
            var loop = GetTarget<Loop>(loopHandle);
            int count = NativeMethods.uv_pipe_pending_count(Handle);

            if (count > 0)
            {
                var type = (uv_handle_type)NativeMethods.uv_pipe_pending_type(Handle);
                if (type == uv_handle_type.UV_TCP)
                {
                    client = new Tcp(loop);
                }
                else
                {
                    ThrowHelper.ThrowInvalidOperationException_ExpectingTcpHandle(type);
                }

                int result = NativeMethods.uv_accept(Handle, client.Handle);
                NativeMethods.ThrowIfError(result);
            }

            return client;
        }

        internal void Send(NativeHandle serverHandle)
        {
            Debug.Assert(serverHandle is object);

            var ping = new Ping(serverHandle);
            uv_buf_t[] bufs = ping.Bufs;
            int result = NativeMethods.uv_write2(
                ping.Handle,
                Handle,
                bufs,
                bufs.Length,
                serverHandle.Handle,
                Ping.WriteCallback);

            if (result < 0)
            {
                ping.Dispose();
                NativeMethods.ThrowOperationException((uv_err_code)result);
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            _scratch.Dispose();
        }

        static void OnReadCallback(IntPtr handle, IntPtr nread, ref uv_buf_t buf)
        {
            var pipe = GetTarget<Pipe>(handle);
            pipe.OnReadCallback((int)nread.ToInt64());
        }

        static void OnAllocateCallback(IntPtr handle, IntPtr suggestedSize, out uv_buf_t buf)
        {
            var pipe = GetTarget<Pipe>(handle);
            pipe.OnAllocateCallback(out buf);
        }

        void OnAllocateCallback(out uv_buf_t buf)
        {
            buf = _scratch.Buf;
        }

        sealed class Scratch : IDisposable
        {
            static readonly byte[] ScratchBuf = new byte[64];
            GCHandle _gcHandle;

            public Scratch()
            {
                byte[] scratch = ScratchBuf;
                _gcHandle = GCHandle.Alloc(scratch, GCHandleType.Pinned);
                IntPtr arrayHandle = _gcHandle.AddrOfPinnedObject();
                Buf = new uv_buf_t(arrayHandle, scratch.Length);
            }

            internal readonly uv_buf_t Buf;

            public void Dispose()
            {
                if (_gcHandle.IsAllocated)
                {
                    _gcHandle.Free();
                }
            }
        }

        sealed class Ping : NativeRequest
        {
            internal static readonly uv_watcher_cb WriteCallback = (h, s) => OnWriteCallback(h, s);
            static readonly byte[] PingBuf = TextEncodings.UTF8NoBOM.GetBytes("PING");

            readonly NativeHandle _sentHandle;
            GCHandle _gcHandle;

            public Ping(NativeHandle sentHandle) : base(uv_req_type.UV_WRITE, 0)
            {
                _sentHandle = sentHandle;
                byte[] array = PingBuf;
                Bufs = new uv_buf_t[1];

                GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                IntPtr arrayHandle = handle.AddrOfPinnedObject();
                _gcHandle = handle;

                Bufs[0] = new uv_buf_t(arrayHandle, array.Length);
            }

            internal readonly uv_buf_t[] Bufs;

            void OnWriteCallback(int status)
            {
                if (status < 0)
                {
                    OperationException error = NativeMethods.CreateError((uv_err_code)status);
                    if (Logger.WarnEnabled) Logger.FailedToWriteServerHandleToClient(error);
                }

                _sentHandle.CloseHandle();
                Dispose();
            }

            static void OnWriteCallback(IntPtr handle, int status)
            {
                var request = GetTarget<Ping>(handle);
                request.OnWriteCallback(status);
            }

            protected override void Dispose(bool disposing)
            {
                if (_gcHandle.IsAllocated)
                {
                    _gcHandle.Free();
                }
                base.Dispose(disposing);
            }
        }
    }
}
