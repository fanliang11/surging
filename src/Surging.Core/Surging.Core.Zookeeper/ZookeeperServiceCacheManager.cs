using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Cache.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.Zookeeper.Configurations;
using Surging.Core.Zookeeper.Internal;
using Surging.Core.Zookeeper.WatcherProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper
{
    /// <summary>
    /// Defines the <see cref="ZookeeperServiceCacheManager" />
    /// </summary>
    public class ZookeeperServiceCacheManager : ServiceCacheManagerBase, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _configInfo
        /// </summary>
        private readonly ConfigInfo _configInfo;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ZookeeperServiceCacheManager> _logger;

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
        /// Defines the _zookeeperClientProvider
        /// </summary>
        private readonly IZookeeperClientProvider _zookeeperClientProvider;

        /// <summary>
        /// Defines the _serviceCaches
        /// </summary>
        private ServiceCache[] _serviceCaches;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ZookeeperServiceCacheManager"/> class.
        /// </summary>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <param name="serializer">The serializer<see cref="ISerializer{byte[]}"/></param>
        /// <param name="stringSerializer">The stringSerializer<see cref="ISerializer{string}"/></param>
        /// <param name="serviceCacheFactory">The serviceCacheFactory<see cref="IServiceCacheFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ZookeeperServiceCacheManager}"/></param>
        /// <param name="zookeeperClientProvider">The zookeeperClientProvider<see cref="IZookeeperClientProvider"/></param>
        public ZookeeperServiceCacheManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
        ISerializer<string> stringSerializer, IServiceCacheFactory serviceCacheFactory,
        ILogger<ZookeeperServiceCacheManager> logger, IZookeeperClientProvider zookeeperClientProvider) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceCacheFactory = serviceCacheFactory;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
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
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有缓存配置。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                var path = _configInfo.CachePath;
                var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var index = 0;
                while (childrens.Count() > 1)
                {
                    var nodePath = "/" + string.Join("/", childrens);

                    if (await zooKeeper.Item2.existsAsync(nodePath) != null)
                    {
                        var result = await zooKeeper.Item2.getChildrenAsync(nodePath);
                        if (result?.Children != null)
                        {
                            foreach (var child in result.Children)
                            {
                                var childPath = $"{nodePath}/{child}";
                                if (_logger.IsEnabled(LogLevel.Debug))
                                    _logger.LogDebug($"准备删除：{childPath}。");
                                await zooKeeper.Item2.deleteAsync(childPath);
                            }
                        }
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"准备删除：{nodePath}。");
                        await zooKeeper.Item2.deleteAsync(nodePath);
                    }
                    index++;
                    childrens = childrens.Take(childrens.Length - index).ToArray();
                }
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("服务缓存配置清空完成。");
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
            var serviceCaches = await GetCaches(caches.Select(p => p.CacheDescriptor.Id));
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
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加服务命令。");
            var path = _configInfo.CachePath;
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                await CreateSubdirectory(zooKeeper, _configInfo.CachePath);
                if (!path.EndsWith("/"))
                    path += "/";

                cacheDescriptors = cacheDescriptors.ToArray();

                foreach (var cacheDescriptor in cacheDescriptors)
                {
                    var nodePath = $"{path}{cacheDescriptor.CacheDescriptor.Id}";
                    var nodeData = _serializer.Serialize(cacheDescriptor);
                    if (await zooKeeper.Item2.existsAsync(nodePath) == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"节点：{nodePath}不存在将进行创建。");

                        await zooKeeper.Item2.createAsync(nodePath, nodeData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                    }
                    else
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"将更新节点：{nodePath}的数据。");

                        var onlineData = (await zooKeeper.Item2.getDataAsync(nodePath)).Data;
                        if (!DataEquals(nodeData, onlineData))
                            await zooKeeper.Item2.setDataAsync(nodePath, nodeData);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("服务缓存添加成功。");
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

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被删除的服务缓存节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被添加的服务缓存节点：{string.Join(",", createdChildrens)}");

            //获取新增的缓存信息。
            var newCaches = (await GetCaches(createdChildrens)).ToArray();

            var caches = _serviceCaches.ToArray();
            lock (_serviceCaches)
            {
                _serviceCaches = _serviceCaches
                    //删除无效的缓存节点。
                    .Where(i => !deletedChildrens.Contains(i.CacheDescriptor.Id))
                    //连接上新的缓存。
                    .Concat(newCaches)
                    .ToArray();
            }
            //需要删除的缓存集合。
            var deletedCaches = caches.Where(i => deletedChildrens.Contains(i.CacheDescriptor.Id)).ToArray();
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
        /// The CreateSubdirectory
        /// </summary>
        /// <param name="zooKeeper">The zooKeeper<see cref="(ManualResetEvent, ZooKeeper)"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task CreateSubdirectory((ManualResetEvent, ZooKeeper) zooKeeper, string path)
        {
            zooKeeper.Item1.WaitOne();
            if (await zooKeeper.Item2.existsAsync(path) != null)
                return;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"节点{path}不存在，将进行创建。");

            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = "/";

            foreach (var children in childrens)
            {
                nodePath += children;
                if (await zooKeeper.Item2.existsAsync(nodePath) == null)
                {
                    await zooKeeper.Item2.createAsync(nodePath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                nodePath += "/";
            }
        }

        /// <summary>
        /// The EnterCaches
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task EnterCaches()
        {
            if (_serviceCaches != null)
                return;
            var zooKeeper = await GetZooKeeper();
            zooKeeper.Item1.WaitOne();
            var path = _configInfo.CachePath;
            var watcher = new ChildrenMonitorWatcher(GetZooKeeper, path,
                async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
            if (await zooKeeper.Item2.existsAsync(path, watcher) != null)
            {
                var result = await zooKeeper.Item2.getChildrenAsync(path, watcher);
                var childrens = result.Children.ToArray();
                watcher.SetCurrentData(childrens);
                _serviceCaches = await GetCaches(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取服务缓存信息，因为节点：{path}，不存在。");
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
            var zooKeeper = await GetZooKeeper();
            var watcher = new NodeMonitorWatcher(GetZooKeeper, path,
                 async (oldData, newData) => await NodeChange(oldData, newData));
            if (await zooKeeper.Item2.existsAsync(path) != null)
            {
                var data = (await zooKeeper.Item2.getDataAsync(path, watcher)).Data;
                watcher.SetCurrentData(data);
                result = await GetCache(data);
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
            var rootPath = _configInfo.CachePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var caches = new List<ServiceCache>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取缓存信息。");

                var nodePath = $"{rootPath}{children}";
                var cache = await GetCache(nodePath);
                if (cache != null)
                    caches.Add(cache);
            }
            return caches.ToArray();
        }

        /// <summary>
        /// The GetZooKeeper
        /// </summary>
        /// <returns>The <see cref="ValueTask{(ManualResetEvent, ZooKeeper)}"/></returns>
        private async ValueTask<(ManualResetEvent, ZooKeeper)> GetZooKeeper()
        {
            var zooKeeper = await _zookeeperClientProvider.GetZooKeeper();
            return zooKeeper;
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
            if (!path.EndsWith("/"))
                path += "/";
            caches = caches.ToArray();
            if (_serviceCaches != null)
            {
                var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
                foreach (var zooKeeper in zooKeepers)
                {
                    var deletedCacheIds = caches.Select(i => i.CacheDescriptor.Id).ToArray();
                    foreach (var deletedCacheId in deletedCacheIds)
                    {
                        var nodePath = $"{path}{deletedCacheId}";
                        await zooKeeper.Item2.deleteAsync(nodePath);
                    }
                }
            }
        }

        #endregion 方法
    }
}