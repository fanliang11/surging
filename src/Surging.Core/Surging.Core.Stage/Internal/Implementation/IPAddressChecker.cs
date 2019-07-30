using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace Surging.Core.Stage.Internal.Implementation
{
    public class IPAddressChecker : IIPChecker
    {
        private readonly ConcurrentDictionary<string, ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>> _ipNetworkSegments = new ConcurrentDictionary<string, ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>>();
        private readonly ConcurrentDictionary<string, ValueTuple<List<string>, List<string>>> _ipAddresses = new ConcurrentDictionary<string, ValueTuple<List<string>, List<string>>>();
        public IPAddressChecker()
        {
            Init();
        }

        public bool IsBlackIp(IPAddress ip,string routePath)
        {
            var result =false;
            if (AppConfig.Options.AccessSetting != null)
            {
                var longIpAddress = IPToLong(ip);
                var ipNetworkSegments = GetIPNetworkSegments(routePath);
                var ipAddresses = GetIPAddresses(routePath);
                result = ipNetworkSegments.Item1.Count > 0 && ipAddresses.Item1.Count > 0;
                if (ipNetworkSegments.Item2.Count > 0|| ipAddresses.Item2.Count>0)
                {
                    result = ipNetworkSegments.Item2.Any(p => p.LongFirstUsable >= longIpAddress && p.LongLastUsable <= longIpAddress);
                    if (!result)
                        result = ipAddresses.Item2.Any(p => p == ip.ToString());
                }
                if (ipNetworkSegments.Item1.Count > 0  || ipAddresses.Item1.Count > 0)
                {
                    result = !ipNetworkSegments.Item1.Any(p => p.LongFirstUsable >= longIpAddress && p.LongLastUsable <= longIpAddress);
                    if (result)
                        result = !ipAddresses.Item1.Any(p => p == ip.ToString());
                }
            }
            return result;
        }

        public void Init()
        {
            var settings = AppConfig.Options.AccessSetting;
            if (settings != null)
            {
                settings.ForEach(setting =>
                {
                    if (setting.Enable)
                    {
                        var whiteList = setting.WhiteList?.Split(",");
                        var blackList = setting.BlackList?.Split(",");
                        var whiteIPNetworkSegments = GetIPNetworkSegments(whiteList);
                        var blackIPNetworkSegments = GetIPNetworkSegments(blackList);
                        _ipNetworkSegments.TryAdd(setting.RoutePath ?? "", new ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>(whiteIPNetworkSegments, blackIPNetworkSegments));
                        _ipAddresses.TryAdd(setting.RoutePath ?? "", new ValueTuple<List<string>, List<string>>(GetIPAddresses(whiteList), GetIPAddresses(blackList)));
                    }
                }
                );
            }
        }

        private List<IPNetworkSegment> GetIPNetworkSegments(IEnumerable<string> ipAddresses)
        {
            var ipNetworkSegments = new List<IPNetworkSegment>();
            if (ipAddresses != null)
            {
                var addresses = ipAddresses.Where(p => p.AsSpan().IndexOf("/") > 0);
                foreach (var ipAddress in addresses)
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
            }
            return ipNetworkSegments;
        }

        private (List<IPNetworkSegment>,List<IPNetworkSegment>) GetIPNetworkSegments(string routePath)
        {
            var whiteIPNetworkSegments = new List<IPNetworkSegment>();
            var blackIPNetworkSegments = new List<IPNetworkSegment>();
            var valueTuple = _ipNetworkSegments.GetValueOrDefault(routePath);
            if (valueTuple == (null, null))
            {
                var keys = _ipNetworkSegments.Keys.Where(p => routePath.Contains(p));
                foreach (var key in keys)
                {
                    whiteIPNetworkSegments.AddRange(_ipNetworkSegments[key].Item1);
                    blackIPNetworkSegments.AddRange(_ipNetworkSegments[key].Item2);
                }
                _ipNetworkSegments.AddOrUpdate(routePath,
                    new ValueTuple<List<IPNetworkSegment>,
                    List<IPNetworkSegment>>(whiteIPNetworkSegments, blackIPNetworkSegments),
                    (key, value) => new ValueTuple<List<IPNetworkSegment>, List<IPNetworkSegment>>(whiteIPNetworkSegments, blackIPNetworkSegments));
            }
            else
            {
                whiteIPNetworkSegments = valueTuple.Item1;
                blackIPNetworkSegments = valueTuple.Item2;
            }
            return (whiteIPNetworkSegments, blackIPNetworkSegments);
        }

        private (List<string>,List<string>) GetIPAddresses(string routePath)
        {
            var whiteIPAddresses = new List<string>();
            var blackIPAddresses = new List<string>();
            var valueTuple = _ipAddresses.GetValueOrDefault(routePath);
            if (valueTuple == (null, null))
            {
                var keys = _ipAddresses.Keys.Where(p => routePath.Contains(p));
                foreach (var key in keys)
                {
                    whiteIPAddresses.AddRange(_ipAddresses[key].Item1);
                    blackIPAddresses.AddRange(_ipAddresses[key].Item2);
                }
                _ipAddresses.AddOrUpdate(routePath,
                    new ValueTuple<List<string>,
                    List<string>>(whiteIPAddresses, blackIPAddresses),
                    (key, value) => new ValueTuple<List<string>, List<string>>(whiteIPAddresses, blackIPAddresses));
            }
            else
            {
                whiteIPAddresses = valueTuple.Item1;
                blackIPAddresses = valueTuple.Item2;
            }
            return (whiteIPAddresses, blackIPAddresses);
        }

        private List<string> GetIPAddresses(IEnumerable<string> ipAddresses)
        {
            var result = new List<string>();
            if (ipAddresses != null)
                result = ipAddresses.Where(p => p.AsSpan().IndexOf("/") < 0).ToList();
            return result;
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
