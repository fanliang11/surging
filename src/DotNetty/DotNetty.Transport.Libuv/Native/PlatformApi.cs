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
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    static partial class PlatformApi
    {
        const int AF_INET6_LINUX = 10;
        const int AF_INET6_OSX = 30;

        internal static uint GetAddressFamily(AddressFamily addressFamily)
        {
            // AF_INET 2
            if (addressFamily == AddressFamily.InterNetwork || IsWindows)
            {
                return (uint)addressFamily;
            }

            if (IsLinux)
            {
                return AF_INET6_LINUX;
            }

            if (IsDarwin)
            {
                return AF_INET6_OSX;
            }

            return ThrowHelper.ThrowInvalidOperationException_Dispatch(addressFamily);
        }

        internal static bool GetReuseAddress(TcpHandle tcpHandle)
        {
            IntPtr socketHandle = GetSocketHandle(tcpHandle);

            return IsWindows 
                ? WindowsApi.GetReuseAddress(socketHandle) 
                : UnixApi.GetReuseAddress(socketHandle);
        }

        internal static void SetReuseAddress(TcpHandle tcpHandle, int value)
        {
            IntPtr socketHandle = GetSocketHandle(tcpHandle);
            if (IsWindows)
            {
                WindowsApi.SetReuseAddress(socketHandle, value);
            }
            else
            {
                UnixApi.SetReuseAddress(socketHandle, value);
            }
        }

        internal static bool GetReusePort(TcpHandle tcpHandle)
        {
            if (IsWindows)
            {
                return GetReuseAddress(tcpHandle);
            }

            IntPtr socketHandle = GetSocketHandle(tcpHandle);
            return UnixApi.GetReusePort(socketHandle);
        }

        internal static void SetReusePort(TcpHandle tcpHandle, int value)
        {
            IntPtr socketHandle = GetSocketHandle(tcpHandle);
            // Ignore SO_REUSEPORT on Windows because it is controlled
            // by SO_REUSEADDR
            if (IsWindows)
            {
                return;
            }

            UnixApi.SetReusePort(socketHandle, value);
        }

        static IntPtr GetSocketHandle(TcpHandle handle)
        {
            IntPtr socket = IntPtr.Zero;
            _ = NativeMethods.uv_fileno(handle.Handle, ref socket);
            return socket;
        }
    }
}
