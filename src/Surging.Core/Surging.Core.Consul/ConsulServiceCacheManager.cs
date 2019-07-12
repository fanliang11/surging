using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Internal;
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
    /// <summary>
    /// Defines the <see cref="ConsulServiceCacheManager" />
    /// </summary>
    public class ConsulServiceCacheManager : ServiceCacheManagerBase, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _configInfo
        /// </summary>
        private readonly ConfigInfo _configInfo;

        /// <summary>
        /// Defines the _consulClientFactory
        /// </summary>
        private readonly IConsulClientProvider _consulClientFactory;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ConsulServiceCacheManager> _logger;

        /// <summary>
        /// Defines the _manager
        /// </summary>
        private readonly IClientWatchManager _manager;

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<byte[]> _serializer;

        /// <summary>
        /// Defines the _serviceCacheFactory
        /// </summary>
        private readonly IServiceCacheFactory _serviceCacheFactory;

        /// <summary>
        /// Defines the _stringSerializer
        /// </summary>
        private readonly ISerializer<string> _stringSerializer;

        /// <summary>
        /// Defines the _serviceCaches
        /// </summary>
        private ServiceCache[] _serviceCaches;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulServiceCacheManager"/> class.
        /// </summary>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <param name="serializer">The serializer<see cref="ISerializer{byte[]}"/></param>
        /// <param name="stringSerializer">The stringSerializer<see cref="ISerializer{string}"/></param>
        /// <param name="manager">The manager<see cref="IClientWatchManager"/></param>
        /// <param name="serviceCacheFactory">The serviceCacheFactory<see cref="IServiceCacheFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ConsulServiceCacheManager}"/></param>
        /// <param name="consulClientFactory">The consulClientFactory<see cref="IConsulClientProvider"/></param>
        public ConsulServiceCacheManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
        ISerializer<string> stringSerializer, IClientWatchManager manager, IServiceCacheFactory serviceCacheFactory,
        ILogger<ConsulServiceCacheManager> logger, IConsulClientProvider consulClientFactory) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceCacheFactory = serviceCacheFactory;
            _consulClientFactory = consulClientFactory;
            _logger = logger;
            _manager = manager;
            EnterCaches().Wait();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ClearAsync
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task ClearAsync()
        {
            var clients = await _consulClientFactory.GetClients();
            foreach (var client in clients)
            {
                // 根据前缀获取consul结果
                var queryResult = await client.KV.List(_configInfo.CachePath);
                var response = queryResult.Response;
                if (response != null)
                {
                    foreach (var result in response)
                    {
                        await client.KV.DeleteCAS(result);
                    }
                }
            }
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// The GetCachesAsync
        /// </summary>
        /// <returns>The <see cref="Task{IEnumerable{ServiceCache}}"/></returns>
        public override async Task<IEnumerable<ServiceCache>> GetCachesAsync()
        {
            await EnterCaches();
            return _serviceCaches;
        }

        /// <summary>
        /// The RemveAddressAsync
        /// </summary>
        /// <param name="endpoints">The endpoints<see cref="IEnumerable{CacheEndpoint}"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The SetCachesAsync
        /// </summary>
        /// <param name="caches">The caches<see cref="IEnumerable{ServiceCache}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task SetCachesAsync(IEnumerable<ServiceCache> caches)
        {
            var serviceCaches = await GetCaches(caches.Select(p => $"{ _configInfo.CachePath}{p.CacheDescriptor.Id}"));

            await RemoveCachesAsync(caches);
            await base.SetCachesAsync(caches);
        }

        /// <summary>
        /// The SetCachesAsync
        /// </summary>
        /// <param name="cacheDescriptors">The cacheDescriptors<see cref="IEnumerable{ServiceCacheDescriptor}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task SetCachesAsync(IEnumerable<ServiceCacheDescriptor> cacheDescriptors)
        {
            var clients = await _consulClientFactory.GetClients();
            foreach (var client in clients)
            {
                foreach (var cacheDescriptor in cacheDescriptors)
                {
                    var nodeData = _serializer.Serialize(cacheDescriptor);
                    var keyValuePair = new KVPair($"{_configInfo.CachePath}{cacheDescriptor.CacheDescriptor.Id}") { Value = nodeData };
                    await client.KV.Put(keyValuePair);
                }
            }
        }

        /// <summary>
        /// The DataEquals
        /// </summary>
        /// <param name="data1">The data1<see cref="IReadOnlyList{byte}"/></param>
        /// <param name="data2">The data2<see cref="IReadOnlyList{byte}"/></param>
        /// <returns>The <see cref="bool"/></returns>
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

        /// <summary>
        /// The ChildrenChange
        /// </summary>
        /// <param name="oldChildrens">The oldChildrens<see cref="string[]"/></param>
        /// <param name="newChildrens">The newChildrens<see cref="string[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The ConvertPaths
        /// </summary>
        /// <param name="datas">The datas<see cref="string[]"/></param>
        /// <returns>The <see cref="Task{string[]}"/></returns>
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

        /// <summary>
        /// The EnterCaches
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task EnterCaches()
        {
            if (_serviceCaches != null && _serviceCaches.Length > 0)
                return;
            var client = await GetConsulClient();
            var watcher = new ChildrenMonitorWatcher(GetConsulClient, _manager, _configInfo.CachePath,
               async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens),
                 (result) => ConvertPaths(result).Result);
            if (client.KV.Keys(_configInfo.CachePath).Result.Response?.Count() > 0)
            {
                var result = await client.GetChildrenAsync(_configInfo.CachePath);
                var keys = await client.KV.Keys(_configInfo.CachePath);
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

        /// <summary>
        /// The GetCache
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="Task{ServiceCache}"/></returns>
        private async Task<ServiceCache> GetCache(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换服务缓存，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceCacheDescriptor>(data);
            return (await _serviceCacheFactory.CreateServiceCachesAsync(new[] { descriptor })).First();
        }

        /// <summary>
        /// The GetCache
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Task{ServiceCache}"/></returns>
        private async Task<ServiceCache> GetCache(string path)
        {
            ServiceCache result = null;
            var client = await GetConsulClient();
            var watcher = new NodeMonitorWatcher(GetConsulClient, _manager, path,
                 async (oldData, newData) => await NodeChange(oldData, newData), null);
            var queryResult = await client.KV.Keys(path);
            if (queryResult.Response != null)
            {
                var data = (await client.GetDataAsync(path));
                if (data != null)
                {
                    watcher.SetCurrentData(data);
                    result = await GetCache(data);
                }
            }
            return result;
        }

        /// <summary>
        /// The GetCacheData
        /// </summary>
        /// <param name="data">The data<see cref="string"/></param>
        /// <returns>The <see cref="Task{ServiceCache}"/></returns>
        private async Task<ServiceCache> GetCacheData(string data)
        {
            if (data == null)
                return null;

            var descriptor = _stringSerializer.Deserialize(data, typeof(ServiceCacheDescriptor)) as ServiceCacheDescriptor;
            return (await _serviceCacheFactory.CreateServiceCachesAsync(new[] { descriptor })).First();
        }

        /// <summary>
        /// The GetCacheDatas
        /// </summary>
        /// <param name="caches">The caches<see cref="string[]"/></param>
        /// <returns>The <see cref="Task{ServiceCache[]}"/></returns>
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

        /// <summary>
        /// The GetCaches
        /// </summary>
        /// <param name="childrens">The childrens<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Task{ServiceCache[]}"/></returns>
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

        /// <summary>
        /// The GetConsulClient
        /// </summary>
        /// <returns>The <see cref="ValueTask{ConsulClient}"/></returns>
        private async ValueTask<ConsulClient> GetConsulClient()
        {
            var client = await _consulClientFactory.GetClient();
            return client;
        }

        /// <summary>
        /// The NodeChange
        /// </summary>
        /// <param name="oldData">The oldData<see cref="byte[]"/></param>
        /// <param name="newData">The newData<see cref="byte[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The RemoveCachesAsync
        /// </summary>
        /// <param name="caches">The caches<see cref="IEnumerable{ServiceCache}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task RemoveCachesAsync(IEnumerable<ServiceCache> caches)
        {
            var path = _configInfo.CachePath;
            caches = caches.ToArray();

            if (_serviceCaches != null)
            {
                var clients = await _consulClientFactory.GetClients();
                foreach (var client in clients)
                {
                    var deletedCacheIds = caches.Select(i => i.CacheDescriptor.Id).ToArray();
                    foreach (var deletedCacheId in deletedCacheIds)
                    {
                        var nodePath = $"{path}{deletedCacheId}";
                        await client.KV.Delete(nodePath);
                    }
                }
            }
        }

        #endregion 方法
    }
}