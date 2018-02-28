using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform.Cache;

namespace Surging.Core.Caching.HealthChecks.Implementation
{
    public class HealthCheckService : IHealthCheckService, IDisposable
    {
         private readonly Timer _timer;
        private readonly ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry> _dictionary =
    new ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry>();
        private readonly ICacheProvider _cacheProvider;
        private readonly IServiceCacheManager _serviceCacheManager;


        public HealthCheckService(ICacheProvider cacheProvider, IServiceCacheManager serviceCacheManager)
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            _cacheProvider = cacheProvider;
            _serviceCacheManager = serviceCacheManager;
            _timer = new Timer(async s =>
            {
                await Check(_dictionary.ToArray().Select(i => i.Value));
                RemoveUnhealthyAddress(_dictionary.ToArray().Select(i => i.Value).Where(m => m.UnhealthyTimes >= 6));
            }, null, timeSpan, timeSpan);

            //去除监控。
            _serviceCacheManager.Removed += (s, e) =>
            {
                Remove(e.Cache.CacheEndpoint);
            };
            //重新监控。
            _serviceCacheManager.Created += async (s, e) =>
            {
                var keys = e.Cache.CacheEndpoint.Select(cacheEndpoint =>
                {
                    return new ValueTuple<string, int>(cacheEndpoint.Host, cacheEndpoint.Port);
                });
                await Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value));
            };
            //重新监控。
            _serviceCacheManager.Changed += async (s, e) =>
            {
                var keys = e.Cache.CacheEndpoint.Select(cacheEndpoint =>
                {
                    return new ValueTuple<string, int>(cacheEndpoint.Host, cacheEndpoint.Port);
                });
                await Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value));
            };
        }

        public ValueTask<bool> IsHealth(CacheEndpoint address)
        {
            MonitorEntry entry;
            return !_dictionary.TryGetValue(new ValueTuple<string, int>(address.Host, address.Port), out entry) ? new ValueTask<bool>(Check(address)) : new ValueTask<bool>(entry.Health);
        }

        public Task MarkFailure(CacheEndpoint address)
        {
            return Task.Run(() =>
            {
                var entry = _dictionary.GetOrAdd(new ValueTuple<string, int>(address.Host, address.Port), k => new MonitorEntry(address, false));
                entry.Health = false;
            });
        }

        public void Monitor(CacheEndpoint address)
        {
            _dictionary.GetOrAdd(new ValueTuple<string, int>(address.Host, address.Port), k => new MonitorEntry(address));
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private async Task<bool> Check(CacheEndpoint address)
        { 
            return await _cacheProvider.ConnectionAsync(address);
        }

        private  async Task Check(IEnumerable<MonitorEntry> entrys)
        {
            foreach (var entry in entrys)
            {
                try
                {
                    await _cacheProvider.ConnectionAsync(entry.EndPoint);
                    entry.UnhealthyTimes = 0;
                    entry.Health = true;
                }
                catch
                {
                    entry.UnhealthyTimes++;
                    entry.Health = false;
                }
            }
        }

        private void Remove(IEnumerable<CacheEndpoint> cacheEndpoints)
        {
            foreach (var cacheEndpoint in cacheEndpoints)
            {
                MonitorEntry value; 
                _dictionary.TryRemove(new ValueTuple<string, int>(cacheEndpoint.Host, cacheEndpoint.Port), out value);
            }
        }

        private void RemoveUnhealthyAddress(IEnumerable<MonitorEntry> monitorEntry)
        {
            if (monitorEntry.Any())
            {
                var addresses = monitorEntry.Select(p =>p.EndPoint).ToList();
                _serviceCacheManager.RemveAddressAsync(addresses).Wait();
                addresses.ForEach(p => {
                     
                    _dictionary.TryRemove(new ValueTuple<string, int>(p.Host, p.Port), out MonitorEntry value);
                });

            }
        }


        #region Help Class

        protected class MonitorEntry
        {
            public MonitorEntry(CacheEndpoint address, bool health = true)
            {
                EndPoint = address;
                Health = health;

            }

            public int UnhealthyTimes { get; set; }

            public CacheEndpoint EndPoint { get; set; }
            public bool Health { get; set; }
        }

        #endregion Help Class
    }
}