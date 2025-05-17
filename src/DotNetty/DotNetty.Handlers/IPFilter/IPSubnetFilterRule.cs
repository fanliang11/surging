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

namespace DotNetty.Handlers.IPFilter
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Numerics;
    using DotNetty.Common.Internal;

    /// <summary>
    /// Use this class to create rules for <see cref="RuleBasedIPFilter"/> that group IP addresses into subnets.
    /// Supports both, IPv4 and IPv6.
    /// </summary>
    public class IPSubnetFilterRule : IIPFilterRule
    {
        readonly IIPFilterRule filterRule;

        public IPSubnetFilterRule(string ipAddress, int cidrPrefix, IPFilterRuleType ruleType)
        {
            this.filterRule = SelectFilterRule(SocketUtils.AddressByName(ipAddress), cidrPrefix, ruleType);
        }

        public IPSubnetFilterRule(IPAddress ipAddress, int cidrPrefix, IPFilterRuleType ruleType)
        {
            this.filterRule = SelectFilterRule(ipAddress, cidrPrefix, ruleType);
        }

        public IPFilterRuleType RuleType => this.filterRule.RuleType;

        public bool Matches(IPEndPoint remoteAddress)
        {
            return this.filterRule.Matches(remoteAddress);
        }

        static IIPFilterRule SelectFilterRule(IPAddress ipAddress, int cidrPrefix, IPFilterRuleType ruleType)
        {
            if (ipAddress is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.ipAddress); }

            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return new IP4SubnetFilterRule(ipAddress, cidrPrefix, ruleType);

                case AddressFamily.InterNetworkV6:
                    return new IP6SubnetFilterRule(ipAddress, cidrPrefix, ruleType);

                default:
                    return ThrowHelper.FromArgumentOutOfRangeException_OnlySupportIPv4AndIPv6Addresses();
            }
        }

        private class IP4SubnetFilterRule : IIPFilterRule
        {
            readonly int networkAddress;
            readonly int subnetMask;

            public IP4SubnetFilterRule(IPAddress ipAddress, int cidrPrefix, IPFilterRuleType ruleType)
            {
                if (cidrPrefix < 0 || cidrPrefix > 32)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_IPv4RequiresTheSubnetPrefixToBeInRangeOf0_32(cidrPrefix);
                }

                this.subnetMask = PrefixToSubnetMask(cidrPrefix);
                this.networkAddress = GetNetworkAddress(ipAddress, this.subnetMask);
                this.RuleType = ruleType;
            }

            public IPFilterRuleType RuleType { get; }

            public bool Matches(IPEndPoint remoteAddress)
            {
                if (remoteAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return GetNetworkAddress(remoteAddress.Address, this.subnetMask) == this.networkAddress;
                }
                return false;
            }

            static int GetNetworkAddress(IPAddress ipAddress, int subnetMask)
            {
                return IpToInt(ipAddress) & subnetMask;
            }

            static int PrefixToSubnetMask(int cidrPrefix)
            {
                /*
                 * Perform the shift on a long and downcast it to int afterwards.
                 * This is necessary to handle a cidrPrefix of zero correctly.
                 * The left shift operator on an int only uses the five least
                 * significant bits of the right-hand operand. Thus -1 << 32 evaluates
                 * to -1 instead of 0. The left shift operator applied on a long
                 * uses the six least significant bits.
                 *
                 * Also see https://github.com/netty/netty/issues/2767
                 */
                return (int)((-1L << 32 - cidrPrefix) & 0xffffffff);
            }

            static int IpToInt(IPAddress ipAddress)
            {
                byte[] octets = ipAddress.GetAddressBytes();
                if (octets.Length != 4)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_OctetsCountMustBeEqual4ForIPv4();
                }

                return (octets[0] & 0xff) << 24 |
                    (octets[1] & 0xff) << 16 |
                    (octets[2] & 0xff) << 8 |
                    octets[3] & 0xff;
            }
        }

        private class IP6SubnetFilterRule : IIPFilterRule
        {
            readonly BigInteger networkAddress;
            readonly BigInteger subnetMask;

            public IP6SubnetFilterRule(IPAddress ipAddress, int cidrPrefix, IPFilterRuleType ruleType)
            {
                if (cidrPrefix < 0 || cidrPrefix > 128)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_IPv6RequiresTheSubnetPrefixToBeInRangeOf0_128(cidrPrefix);
                }

                this.subnetMask = CidrToSubnetMask((byte)cidrPrefix);
                this.networkAddress = GetNetworkAddress(ipAddress, this.subnetMask);
                this.RuleType = ruleType;
            }

            public IPFilterRuleType RuleType { get; }

            public bool Matches(IPEndPoint remoteAddress)
            {
                if (remoteAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return this.networkAddress == GetNetworkAddress(remoteAddress.Address, this.subnetMask);
                }
                return false;
            }

            static readonly BigInteger s_mask = new BigInteger(
                new byte[]
                {
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff,
                    0x00
                });
            static BigInteger CidrToSubnetMask(byte cidr)
            {
                BigInteger masked = 0u >= cidr ? 0 : s_mask << (128 - cidr);
                byte[] m = masked.ToByteArray();
                var bmask = new byte[16];
                int copy = (uint)m.Length > 16u ? 16 : m.Length;
                Array.Copy(m, 0, bmask, 0, copy);
                byte[] resBytes = bmask.Reverse().ToArray();
                return new BigInteger(resBytes);
            }

            static BigInteger IpToInt(IPAddress ipAddress)
            {
                byte[] octets = ipAddress.GetAddressBytes();
                if (octets.Length != 16)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_OctetsCountMustBeEqual16ForIPv6();
                }
                return new BigInteger(octets);
            }

            static BigInteger GetNetworkAddress(IPAddress ipAddress, BigInteger subnetMask)
            {
                return IpToInt(ipAddress) & subnetMask;
            }
        }
    }
}