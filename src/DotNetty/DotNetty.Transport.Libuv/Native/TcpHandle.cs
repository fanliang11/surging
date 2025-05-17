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
    using System.Runtime.InteropServices;

    public abstract unsafe class TcpHandle : NativeHandle
    {
        internal TcpHandle(Loop loop, uint flags) : base(uv_handle_type.UV_TCP)
        {
            Debug.Assert(loop is object);

            IntPtr handle = NativeMethods.Allocate(uv_handle_type.UV_TCP);

            try
            {
                // if flags is specified as AF_INET or AF_INET6, Libuv 
                // creates the socket when tcp handle is created.
                // Otherwise the socket is created when bind to an address.
                //  
                // This is for TcpListener to create socket early before bind
                int result = 0u >= flags
                    ? NativeMethods.uv_tcp_init(loop.Handle, handle)
                    : NativeMethods.uv_tcp_init_ex(loop.Handle, handle, flags);
                NativeMethods.ThrowIfError(result);
            }
            catch
            {
                NativeMethods.FreeMemory(handle);
                throw;
            }

            GCHandle gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
            ((uv_handle_t*)handle)->data = GCHandle.ToIntPtr(gcHandle);
            Handle = handle;
        }

        internal void Bind(IPEndPoint endPoint, bool dualStack = false)
        {
            Debug.Assert(endPoint is object);

            Validate();
            NativeMethods.GetSocketAddress(endPoint, out sockaddr addr);
            int result = NativeMethods.uv_tcp_bind(Handle, ref addr, (uint)(dualStack ? 1 : 0));
            NativeMethods.ThrowIfError(result);
        }

        public IPEndPoint GetLocalEndPoint()
        {
            Validate();
            return NativeMethods.TcpGetSocketName(Handle);
        }

        public void NoDelay(int value)
        {
            Validate();
            int result = NativeMethods.uv_tcp_nodelay(Handle, value);
            NativeMethods.ThrowIfError(result);
        }

        public int SendBufferSize(int value)
        {
            if ((uint)value > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(value, ExceptionArgument.value); }

            Validate();
            var size = (IntPtr)value;
            int result = NativeMethods.uv_send_buffer_size(Handle, ref size);
            NativeMethods.ThrowIfError(result);

            return size.ToInt32();
        }

        public int ReceiveBufferSize(int value)
        {
            if ((uint)value > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(value, ExceptionArgument.value); }

            Validate();
            var size = (IntPtr)value;

            int result = NativeMethods.uv_recv_buffer_size(Handle, ref size);
            NativeMethods.ThrowIfError(result);

            return size.ToInt32();
        }

        public void KeepAlive(int value, int delay)
        {
            Validate();
            int result = NativeMethods.uv_tcp_keepalive(Handle, value, delay);
            NativeMethods.ThrowIfError(result);
        }

        public void SimultaneousAccepts(bool value)
        {
            Validate();
            int result = NativeMethods.uv_tcp_simultaneous_accepts(Handle, value ? 1 : 0);
            NativeMethods.ThrowIfError(result);
        }
    }
}
