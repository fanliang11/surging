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

    sealed unsafe class Async : NativeHandle
    {
        static readonly uv_work_cb WorkCallback = h => OnWorkCallback(h);

        readonly Action<object> _callback;
        readonly object _state;

        public Async(Loop loop, Action<object> callback, object state)
            : base(uv_handle_type.UV_ASYNC)
        {
            Debug.Assert(loop is object);
            Debug.Assert(callback is object);

            IntPtr handle = NativeMethods.Allocate(uv_handle_type.UV_ASYNC);
            try
            {
                int result = NativeMethods.uv_async_init(loop.Handle, handle, WorkCallback);
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
            _callback = callback;
            _state = state;
        }

        public void Send()
        {
            if (!IsValid)
            {
                return;
            }

            int result = NativeMethods.uv_async_send(Handle);
            NativeMethods.ThrowIfError(result);
        }

        void OnWorkCallback()
        {
            try
            {
                _callback(_state);
            }
            catch (Exception exception)
            {
                Logger.CallbackRrror(HandleType, Handle, exception);
            }
        }

        static void OnWorkCallback(IntPtr handle)
        {
            var workHandle = GetTarget<Async>(handle);
            workHandle?.OnWorkCallback();
        }
    }
}
