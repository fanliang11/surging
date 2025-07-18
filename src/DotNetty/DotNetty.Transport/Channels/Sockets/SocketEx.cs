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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.Net.Sockets;

namespace DotNetty.Transport.Channels.Sockets
{
    public static class SocketEx
    {
        private static readonly byte[] FastpathEnabled = BitConverter.GetBytes(1);

        public static Socket CreateSocket()
        {
            // .Net45+，默认为AddressFamily.InterNetworkV6，并设置 DualMode 为 true，双线绑定
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.EnableFastpath();
            return socket;
        }

        public static Socket CreateSocket(AddressFamily addressFamily)
        {
            var socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.EnableFastpath();
            return socket;
        }

        public static void SafeClose(this Socket socket)
        {
            if (socket is null)
            {
                return;
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException)
            {
                // Socket is already closed -- we're done here
                return;
            }
            catch (Exception)
            {
                // Ignore
            }

            try
            {
                if (socket.Connected)
                    socket.Disconnect(false);
                else
                    socket.Close();
            }
            catch (Exception)
            {
                // Ignore
            }

            try
            {
                socket.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        /// <summary>Enables TCP Loopback Fast Path on a socket.
        /// See https://blogs.technet.microsoft.com/wincat/2012/12/05/fast-tcp-loopback-performance-and-low-latency-with-windows-server-2012-tcp-loopback-fast-path/
        /// for more information.</summary>
        /// <param name="socket">The socket for which FastPath should be enabled.</param>
        /// <remarks>Code take from Orleans(See https://github.com/dotnet/orleans/blob/main/src/Orleans.Core/Networking/Shared/SocketExtensions.cs). </remarks>
        internal static void EnableFastpath(this Socket socket)
        {
            if (!PlatformApis.IsWindows) { return; }

            const int SIO_LOOPBACK_FAST_PATH = -1744830448;
            try
            {
                // Win8/Server2012+ only
                var osVersion = Environment.OSVersion.Version;
                if (osVersion.Major > 6 || osVersion.Major == 6 && osVersion.Minor >= 2)
                {
                    socket.IOControl(SIO_LOOPBACK_FAST_PATH, FastpathEnabled, null);
                }
            }
            catch
            {
                // If the operating system version on this machine did
                // not support SIO_LOOPBACK_FAST_PATH (i.e. version
                // prior to Windows 8 / Windows Server 2012), handle the exception
            }
        }

        public static bool IsSocketAbortError(this SocketError errorCode)
        {
            switch (errorCode)
            {
                case SocketError.OperationAborted:
                case SocketError.Interrupted:
                // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
                case SocketError.InvalidArgument when !PlatformApis.IsWindows:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSocketResetError(this SocketError errorCode)
        {
            switch (errorCode)
            {
                case SocketError.ConnectionReset:
                case SocketError.Shutdown:
                // A connection reset can be reported as SocketError.ConnectionAborted on Windows.
                case SocketError.ConnectionAborted when PlatformApis.IsWindows:
                // ProtocolType can be removed once https://github.com/dotnet/corefx/issues/31927 is fixed.
                case SocketError.ProtocolType when PlatformApis.IsDarwin:
                    return true;

                default:
                    return false;
            }
        }
    }
}
