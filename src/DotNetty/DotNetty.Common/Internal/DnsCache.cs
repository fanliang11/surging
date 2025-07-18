//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DotNetty.Common.Internal
{
    internal static class DnsCache
    {
        private const int mruWatermark = 64;
        private static MruCache<string, DnsCacheEntry> s_resolveCache = new MruCache<string, DnsCacheEntry>(mruWatermark);
        private static readonly TimeSpan s_cacheTimeout = TimeSpan.FromSeconds(2);

        // Double-checked locking pattern requires volatile for read/write synchronization
        private static volatile string s_machineName;

        private static object ThisLock
        {
            get
            {
                return s_resolveCache;
            }
        }

        public static string MachineName
        {
            get
            {
                if (s_machineName is null)
                {
                    lock (ThisLock)
                    {
                        if (s_machineName is null)
                        {
                            try
                            {
                                s_machineName = Dns.GetHostEntryAsync(String.Empty).GetAwaiter().GetResult().HostName;
                            }
                            catch (SocketException)
                            {
                                throw;
                            }
                        }
                    }
                }

                return s_machineName;
            }
        }

        public static Task<IPAddress[]> ResolveAsync(Uri uri) => ResolveAsync(uri.DnsSafeHost);

        public static async Task<IPAddress[]> ResolveAsync(string hostName)
        {
            IPAddress[] hostAddresses = null;
            DateTime now = DateTime.UtcNow;

            lock (ThisLock)
            {
                DnsCacheEntry cacheEntry;
                if (s_resolveCache.TryGetValue(hostName, out cacheEntry))
                {
                    if (now.Subtract(cacheEntry.TimeStamp) > s_cacheTimeout)
                    {
                        _ = s_resolveCache.Remove(hostName);
                        cacheEntry = null;
                    }
                    else
                    {
                        if (cacheEntry.AddressList is null)
                        {
                            ThrowDnsResolveFailed(hostName);
                        }
                        hostAddresses = cacheEntry.AddressList;
                    }
                }
            }

            if (hostAddresses is null)
            {
                SocketException dnsException = null;
                try
                {
                    hostAddresses = await LookupHostName(hostName);
                }
                catch (SocketException e)
                {
                    dnsException = e;
                }

                lock (ThisLock)
                {
                    // MruCache doesn't have a this[] operator, so we first remove (just in case it exists already)
                    _ = s_resolveCache.Remove(hostName);
                    s_resolveCache.Add(hostName, new DnsCacheEntry(hostAddresses, now));
                }

                if (dnsException is object)
                {
                    ThrowDnsResolveFailed(hostName, dnsException);
                }
            }

            return hostAddresses;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDnsResolveFailed(string hostName)
        {
            throw GetDnsResolveFailedException(hostName, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDnsResolveFailed(string hostName, SocketException dnsException)
        {
            throw GetDnsResolveFailedException(hostName, dnsException);
        }

        private static Exception GetDnsResolveFailedException(string hostName, SocketException dnsException)
        {
            var errMsg = $"No DNS entries exist for host {hostName}.";
            return dnsException is object ? new Exception(errMsg, dnsException) : new Exception(errMsg);
        }

        internal static async Task<IPAddress[]> LookupHostName(string hostName)
        {
            return (await Dns.GetHostEntryAsync(hostName)).AddressList;
        }

        internal class DnsCacheEntry
        {
            private IPAddress[] _addressList;

            public DnsCacheEntry(IPAddress[] addressList, DateTime timeStamp)
            {
                TimeStamp = timeStamp;
                _addressList = addressList;
            }

            public IPAddress[] AddressList
            {
                get
                {
                    return _addressList;
                }
            }

            public DateTime TimeStamp { get; }
        }
    }
}
