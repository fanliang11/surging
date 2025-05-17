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

    static class UnixApi
    {
#pragma warning disable IDE1006 // 命名样式
        [DllImport("libc", SetLastError = true)]
        static extern int setsockopt(int socket, int level, int option_name, IntPtr option_value, uint option_len);

        [DllImport("libc", SetLastError = true)]
        static extern unsafe int getsockopt(int socket, int level, int option_name, byte* optionValue, int* optionLen);
#pragma warning restore IDE1006 // 命名样式

        const int SOL_SOCKET_LINUX = 0x0001;
        const int SO_REUSEADDR_LINUX = 0x0002;
        const int SO_REUSEPORT_LINUX = 0x000f;

        const int SOL_SOCKET_OSX = 0xffff;
        const int SO_REUSEADDR_OSX = 0x0004;
        const int SO_REUSEPORT_OSX = 0x0200;

        internal static unsafe bool GetReuseAddress(IntPtr socket)
        {
            int value = 0;
            int status = 0;
            int optLen = sizeof(int);
            if (PlatformApi.IsLinux)
            {
                status = getsockopt(socket.ToInt32(), SOL_SOCKET_LINUX, SO_REUSEADDR_LINUX, (byte*)&value, &optLen);
            }
            else if (PlatformApi.IsDarwin)
            {
                status = getsockopt(socket.ToInt32(), SOL_SOCKET_OSX, SO_REUSEADDR_OSX, (byte*)&value, &optLen);
            }
            if (status != 0)
            {
                ThrowHelper.ThrowSocketException(Marshal.GetLastWin32Error());
            }

            return value != 0;
        }

        internal static unsafe void SetReuseAddress(IntPtr socket, int value)
        {
            int status = 0;
            if (PlatformApi.IsLinux)
            {
                status = setsockopt(socket.ToInt32(), SOL_SOCKET_LINUX, SO_REUSEADDR_LINUX, (IntPtr)(&value), sizeof(int));
            }
            else if (PlatformApi.IsDarwin)
            {
                status = setsockopt(socket.ToInt32(), SOL_SOCKET_OSX, SO_REUSEADDR_OSX, (IntPtr)(&value), sizeof(int));
            }
            if (status != 0)
            {
                ThrowHelper.ThrowSocketException(Marshal.GetLastWin32Error());
            }
        }

        internal static unsafe bool GetReusePort(IntPtr socket)
        {
            int value = 0;
            int status = 0;
            int optLen = sizeof(int);
            if (PlatformApi.IsLinux)
            {
                status = getsockopt(socket.ToInt32(), SOL_SOCKET_LINUX, SO_REUSEPORT_LINUX, (byte*)&value, &optLen);
            }
            else if (PlatformApi.IsDarwin)
            {
                status = getsockopt(socket.ToInt32(), SOL_SOCKET_OSX, SO_REUSEPORT_OSX, (byte*)&value, &optLen);
            }
            if (status != 0)
            {
                ThrowHelper.ThrowSocketException(Marshal.GetLastWin32Error());
            }
            return value != 0;
        }

        internal static unsafe void SetReusePort(IntPtr socket, int value)
        {
            int status = 0;
            if (PlatformApi.IsLinux)
            {
                status = setsockopt(socket.ToInt32(), SOL_SOCKET_LINUX, SO_REUSEPORT_LINUX, (IntPtr)(&value), sizeof(int));
            }
            else if (PlatformApi.IsDarwin)
            {
                status = setsockopt(socket.ToInt32(), SOL_SOCKET_OSX, SO_REUSEPORT_OSX, (IntPtr)(&value), sizeof(int));
            }
            if (status != 0)
            {
                ThrowHelper.ThrowSocketException(Marshal.GetLastWin32Error());
            }
        }
    }
}