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

    abstract unsafe class PipeHandle : NativeHandle
    {
        const int NameBufferSize = 512;

        protected PipeHandle(Loop loop, bool ipc) : base(uv_handle_type.UV_NAMED_PIPE)
        {
            Debug.Assert(loop is object);

            IntPtr handle = NativeMethods.Allocate(uv_handle_type.UV_NAMED_PIPE);
            try
            {
                int result = NativeMethods.uv_pipe_init(loop.Handle, handle, ipc ? 1 : 0);
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

        public void Bind(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            Validate();
            int result = NativeMethods.uv_pipe_bind(Handle, name);
            NativeMethods.ThrowIfError(result);
        }

        public string GetSocketName()
        {
            Validate();
            var buf = stackalloc byte[NameBufferSize];
            var length = (IntPtr)NameBufferSize;
            var ptr = (IntPtr)buf;

            int result = NativeMethods.uv_pipe_getsockname(Handle, ptr, ref length);
            NativeMethods.ThrowIfError(result);

            string socketName = Marshal.PtrToStringAnsi(ptr, length.ToInt32());
            return socketName;
        }

        public string GetPeerName()
        {
            Validate();

            var buf = stackalloc byte[NameBufferSize];
            var length = (IntPtr)NameBufferSize;
            var ptr = (IntPtr)buf;

            int result = NativeMethods.uv_pipe_getpeername(Handle, ptr, ref length);
            NativeMethods.ThrowIfError(result);

            string peerName = Marshal.PtrToStringAnsi(ptr, length.ToInt32());
            return peerName;
        }
    }
}
