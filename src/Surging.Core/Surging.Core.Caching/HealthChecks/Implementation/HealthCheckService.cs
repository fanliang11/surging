using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public HealthCheckService(ICacheProvider cacheProvider)
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            _cacheProvider = cacheProvider;
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

        private  async Task Check(IEnumerable<MonitorEntry> entrys, int timeout)
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