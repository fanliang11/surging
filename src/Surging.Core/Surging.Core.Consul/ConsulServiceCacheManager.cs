using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.Consul.WatcherProvider.Implementation;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Cache.Implementation;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul
{
    public class ConsulServiceCacheManager : ServiceCacheManagerBase, IDisposable
    {
        private readonly ConsulClient _consul;
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ILogger<ConsulServiceCacheManager> _logger;
        private readonly IClientWatchManager _manager;
        private ServiceCache[] _serviceCaches;
        private readonly IServiceCacheFactory _serviceCacheFactory;
        private readonly ISerializer<string> _stringSerializer; 

        public ConsulServiceCacheManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
        ISerializer<string> stringSerializer, IClientWatchManager manager, IServiceCacheFactory serviceCacheFactory,
        ILogger<ConsulServiceCacheManager> logger) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceCacheFactory = serviceCacheFactory;
            _logger = logger;
            _manager = manager; 
            _consul = new ConsulClient(config =>
            {
                config.Address = new Uri($"http://{configInfo.Host}:{configInfo.Port}");
            }, null, h => { h.UseProxy = false; h.Proxy = null; });
            EnterCaches().Wait();
        }

        public override async Task ClearAsync()
        {

            var queryResult = await _consul.KV.List(_configInfo.CachePath);

            var response = queryResult.Response;
            if (response != null)
            {
                foreach (var result in response)
                {
                    await _consul.KV.DeleteCAS(result);
                }
            }
        }

        public void Dispose()
        {
            _consul.Dispose();
        }

        public override async Task SetCachesAsync(IEnumerable<ServiceCache> caches)
        {
            var serviceCaches = await GetCaches(caches.Select(p => $"{ _configInfo.CachePath}{p.CacheDescriptor.Id}"));
            foreach (var cache in caches)
            {
                var serviceCache = serviceCaches.Where(p => p.CacheDescriptor.Id == cache.CacheDescriptor.Id).FirstOrDefault();
                if (serviceCache != null)
                {
                    cache.CacheEndpoint = serviceCache.CacheEndpoint.Concat(
                      cache.CacheEndpoint.Except(serviceCache.CacheEndpoint));
                }
            }
            await RemoveExceptCachesAsync(caches);
            await base.SetCachesAsync(caches);
        }

        public override async Task<IEnumerable<ServiceCache>> GetCachesAsync()
        {
            await EnterCaches();
            return _serviceCaches;
        }
        
        public override async Task RemveAddressAsync(IEnumerable<CacheEndpoint> endpoints)
        {
            var caches = await GetCachesAsync();
            try
            {
                foreach (var cache in caches)
                {
                    cache.CacheEndpoint = cache.CacheEndpoint.Except(endpoints);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            await base.SetCachesAsync(caches);
        }

        public override async Task SetCachesAsync(IEnumerable<ServiceCacheDescriptor> cacheDescriptors)
        {
            foreach (var cacheDescriptor in cacheDescriptors)
            {
                var nodeData = _serializer.Serialize(cacheDescriptor);
                var keyValuePair = new KVPair($"{_configInfo.CachePath}{cacheDescriptor.CacheDescriptor.Id}") { Value = nodeData };
                await _consul.KV.Put(keyValuePair);
            }
        }

        private async Task<ServiceCache[]> GetCaches(IEnumerable<string> childrens)
        {

            childrens = childrens.ToArray();
            var caches = new List<ServiceCache>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取缓存信息。");

                var cache = await GetCache(children);
                if (cache != null)
                    caches.Add(cache);
            }
            return caches.ToArray();
        }

        private async Task<ServiceCache> GetCache(string path)
        {
            ServiceCache result = null;
            var watcher = new NodeMonitorWatcher(_consul, _manager, path,
                 async (oldData, newData) => await NodeChange(oldData, newData),null);
            var queryResult = await _consul.KV.Keys(path);
            if (queryResult.Response != null)
            {
                var data = (await _consul.GetDataAsync(path));
                if (data != null)
                {
                    watcher.SetCurrentData(data);
                    result = await GetCache(data);
                }
            }
            return result;
        }

        #region 私有方法

        private async Task RemoveExceptCachesAsync(IEnumerable<ServiceCache> caches)
        {
            var path = _configInfo.CachePath;
            caches = caches.ToArray();

            if (_serviceCaches != null)
            {
                var oldCacheIds = _serviceCaches.Select(i => i.CacheDescriptor.Id).ToArray();
                var newCacheIds = _serviceCaches.Select(i => i.CacheDescriptor.Id).ToArray();
                var deletedCacheIds = oldCacheIds.Except(newCacheIds).ToArray();
                foreach (var deletedCacheId in deletedCacheIds)
                {
                    var endpoints = _serviceCaches.Where(p => p.CacheDescriptor.Id == deletedCacheId).Select(p => p.CacheEndpoint).FirstOrDefault();
                    var nodePath = $"{path}{deletedCacheId}";
                    foreach (var endpoint in endpoints)
                    {
                        if (caches.Any(p => p.CacheEndpoint.Select(a => a.ToString()).Contains(endpoint.ToString())))
                            await _consul.KV.Delete(nodePath);
                    }
                }
            }
        }

        private async Task EnterCaches()
        {
            if (_serviceCaches != null && _serviceCaches.Length > 0)
                return; 
             var watcher = new ChildrenMonitorWatcher(_consul, _manager, _configInfo.CachePath,
                async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens),
                  (result) => ConvertPaths(result).Result);
            if (_consul.KV.Keys(_configInfo.CachePath).Result.Response?.Count() > 0)
            {
                var result = await _consul.GetChildrenAsync(_configInfo.CachePath);
                var keys = await _consul.KV.Keys(_configInfo.CachePath);
                var childrens = result; 
                watcher.SetCurrentData(ConvertPaths(childrens).Result.Select(key => $"{_configInfo.CachePath}{key}").ToArray());
                _serviceCaches = await GetCaches(keys.Response);
            }
            else
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
                    _logger.LogWarning($"无法获取缓存信息，因为节点：{_configInfo.CachePath}，不存在。");
                _serviceCaches = new ServiceCache[0];
            }
        }

        private static bool DataEquals(IReadOnlyList<byte> data1, IReadOnlyList<byte> data2)
        {
            if (data1.Count != data2.Count)
                return false;
            for (var i = 0; i < data1.Count; i++)
            {
                var b1 = data1[i];
                var b2 = data2[i];
                if (b1 != b2)
                    return false;
            }
            return true;
        }

        private async Task<ServiceCache> GetCache(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换服务缓存，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceCacheDescriptor>(data);
            return (await _serviceCacheFactory.CreateServiceCachesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceCache> GetCacheData(string data)
        {
            if (data == null)
                return null;

            var descriptor = _stringSerializer.Deserialize(data, typeof(ServiceCacheDescriptor)) as ServiceCacheDescriptor;
            return (await _serviceCacheFactory.CreateServiceCachesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceCache[]> GetCacheDatas(string[] caches)
        {
            List<ServiceCache> serviceCaches = new List<ServiceCache>();
            foreach (var cache in caches)
            {
                var serviceCache = await GetCacheData(cache);
                serviceCaches.Add(serviceCache);
            }
            return serviceCaches.ToArray();
        }

        private async Task<string[]> ConvertPaths(string[] datas)
        {
            List<string> paths = new List<string>();
            foreach (var data in datas)
            {
                var result = await GetCacheData(data);
                var serviceId = result?.CacheDescriptor.Id;
                if (!string.IsNullOrEmpty(serviceId))
                    paths.Add(serviceId);
            }
            return paths.ToArray();
        }

        private async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newCache = await GetCache(newData);
            //得到旧的缓存。
            var oldCache = _serviceCaches.FirstOrDefault(i => i.CacheDescriptor.Id == newCache.CacheDescriptor.Id);

            lock (_serviceCaches)
            {
                //删除旧缓存，并添加上新的缓存。
                _serviceCaches =
                    _serviceCaches
                        .Where(i => i.CacheDescriptor.Id != newCache.CacheDescriptor.Id)
                        .Concat(new[] { newCache }).ToArray();
            }

            if (newCache == null)
                //触发删除事件。
                OnRemoved(new ServiceCacheEventArgs(oldCache));

            else if (oldCache == null)
                OnCreated(new ServiceCacheEventArgs(newCache));

            else 
            //触发缓存变更事件。
            OnChanged(new ServiceCacheChangedEventArgs(newCache, oldCache));
        }

        private async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"最新的节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"旧的节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"需要被删除的服务缓存节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.LogDebug($"需要被添加的服务缓存节点：{string.Join(",", createdChildrens)}");

            //获取新增的缓存信息。
            var newCaches = (await GetCaches(createdChildrens)).ToArray();

            var caches = _serviceCaches.ToArray();
            lock (_serviceCaches)
            {
                _serviceCaches = _serviceCaches
                    //删除无效的缓存节点。
                    .Where(i => !deletedChildrens.Contains($"{_configInfo.CachePath}{i.CacheDescriptor.Id}"))
                    //连接上新的缓存。
                    .Concat(newCaches)
                    .ToArray();
            }
            //需要删除的缓存集合。
            var deletedCaches = caches.Where(i => deletedChildrens.Contains($"{_configInfo.CachePath}{i.CacheDescriptor.Id}")).ToArray();
            //触发删除事件。
            OnRemoved(deletedCaches.Select(cache => new ServiceCacheEventArgs(cache)).ToArray());

            //触发缓存被创建事件。
            OnCreated(newCaches.Select(cache => new ServiceCacheEventArgs(cache)).ToArray());

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.LogInformation("缓存数据更新成功。");
        }

        #endregion
    }
}
