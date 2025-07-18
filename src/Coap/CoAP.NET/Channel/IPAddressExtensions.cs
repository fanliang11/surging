/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Net;
using System.Net.Sockets;

namespace CoAP.Channel
{
    /// <summary>
    /// Extension methods for <see cref="IPAddress"/>.
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// Checks whether the IP address is an IPv4-mapped IPv6 address.
        /// </summary>
        /// <param name="address">the <see cref="IPAddress"/> object to check</param>
        /// <returns>true if the IP address is an IPv4-mapped IPv6 address; otherwise, false.</returns>
        public static Boolean IsIPv4MappedToIPv6(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetworkV6)
                return false;
            Byte[] bytes = address.GetAddressBytes();
            for (Int32 i = 0; i < 10; i++)
            {
                if (bytes[i] != 0)
                    return false;
            }
            return bytes[10] == 0xff && bytes[11] == 0xff;
        }

        /// <summary>
        /// Maps the <see cref="IPAddress"/> object to an IPv4 address.
        /// </summary>
        /// <param name="address">the <see cref="IPAddress"/> object</param>
        /// <returns>An IPv4 address.</returns>
        public static IPAddress MapToIPv4(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                return address;
            Byte[] bytes = address.GetAddressBytes();
            Int64 newAddress = (UInt32)(bytes[12] & 0xff) | (UInt32)(bytes[13] & 0xff) << 8 | (UInt32)(bytes[14] & 0xff) << 16 | (UInt32)(bytes[15] & 0xff) << 24;
            return new IPAddress(newAddress);
        }

        /// <summary>
        /// Maps the <see cref="IPAddress"/> object to an IPv6 address.
        /// </summary>
        /// <param name="address">the <see cref="IPAddress"/> object</param>
        /// <returns>An IPv6 address.</returns>
        public static IPAddress MapToIPv6(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
                return address;
            Byte[] bytes = address.GetAddressBytes();
            Byte[] newAddress = new Byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, bytes[0], bytes[1], bytes[2], bytes[3] };
            return new IPAddress(newAddress);
        }
    }
}
