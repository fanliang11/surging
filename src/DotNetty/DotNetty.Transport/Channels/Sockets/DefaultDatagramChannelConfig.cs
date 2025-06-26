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
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    public class DefaultDatagramChannelConfig : DefaultChannelConfiguration, IDatagramChannelConfig
    {
        const int DefaultFixedBufferSize = 2048;

        private readonly Socket _socket;

        public DefaultDatagramChannelConfig(IDatagramChannel channel, Socket socket)
            : base(channel, new FixedRecvByteBufAllocator(DefaultFixedBufferSize))
        {
            if (socket is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.socket); }

            _socket = socket;
        }

        public override T GetOption<T>(ChannelOption<T> option)
        {
            if (ChannelOption.SoBroadcast.Equals(option))
            {
                return (T)(object)Broadcast;
            }
            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                return (T)(object)ReceiveBufferSize;
            }
            if (ChannelOption.SoSndbuf.Equals(option))
            {
                return (T)(object)SendBufferSize;
            }
            if (ChannelOption.SoReuseaddr.Equals(option))
            {
                return (T)(object)ReuseAddress;
            }
            if (ChannelOption.IpMulticastLoopDisabled.Equals(option))
            {
                return (T)(object)LoopbackModeDisabled;
            }
            if (ChannelOption.IpMulticastTtl.Equals(option))
            {
                return (T)(object)TimeToLive;
            }
            if (ChannelOption.IpMulticastAddr.Equals(option))
            {
                return (T)(object)Interface;
            }
            if (ChannelOption.IpMulticastIf.Equals(option))
            {
                return (T)(object)NetworkInterface;
            }
            if (ChannelOption.IpTos.Equals(option))
            {
                return (T)(object)TrafficClass;
            }

            return base.GetOption(option);
        }

        public override bool SetOption<T>(ChannelOption<T> option, T value)
        {
            if (base.SetOption(option, value))
            {
                return true;
            }

            if (ChannelOption.SoBroadcast.Equals(option))
            {
                Broadcast = (bool)(object)value;
            }
            else if (ChannelOption.SoRcvbuf.Equals(option))
            {
                ReceiveBufferSize = (int)(object)value;
            }
            else if (ChannelOption.SoSndbuf.Equals(option))
            {
                SendBufferSize = (int)(object)value;
            }
            else if (ChannelOption.SoReuseaddr.Equals(option))
            {
                ReuseAddress = (bool)(object)value;
            }
            else if (ChannelOption.IpMulticastLoopDisabled.Equals(option))
            {
                LoopbackModeDisabled = (bool)(object)value;
            }
            else if (ChannelOption.IpMulticastTtl.Equals(option))
            {
                TimeToLive = (short)(object)value;
            }
            else if (ChannelOption.IpMulticastAddr.Equals(option))
            {
                Interface = (EndPoint)(object)value;
            }
            else if (ChannelOption.IpMulticastIf.Equals(option))
            {
                NetworkInterface = (NetworkInterface)(object)value;
            }
            else if (ChannelOption.IpTos.Equals(option))
            {
                TrafficClass = (int)(object)value;
            }
            else
            {
                return false;
            }

            return true;
        }

        public int SendBufferSize
        {
            get
            {
                try
                {
                    return _socket.SendBufferSize;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.SendBufferSize = value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                try
                {
                    return _socket.ReceiveBufferSize;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.ReceiveBufferSize = value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public int TrafficClass
        {
            get
            {
                try
                {
                    return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.TypeOfService);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.TypeOfService, value);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public bool ReuseAddress
        {
            get
            {
                try
                {
                    return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress) != 0;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value ? 1 : 0);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public bool Broadcast
        {
            get
            {
                try
                {
                    return _socket.EnableBroadcast;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.EnableBroadcast = value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public bool LoopbackModeDisabled
        {
            get
            {
                try
                {
                    return !_socket.MulticastLoopback;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.MulticastLoopback = !value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public short TimeToLive
        {
            get
            {
                try
                {
                    return (short)_socket.GetSocketOption(
                        AddressFamilyOptionLevel,
                        SocketOptionName.MulticastTimeToLive);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    _socket.SetSocketOption(
                        AddressFamilyOptionLevel,
                        SocketOptionName.MulticastTimeToLive,
                        value);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public EndPoint Interface
        {
            get
            {
                try
                {
                    return _socket.LocalEndPoint;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

                try
                {
                    _socket.Bind(value);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public NetworkInterface NetworkInterface
        {
            get
            {
                try
                {
                    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    int value = (int)_socket.GetSocketOption(
                        AddressFamilyOptionLevel,
                        SocketOptionName.MulticastInterface);
                    int index = IPAddress.NetworkToHostOrder(value);

                    if (0u < (uint)interfaces.Length
                        && index >= 0
                        && (uint)index < (uint)interfaces.Length)
                    {
                        return interfaces[index];
                    }

                    return null;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

                try
                {
                    int index = GetNetworkInterfaceIndex(value);
                    if (index >= 0)
                    {
                        _socket.SetSocketOption(
                            AddressFamilyOptionLevel,
                            SocketOptionName.MulticastInterface,
                            index);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        internal SocketOptionLevel AddressFamilyOptionLevel
        {
            get
            {
                if (_socket.AddressFamily == AddressFamily.InterNetwork)
                {
                    return SocketOptionLevel.IP;
                }

                if (_socket.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return SocketOptionLevel.IPv6;
                }

                throw new NotSupportedException($"Socket address family {_socket.AddressFamily} not supported, expecting InterNetwork or InterNetworkV6");
            }
        }

        internal int GetNetworkInterfaceIndex(NetworkInterface networkInterface)
        {
            if (networkInterface is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.networkInterface); }

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int index = 0; index < interfaces.Length; index++)
            {
                if (interfaces[index].Id == networkInterface.Id)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}