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
    using System.Net;

    public sealed class Tcp : TcpHandle
    {
        static readonly uv_alloc_cb AllocateCallback = OnAllocateCallback;
        static readonly uv_read_cb ReadCallback = OnReadCallback;

        readonly ReadOperation _pendingRead;
        INativeUnsafe _nativeUnsafe;

        internal Tcp(Loop loop, uint flags = 0 /* AF_UNSPEC */ ) : base(loop, flags)
        {
            _pendingRead = new ReadOperation();
        }

        internal void ReadStart(INativeUnsafe channel)
        {
            Debug.Assert(channel is object);

            Validate();
            int result = NativeMethods.uv_read_start(Handle, AllocateCallback, ReadCallback);
            NativeMethods.ThrowIfError(result);
            _nativeUnsafe = channel;
        }

        public void ReadStop()
        {
            if (Handle == IntPtr.Zero)
            {
                return;
            }

            // This function is idempotent and may be safely called on a stopped stream.
            _ = NativeMethods.uv_read_stop(Handle);
        }

        void OnReadCallback(int statusCode, OperationException error)
        {
            try
            {
                _pendingRead.Complete(statusCode, error);
                _nativeUnsafe.FinishRead(_pendingRead);
            }
            catch (Exception exception)
            {
                if (Logger.WarnEnabled) Logger.TcpHandleReadCallbcakError(Handle, exception);
            }
            finally
            {
                _pendingRead.Reset();
            }
        }

        static void OnReadCallback(IntPtr handle, IntPtr nread, ref uv_buf_t buf)
        {
            var tcp = GetTarget<Tcp>(handle);
            int status = (int)nread.ToInt64();

            OperationException error = null;
            if (status < 0 && status != NativeMethods.EOF)
            {
                error = NativeMethods.CreateError((uv_err_code)status);
            }

            tcp.OnReadCallback(status, error);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            Dispose();
            _pendingRead.Dispose();
            _nativeUnsafe = null;
        }

        void OnAllocateCallback(out uv_buf_t buf)
        {
            buf = _nativeUnsafe.PrepareRead(_pendingRead);
        }

        static void OnAllocateCallback(IntPtr handle, IntPtr suggestedSize, out uv_buf_t buf)
        {
            var tcp = GetTarget<Tcp>(handle);
            tcp.OnAllocateCallback(out buf);
        }

        public IPEndPoint GetPeerEndPoint()
        {
            Validate();
            return NativeMethods.TcpGetPeerName(Handle);
        }
    }
}
