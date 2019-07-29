using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Stage.Internal.Implementation
{
    public class IPAddressChecker : IIPChecker
    {
        private readonly ConcurrentDictionary<string, ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>> _ipNetworkSegments = new ConcurrentDictionary<string, ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>>();

        public IPAddressChecker()
        {
            Init();
        }

        public bool IsBlackIp(IPAddress ip)
        {
            return false;
        }

        public void Init()
        {
            var settings = AppConfig.Options.AccessSetting;
            settings.ForEach(setting =>
            {
                if (!string.IsNullOrEmpty(setting.RoutePath))
                {  
                    var whiteIPNetworkSegments = GetIPNetworkSegments(setting.WhiteList.Split(","));
                    var blackIPNetworkSegments = GetIPNetworkSegments(setting.BlackList.Split(","));
                    _ipNetworkSegments.TryAdd(setting.RoutePath, new ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>(whiteIPNetworkSegments, blackIPNetworkSegments));
                }
            }
            );
        }

        private List<IPNetworkSegment> GetIPNetworkSegments(IEnumerable<string> ipAddresses)
        {
            var ipNetworkSegments = new List<IPNetworkSegment>(); 
            foreach (var ipAddress in ipAddresses)
            {
                IPNetwork ipnetwork = IPNetwork.Parse(ipAddress);
                var ipNetworkSegment = new IPNetworkSegment
                {
                    Cidr = ipnetwork.Cidr,
                    FirstUsable = ipnetwork.FirstUsable,
                    LastUsable = ipnetwork.LastUsable,
                    LongFirstUsable = IPToLong(ipnetwork.FirstUsable),
                    LongLastUsable = IPToLong(ipnetwork.LastUsable),
                };
                ipNetworkSegments.Add(ipNetworkSegment);
            }
            return ipNetworkSegments;
        }

        private long IPToLong(IPAddress ip)
        {
            long result = 0;
            byte[] ipAdds = ip.GetAddressBytes();
            foreach (byte b in ipAdds)
            {
                result <<= 8; 
                result |= b & (uint)0xff; 
            }
            return result;
        }
    }
}
