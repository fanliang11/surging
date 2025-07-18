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
    using DotNetty.Common.Internal.Logging;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    abstract unsafe class NativeRequest : IDisposable
    {
        protected static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<NativeRequest>();

        readonly uv_req_type _requestType;
        protected internal IntPtr Handle;

        protected NativeRequest(uv_req_type requestType, int size)
        {
            IntPtr handle = NativeMethods.Allocate(requestType, size);

            GCHandle gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
            *(IntPtr*)handle = GCHandle.ToIntPtr(gcHandle);

            Handle = handle;
            _requestType = requestType;
        }

        protected bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle != IntPtr.Zero;
        }

        protected void CloseHandle()
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            IntPtr pHandle = ((uv_req_t*)handle)->data;

            // Free GCHandle
            if (pHandle != IntPtr.Zero)
            {
                GCHandle nativeHandle = GCHandle.FromIntPtr(pHandle);
                if (nativeHandle.IsAllocated)
                {
                    nativeHandle.Free();
                    ((uv_req_t*)handle)->data = IntPtr.Zero;
                }
            }

            // Release memory
            NativeMethods.FreeMemory(handle);
            Handle = IntPtr.Zero;
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (IsValid)
                {
                    CloseHandle();
                }
            }
            catch (Exception exception)
            {
                Logger.ErrorWhilstClosingHandle(_requestType ,Handle, exception);

                // For finalizer, we cannot allow this to escape.
                if (disposing) throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NativeRequest() => Dispose(false);

        internal static T GetTarget<T>(IntPtr handle)
        {
            Debug.Assert(handle != IntPtr.Zero);

            IntPtr internalHandle = ((uv_req_t*)handle)->data;
            if (internalHandle != IntPtr.Zero)
            {
                GCHandle gcHandle = GCHandle.FromIntPtr(internalHandle);
                if (gcHandle.IsAllocated)
                {
                    return (T)gcHandle.Target;
                }
            }
            return default;
        }
    }
}
