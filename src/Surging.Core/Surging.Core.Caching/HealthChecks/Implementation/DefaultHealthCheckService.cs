using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Caching.HealthChecks.Implementation
{
    /// <summary>
    /// 默认的健康检查服务
    /// </summary>
    public class DefaultHealthCheckService : IHealthCheckService, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _dictionary
        /// </summary>
        private readonly ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry> _dictionary =
    new ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry>();

        /// <summary>
        /// Defines the _serviceCacheManager
        /// </summary>
        private readonly IServiceCacheManager _serviceCacheManager;

        /// <summary>
        /// Defines the _timer
        /// </summary>
        private readonly Timer _timer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHealthCheckService"/> class.
        /// </summary>
        /// <param name="serviceCacheManager">The serviceCacheManager<see cref="IServiceCacheManager"/></param>
        public DefaultHealthCheckService(IServiceCacheManager serviceCacheManager)
        {
            var timeSpan = TimeSpan.FromSeconds(10);
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

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
        }

        /// <summary>
        /// The IsHealth
        /// </summary>
        /// <param name="address">The address<see cref="CacheEndpoint"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{bool}"/></returns>
        public ValueTask<bool> IsHealth(CacheEndpoint address, string cacheId)
        {
            MonitorEntry entry;
            return !_dictionary.TryGetValue(new ValueTuple<string, int>(address.Host, address.Port), out entry) ? new ValueTask<bool>(Check(address, cacheId)) : new ValueTask<bool>(entry.Health);
        }

        /// <summary>
        /// The MarkFailure
        /// </summary>
        /// <param name="address">The address<see cref="CacheEndpoint"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public Task MarkFailure(CacheEndpoint address, string cacheId)
        {
            return Task.Run(() =>
            {
                var entry = _dictionary.GetOrAdd(new ValueTuple<string, int>(address.Host, address.Port), k => new MonitorEntry(address, cacheId, false));
                entry.Health = false;
            });
        }

        /// <summary>
        /// The Monitor
        /// </summary>
        /// <param name="address">The address<see cref="CacheEndpoint"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        public void Monitor(CacheEndpoint address, string cacheId)
        {
            _dictionary.GetOrAdd(new ValueTuple<string, int>(address.Host, address.Port), k => new MonitorEntry(address, cacheId));
        }

        /// <summary>
        /// The Check
        /// </summary>
        /// <param name="address">The address<see cref="CacheEndpoint"/></param>
        /// <param name="id">The id<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        private async Task<bool> Check(CacheEndpoint address, string id)
        {
            return await CacheContainer.GetService<ICacheProvider>(id)
            .ConnectionAsync(address);
        }

        /// <summary>
        /// The Check
        /// </summary>
        /// <param name="entrys">The entrys<see cref="IEnumerable{MonitorEntry}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task Check(IEnumerable<MonitorEntry> entrys)
        {
            foreach (var entry in entrys)
            {
                try
                {
                    await CacheContainer.GetService<ICacheProvider>(entry.CacheId).ConnectionAsync(entry.EndPoint);
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

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="cacheEndpoints">The cacheEndpoints<see cref="IEnumerable{CacheEndpoint}"/></param>
        private void Remove(IEnumerable<CacheEndpoint> cacheEndpoints)
        {
            foreach (var cacheEndpoint in cacheEndpoints)
            {
                MonitorEntry value;
                _dictionary.TryRemove(new ValueTuple<string, int>(cacheEndpoint.Host, cacheEndpoint.Port), out value);
            }
        }

        /// <summary>
        /// The RemoveUnhealthyAddress
        /// </summary>
        /// <param name="monitorEntry">The monitorEntry<see cref="IEnumerable{MonitorEntry}"/></param>
        private void RemoveUnhealthyAddress(IEnumerable<MonitorEntry> monitorEntry)
        {
            if (monitorEntry.Any())
            {
                var addresses = monitorEntry.Select(p => p.EndPoint).ToList();
                _serviceCacheManager.RemveAddressAsync(addresses).Wait();
                addresses.ForEach(p =>
                {
                    _dictionary.TryRemove(new ValueTuple<string, int>(p.Host, p.Port), out MonitorEntry value);
                });
            }
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="MonitorEntry" />
        /// </summary>
        protected class MonitorEntry
        {
            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="MonitorEntry"/> class.
            /// </summary>
            /// <param name="address">The address<see cref="CacheEndpoint"/></param>
            /// <param name="cacheId">The cacheId<see cref="string"/></param>
            /// <param name="health">The health<see cref="bool"/></param>
            public MonitorEntry(CacheEndpoint address, string cacheId, bool health = true)
            {
                EndPoint = address;
                Health = health;
                CacheId = cacheId;
            }

            #endregion 构造函数

            #region 属性

            /// <summary>
            /// Gets or sets the CacheId
            /// </summary>
            public string CacheId { get; set; }

            /// <summary>
            /// Gets or sets the EndPoint
            /// </summary>
            public CacheEndpoint EndPoint { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether Health
            /// </summary>
            public bool Health { get; set; }

            /// <summary>
            /// Gets or sets the UnhealthyTimes
            /// </summary>
            public int UnhealthyTimes { get; set; }

            #endregion 属性
        }
    }
}