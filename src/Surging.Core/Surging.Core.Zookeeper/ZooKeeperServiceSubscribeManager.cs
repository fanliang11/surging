using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Implementation;
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
    /// Defines the <see cref="ZooKeeperServiceSubscribeManager" />
    /// </summary>
    public class ZooKeeperServiceSubscribeManager : ServiceSubscribeManagerBase, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _configInfo
        /// </summary>
        private readonly ConfigInfo _configInfo;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ZooKeeperServiceSubscribeManager> _logger;

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<byte[]> _serializer;

        /// <summary>
        /// Defines the _serviceSubscriberFactory
        /// </summary>
        private readonly IServiceSubscriberFactory _serviceSubscriberFactory;

        /// <summary>
        /// Defines the _zookeeperClientProvider
        /// </summary>
        private readonly IZookeeperClientProvider _zookeeperClientProvider;

        /// <summary>
        /// Defines the _subscribers
        /// </summary>
        private ServiceSubscriber[] _subscribers;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ZooKeeperServiceSubscribeManager"/> class.
        /// </summary>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <param name="serializer">The serializer<see cref="ISerializer{byte[]}"/></param>
        /// <param name="stringSerializer">The stringSerializer<see cref="ISerializer{string}"/></param>
        /// <param name="serviceSubscriberFactory">The serviceSubscriberFactory<see cref="IServiceSubscriberFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ZooKeeperServiceSubscribeManager}"/></param>
        /// <param name="zookeeperClientProvider">The zookeeperClientProvider<see cref="IZookeeperClientProvider"/></param>
        public ZooKeeperServiceSubscribeManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IServiceSubscriberFactory serviceSubscriberFactory,
            ILogger<ZooKeeperServiceSubscribeManager> logger, IZookeeperClientProvider zookeeperClientProvider) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serviceSubscriberFactory = serviceSubscriberFactory;
            _serializer = serializer;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterSubscribers().Wait();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 清空所有的服务订阅者。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有服务订阅配置。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                var path = _configInfo.SubscriberPath;
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
                    _logger.LogInformation("路由配置清空完成。");
            }
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 获取所有可用的服务订阅者信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceSubscriber>> GetSubscribersAsync()
        {
            await EnterSubscribers();
            return _subscribers;
        }

        /// <summary>
        /// The SetSubscribersAsync
        /// </summary>
        /// <param name="subscribers">The subscribers<see cref="IEnumerable{ServiceSubscriber}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task SetSubscribersAsync(IEnumerable<ServiceSubscriber> subscribers)
        {
            var serviceSubscribers = await GetSubscribers(subscribers.Select(p => p.ServiceDescriptor.Id));
            foreach (var subscriber in subscribers)
            {
                var serviceSubscriber = serviceSubscribers.Where(p => p.ServiceDescriptor.Id == subscriber.ServiceDescriptor.Id).FirstOrDefault();
                if (serviceSubscriber != null)
                {
                    subscriber.Address = subscriber.Address.Concat(
                        subscriber.Address.Except(serviceSubscriber.Address));
                }
            }
            await base.SetSubscribersAsync(subscribers);
        }

        /// <summary>
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="subscribers">The subscribers<see cref="IEnumerable{ServiceSubscriberDescriptor}"/></param>
        /// <returns>一个任务。</returns>
        protected override async Task SetSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> subscribers)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加服务订阅者。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                await CreateSubdirectory(zooKeeper, _configInfo.SubscriberPath);

                var path = _configInfo.SubscriberPath;
                if (!path.EndsWith("/"))
                    path += "/";

                subscribers = subscribers.ToArray();

                if (_subscribers != null)
                {
                    var oldSubscriberIds = _subscribers.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var newSubscriberIds = subscribers.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var deletedSubscriberIds = oldSubscriberIds.Except(newSubscriberIds).ToArray();
                    foreach (var deletedSubscriberId in deletedSubscriberIds)
                    {
                        var nodePath = $"{path}{deletedSubscriberId}";
                        await zooKeeper.Item2.deleteAsync(nodePath);
                    }
                }

                foreach (var serviceSubscriber in subscribers)
                {
                    var nodePath = $"{path}{serviceSubscriber.ServiceDescriptor.Id}";
                    var nodeData = _serializer.Serialize(serviceSubscriber);
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
                    _logger.LogInformation("服务订阅者添加成功。");
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
        /// The EnterSubscribers
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task EnterSubscribers()
        {
            if (_subscribers != null)
                return;
            var zooKeeper = await GetZooKeeper();
            zooKeeper.Item1.WaitOne();

            if (await zooKeeper.Item2.existsAsync(_configInfo.SubscriberPath) != null)
            {
                var result = await zooKeeper.Item2.getChildrenAsync(_configInfo.SubscriberPath);
                var childrens = result.Children.ToArray();
                _subscribers = await GetSubscribers(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取订阅者信息，因为节点：{_configInfo.SubscriberPath}，不存在。");
                _subscribers = new ServiceSubscriber[0];
            }
        }

        /// <summary>
        /// The GetSubscriber
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="Task{ServiceSubscriber}"/></returns>
        private async Task<ServiceSubscriber> GetSubscriber(byte[] data)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备转换服务订阅者，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceSubscriberDescriptor>(data);
            return (await _serviceSubscriberFactory.CreateServiceSubscribersAsync(new[] { descriptor })).First();
        }

        /// <summary>
        /// The GetSubscriber
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Task{ServiceSubscriber}"/></returns>
        private async Task<ServiceSubscriber> GetSubscriber(string path)
        {
            ServiceSubscriber result = null;

            var zooKeeper = await GetZooKeeper();
            if (await zooKeeper.Item2.existsAsync(path) != null)
            {
                var data = (await zooKeeper.Item2.getDataAsync(path)).Data;
                result = await GetSubscriber(data);
            }
            return result;
        }

        /// <summary>
        /// The GetSubscribers
        /// </summary>
        /// <param name="childrens">The childrens<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Task{ServiceSubscriber[]}"/></returns>
        private async Task<ServiceSubscriber[]> GetSubscribers(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.SubscriberPath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var subscribers = new List<ServiceSubscriber>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取订阅者信息。");

                var nodePath = $"{rootPath}{children}";
                var subscriber = await GetSubscriber(nodePath);
                if (subscriber != null)
                    subscribers.Add(subscriber);
            }
            return subscribers.ToArray();
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

        #endregion 方法
    }
}