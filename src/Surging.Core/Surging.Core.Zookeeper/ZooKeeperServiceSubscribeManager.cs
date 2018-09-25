using Surging.Core.CPlatform.Runtime.Client.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using Surging.Core.CPlatform.Runtime.Client;
using System.Threading.Tasks;
using Surging.Core.Zookeeper.Configurations;
using org.apache.zookeeper;
using Surging.Core.CPlatform.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Linq;
using Surging.Core.Zookeeper.WatcherProvider;

namespace Surging.Core.Zookeeper
{
    public class ZooKeeperServiceSubscribeManager : ServiceSubscribeManagerBase, IDisposable
    {
        private ZooKeeper _zooKeeper;
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly IServiceSubscriberFactory _serviceSubscriberFactory;
        private ServiceSubscriber[] _subscribers;
        private readonly ILogger<ZooKeeperServiceSubscribeManager> _logger;
        private readonly ManualResetEvent _connectionWait = new ManualResetEvent(false);

        public ZooKeeperServiceSubscribeManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IServiceSubscriberFactory serviceSubscriberFactory,
            ILogger<ZooKeeperServiceSubscribeManager> logger) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serviceSubscriberFactory = serviceSubscriberFactory;
            _serializer = serializer;
            _logger = logger;
            CreateZooKeeper().Wait();
            EnterSubscribers().Wait();
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
        /// 清空所有的服务订阅者。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有服务订阅配置。");
            var path = _configInfo.SubscriberPath;
            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var index = 0;
            while (childrens.Count() >1)
            {
                var nodePath = "/" + string.Join("/", childrens);

                if (await _zooKeeper.existsAsync(nodePath) != null)
                {
                    var result = await _zooKeeper.getChildrenAsync(nodePath);
                    if (result?.Children != null)
                    {
                        foreach (var child in result.Children)
                        {
                            var childPath = $"{nodePath}/{child}";
                            if (_logger.IsEnabled(LogLevel.Debug))
                                _logger.LogDebug($"准备删除：{childPath}。");
                            await _zooKeeper.deleteAsync(childPath);
                        }
                    }
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"准备删除：{nodePath}。");
                    await _zooKeeper.deleteAsync(nodePath);
                }
                index++;
                childrens = childrens.Take(childrens.Length - index).ToArray();
            }
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("路由配置清空完成。");
        }

        /// <summary>
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="routes">服务订阅者集合。</param>
        /// <returns>一个任务。</returns>
        protected override async Task SetSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> subscribers)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加服务订阅者。");
            await CreateSubdirectory(_configInfo.SubscriberPath);

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
                    await _zooKeeper.deleteAsync(nodePath);
                }
            }

            foreach (var serviceSubscriber in subscribers)
            {
                var nodePath = $"{path}{serviceSubscriber.ServiceDescriptor.Id}";
                var nodeData = _serializer.Serialize(serviceSubscriber);
                if (await _zooKeeper.existsAsync(nodePath) == null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"节点：{nodePath}不存在将进行创建。");

                    await _zooKeeper.createAsync(nodePath, nodeData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"将更新节点：{nodePath}的数据。");

                    var onlineData = (await _zooKeeper.getDataAsync(nodePath)).Data;
                    if (!DataEquals(nodeData, onlineData))
                        await _zooKeeper.setDataAsync(nodePath, nodeData);
                }
            }
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("服务订阅者添加成功。");
        }

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

        private async Task CreateZooKeeper()
        {
            if (_zooKeeper != null)
                await _zooKeeper.closeAsync();
            _zooKeeper = new ZooKeeper(_configInfo.ConnectionString, (int)_configInfo.SessionTimeout.TotalMilliseconds
               , new ReconnectionWatcher(
                    () =>
                    {
                        _connectionWait.Set();
                    },
                    () =>
                    {
                        _connectionWait.Close();
                    },
                    async () =>
                    {
                        _connectionWait.Reset();
                        await CreateZooKeeper();
                    }));

        }

        private async Task CreateSubdirectory(string path)
        {
            _connectionWait.WaitOne();
            if (await _zooKeeper.existsAsync(path) != null)
                return;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"节点{path}不存在，将进行创建。");

            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = "/";

            foreach (var children in childrens)
            {
                nodePath += children;
                if (await _zooKeeper.existsAsync(nodePath) == null)
                {
                    await _zooKeeper.createAsync(nodePath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                nodePath += "/";
            }
        }

        private async Task<ServiceSubscriber> GetSubscriber(byte[] data)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备转换服务订阅者，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceSubscriberDescriptor>(data);
            return (await _serviceSubscriberFactory.CreateServiceSubscribersAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceSubscriber> GetSubscriber(string path)
        {
            ServiceSubscriber result = null;
            if (await _zooKeeper.existsAsync(path) != null)
            {
                var data = (await _zooKeeper.getDataAsync(path)).Data;
                result = await GetSubscriber(data);
            }
            return result;
        }

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

        private async Task EnterSubscribers()
        {
            if (_subscribers != null)
                return;
            _connectionWait.WaitOne();

            if (await _zooKeeper.existsAsync(_configInfo.SubscriberPath) != null)
            {
                var result = await _zooKeeper.getChildrenAsync(_configInfo.SubscriberPath);
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

        public void Dispose()
        {
            _connectionWait.Dispose();
            _zooKeeper.closeAsync().Wait();
        }
    }
}
