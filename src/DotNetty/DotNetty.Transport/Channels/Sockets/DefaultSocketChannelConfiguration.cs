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

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// The default <see cref="ISocketChannelConfiguration"/> implementation.
    /// </summary>
    public class DefaultSocketChannelConfiguration : DefaultChannelConfiguration, ISocketChannelConfiguration
    {
        protected readonly Socket Socket;
        private int _allowHalfClosure;

        public DefaultSocketChannelConfiguration(ISocketChannel channel, Socket socket)
            : base(channel)
        {
            if (socket is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.socket); }
            Socket = socket;

            // Enable TCP_NODELAY by default if possible.
            try
            {
                TcpNoDelay = true;
            }
            catch
            {
            }
        }

        public override T GetOption<T>(ChannelOption<T> option)
        {
            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                return (T)(object)ReceiveBufferSize;
            }
            if (ChannelOption.SoSndbuf.Equals(option))
            {
                return (T)(object)SendBufferSize;
            }
            if (ChannelOption.TcpNodelay.Equals(option))
            {
                return (T)(object)TcpNoDelay;
            }
            if (ChannelOption.SoKeepalive.Equals(option))
            {
                return (T)(object)KeepAlive;
            }
            if (ChannelOption.SoReuseaddr.Equals(option))
            {
                return (T)(object)ReuseAddress;
            }
            if (ChannelOption.SoLinger.Equals(option))
            {
                return (T)(object)Linger;
            }
            //if (ChannelOption.IP_TOS.Equals(option))
            //{
            //    return (T)(object)this.TrafficClass;
            //}
            if (ChannelOption.AllowHalfClosure.Equals(option))
            {
                return (T)(object)AllowHalfClosure;
            }

            return base.GetOption(option);
        }

        public override bool SetOption<T>(ChannelOption<T> option, T value)
        {
            if (base.SetOption(option, value))
            {
                return true;
            }

            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                ReceiveBufferSize = (int)(object)value;
            }
            else if (ChannelOption.SoSndbuf.Equals(option))
            {
                SendBufferSize = (int)(object)value;
            }
            else if (ChannelOption.TcpNodelay.Equals(option))
            {
                TcpNoDelay = (bool)(object)value;
            }
            else if (ChannelOption.SoKeepalive.Equals(option))
            {
                KeepAlive = (bool)(object)value;
            }
            else if (ChannelOption.SoReuseaddr.Equals(option))
            {
                ReuseAddress = (bool)(object)value;
            }
            else if (ChannelOption.SoLinger.Equals(option))
            {
                Linger = (int)(object)value;
            }
            //else if (option == IP_TOS)
            //{
            //    setTrafficClass((Integer)value);
            //}
            else if (ChannelOption.AllowHalfClosure.Equals(option))
            {
                AllowHalfClosure = (bool)(object)value;
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool AllowHalfClosure
        {
            get { return SharedConstants.False < (uint)Volatile.Read(ref _allowHalfClosure); }
            set { Interlocked.Exchange(ref _allowHalfClosure, value ? SharedConstants.True : SharedConstants.False); }
        }

        public int ReceiveBufferSize
        {
            get
            {
                try
                {
                    return Socket.ReceiveBufferSize;
                }
                catch (ObjectDisposedException ex)
                {
                    return ThrowHelper.FromChannelException_Get_Int(ex);
                }
                catch (SocketException ex)
                {
                    return ThrowHelper.FromChannelException_Get_Int(ex);
                }
            }
            set
            {
                try
                {
                    Socket.ReceiveBufferSize = value;
                }
                catch (ObjectDisposedException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
                catch (SocketException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
            }
        }

        public virtual int SendBufferSize
        {
            get
            {
                try
                {
                    return Socket.SendBufferSize;
                }
                catch (ObjectDisposedException ex)
                {
                    return ThrowHelper.FromChannelException_Get_Int(ex);
                }
                catch (SocketException ex)
                {
                    return ThrowHelper.FromChannelException_Get_Int(ex);
                }
            }
            set
            {
                try
                {
                    Socket.SendBufferSize = value;
                }
                catch (ObjectDisposedException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
                catch (SocketException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
            }
        }

        public int Linger
        {
            get
            {
                try
                {
                    LingerOption lingerState = Socket.LingerState;
                    return lingerState.Enabled ? lingerState.LingerTime : -1;
                }
                catch (ObjectDisposedException ex)
                {
                    return ThrowHelper.FromChannelException_Get_Int(ex);
                }
                catch (SocketException ex)
                {
                    return ThrowHelper.FromChannelException_Get_Int(ex);
                }
            }
            set
            {
                try
                {
                    if (value < 0)
                    {
                        Socket.LingerState = new LingerOption(false, 0);
                    }
                    else
                    {
                        if (s_lingerCache.TryGetValue(value, out var lingerOption))
                        {
                            Socket.LingerState = lingerOption;
                        }
                        else
                        {
                            Socket.LingerState = new LingerOption(true, value);
                        }
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
                catch (SocketException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
            }
        }

        public bool KeepAlive
        {
            get
            {
                try
                {
                    return (int)Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive) != 0;
                }
                catch (ObjectDisposedException ex)
                {
                    return ThrowHelper.ThrowChannelException_Get_Bool(ex);
                }
                catch (SocketException ex)
                {
                    return ThrowHelper.ThrowChannelException_Get_Bool(ex);
                }
            }
            set
            {
                try
                {
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value ? 1 : 0);
                }
                catch (ObjectDisposedException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
                catch (SocketException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
            }
        }

        public bool ReuseAddress
        {
            get
            {
                try
                {
                    return (int)Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress) != 0;
                }
                catch (ObjectDisposedException ex)
                {
                    return ThrowHelper.ThrowChannelException_Get_Bool(ex);
                }
                catch (SocketException ex)
                {
                    return ThrowHelper.ThrowChannelException_Get_Bool(ex);
                }
            }
            set
            {
                try
                {
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value ? 1 : 0);
                }
                catch (ObjectDisposedException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
                catch (SocketException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
            }
        }

        public bool TcpNoDelay
        {
            get
            {
                try
                {
                    return Socket.NoDelay;
                }
                catch (ObjectDisposedException ex)
                {
                    return ThrowHelper.ThrowChannelException_Get_Bool(ex);
                }
                catch (SocketException ex)
                {
                    return ThrowHelper.ThrowChannelException_Get_Bool(ex);
                }
            }
            set
            {
                try
                {
                    Socket.NoDelay = value;
                }
                catch (ObjectDisposedException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
                catch (SocketException ex)
                {
                    ThrowHelper.ThrowChannelException_Set(ex);
                }
            }
        }

        private static readonly Dictionary<int, LingerOption> s_lingerCache;
        static DefaultSocketChannelConfiguration()
        {
            s_lingerCache = new Dictionary<int, LingerOption>(11);
            for(var idx = 0; idx <= 10; idx++)
            {
                s_lingerCache.Add(idx, new LingerOption(true, idx));
            }
        }
    }
}