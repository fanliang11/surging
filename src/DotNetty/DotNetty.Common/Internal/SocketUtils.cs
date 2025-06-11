// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class SocketUtils
    {
        public static IPAddress AddressByName(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                bool isIPv6Supported = Socket.OSSupportsIPv6;
                if (isIPv6Supported)
                {
                    return IPAddress.IPv6Loopback;
                }
                else
                {
                    return IPAddress.Loopback;
                }
            }
            if (string.Equals(hostname, "0.0.0.0", StringComparison.Ordinal))
            {
                return IPAddress.Any;
            }
            if (string.Equals(hostname ,"::0", StringComparison.Ordinal) || string.Equals(hostname ,"::", StringComparison.Ordinal))
            {
                return IPAddress.IPv6Any;
            }
            if (IPAddress.TryParse(hostname, out IPAddress parseResult))
            {
                return parseResult;
            }
            var addressList = DnsCache.ResolveAsync(hostname).GetAwaiter().GetResult();
            return addressList[0];
        }
    }
}