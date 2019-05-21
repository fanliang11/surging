using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Mqtt.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
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
   public class ZooKeeperMqttServiceRouteManager : MqttServiceRouteManagerBase, IDisposable
    { 
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly IMqttServiceFactory _mqttServiceFactory;
        private readonly ILogger<ZooKeeperMqttServiceRouteManager> _logger;
        private MqttServiceRoute[] _routes;
        private readonly IZookeeperClientProvider _zookeeperClientProvider;

        public ZooKeeperMqttServiceRouteManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IMqttServiceFactory mqttServiceFactory,
            ILogger<ZooKeeperMqttServiceRouteManager> logger, IZookeeperClientProvider zookeeperClientProvider) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _mqttServiceFactory = mqttServiceFactory;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterRoutes().Wait();
        }


        /// <summary>
        /// 获取所有可用的mqtt服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<MqttServiceRoute>> GetRoutesAsync()
        {
            await EnterRoutes();
            return _routes;
        }

        /// <summary>
        /// 清空所有的mqtt服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有mqtt路由配置。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                var path = _configInfo.MqttRoutePath;
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
        /// 设置mqtt服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        protected override async Task SetRoutesAsync(IEnumerable<MqttServiceDescriptor> routes)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加mqtt服务路由。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                await CreateSubdirectory(zooKeeper, _configInfo.MqttRoutePath);

                var path = _configInfo.MqttRoutePath;
                if (!path.EndsWith("/"))
                    path += "/";

                routes = routes.ToArray();

                foreach (var serviceRoute in routes)
                {
                    var nodePath = $"{path}{serviceRoute.MqttDescriptor.Topic}";
                    var nodeData = _serializer.Serialize(serviceRoute);
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
                    _logger.LogInformation("mqtt服务路由添加成功。");
            }
        }

        public override async Task RemveAddressAsync(IEnumerable<AddressModel> Address)
        {
            var routes = await GetRoutesAsync();
            foreach (var route in routes)
            {
                route.MqttEndpoint = route.MqttEndpoint.Except(Address);
            }
            await base.SetRoutesAsync(routes);
        }

        public override async Task RemoveByTopicAsync(string topic, IEnumerable<AddressModel> endpoint)
        {
            var routes = await GetRoutesAsync();
            var route = routes.Where(p => p.MqttDescriptor.Topic == topic).SingleOrDefault();
            if (route != null)
            {
                route.MqttEndpoint = route.MqttEndpoint.Except(endpoint);
                await base.SetRoutesAsync(new MqttServiceRoute[] { route });
            }
        }

        public override async Task SetRoutesAsync(IEnumerable<MqttServiceRoute> routes)
        {
            var hostAddr = NetUtils.GetHostAddress();
            var serviceRoutes = await GetRoutes(routes.Select(p => p.MqttDescriptor.Topic));
            if (serviceRoutes.Count() > 0)
            {
                foreach (var route in routes)
                {
                    var serviceRoute = serviceRoutes.Where(p => p.MqttDescriptor.Topic == route.MqttDescriptor.Topic).FirstOrDefault();
                    if (serviceRoute != null)
                    {
                        var addresses = serviceRoute.MqttEndpoint.Concat(
                          route.MqttEndpoint.Except(serviceRoute.MqttEndpoint)).ToList();

                        foreach (var address in route.MqttEndpoint)
                        {
                            addresses.Remove(addresses.Where(p => p.ToString() == address.ToString()).FirstOrDefault());
                            addresses.Add(address);
                        }
                        route.MqttEndpoint = addresses;
                    }
                }
            }
            await RemoveExceptRoutesAsync(routes, hostAddr);
            await base.SetRoutesAsync(routes);
        }

        private async Task RemoveExceptRoutesAsync(IEnumerable<MqttServiceRoute> routes, AddressModel hostAddr)
        {
            var path = _configInfo.MqttRoutePath;
            if (!path.EndsWith("/"))
                path += "/";
            routes = routes.ToArray();
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                if (_routes != null)
                {
                    var oldRouteTopics = _routes.Select(i => i.MqttDescriptor.Topic).ToArray();
                    var newRouteTopics = routes.Select(i => i.MqttDescriptor.Topic).ToArray();
                    var deletedRouteTopics = oldRouteTopics.Except(newRouteTopics).ToArray();
                    foreach (var deletedRouteTopic in deletedRouteTopics)
                    {
                        var addresses = _routes.Where(p => p.MqttDescriptor.Topic == deletedRouteTopic).Select(p => p.MqttEndpoint).FirstOrDefault();
                        if (addresses.Contains(hostAddr))
                        {
                            var nodePath = $"{path}{deletedRouteTopic}";
                            await zooKeeper.Item2.deleteAsync(nodePath);
                        }
                    }
                }
            }
        }

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

        private async Task<MqttServiceRoute> GetRoute(byte[] data)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备转换mqtt服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], MqttServiceDescriptor>(data);
            return (await _mqttServiceFactory.CreateMqttServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<MqttServiceRoute> GetRoute(string path)
        {
            MqttServiceRoute result = null;

            var zooKeeper = await GetZooKeeper();
            var watcher = new NodeMonitorWatcher(GetZooKeeper, path,
                 async (oldData, newData) => await NodeChange(oldData, newData));
            if (await zooKeeper.Item2.existsAsync(path) != null)
            {
                var data = (await zooKeeper.Item2.getDataAsync(path, watcher)).Data;
                watcher.SetCurrentData(data);
                result = await GetRoute(data);
            }
            return result;
        }

        private async Task<MqttServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.MqttRoutePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var routes = new List<MqttServiceRoute>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取mqtt路由信息。");

                var nodePath = $"{rootPath}{children}";
                var route = await GetRoute(nodePath);
                if (route != null)
                    routes.Add(route);
            }

            return routes.ToArray();
        }

        private async Task EnterRoutes()
        {
            if (_routes != null)
                return;
            var zooKeeper = await GetZooKeeper();
            zooKeeper.Item1.WaitOne(); 
            var watcher = new ChildrenMonitorWatcher(GetZooKeeper, _configInfo.MqttRoutePath,
             async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
            if (await zooKeeper.Item2.existsAsync(_configInfo.MqttRoutePath, watcher) != null)
            {
                var result = await zooKeeper.Item2.getChildrenAsync(_configInfo.MqttRoutePath, watcher);
                var childrens = result.Children.ToArray();
                watcher.SetCurrentData(childrens);
                _routes = await GetRoutes(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取mqtt路由信息，因为节点：{_configInfo.RoutePath}，不存在。");
                _routes = new MqttServiceRoute[0];
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

        public async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newRoute = await GetRoute(newData);
            //得到旧的mqtt路由。
            var oldRoute = _routes.FirstOrDefault(i => i.MqttDescriptor.Topic == newRoute.MqttDescriptor.Topic);

            lock (_routes)
            {
                //删除旧mqtt路由，并添加上新的mqtt路由。
                _routes =
                    _routes
                        .Where(i => i.MqttDescriptor.Topic != newRoute.MqttDescriptor.Topic)
                        .Concat(new[] { newRoute }).ToArray();
            }

            //触发路由变更事件。
            OnChanged(new MqttServiceRouteChangedEventArgs(newRoute, oldRoute));
        }

        public async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"最新的mqtt节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"旧的mqtt节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被删除的mqtt路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被添加的mqtt路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的mqtt路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();

            var routes = _routes.ToArray();
            lock (_routes)
            {
                _routes = _routes
                    //删除无效的节点路由。
                    .Where(i => !deletedChildrens.Contains(i.MqttDescriptor.Topic))
                    //连接上新的mqtt路由。
                    .Concat(newRoutes)
                    .ToArray();
            }
            //需要删除的Topic路由集合。
            var deletedRoutes = routes.Where(i => deletedChildrens.Contains(i.MqttDescriptor.Topic)).ToArray();
            //触发删除事件。
            OnRemoved(deletedRoutes.Select(route => new MqttServiceRouteEventArgs(route)).ToArray());

            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new MqttServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("mqtt路由数据更新成功。");
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        { 
        }

        private async ValueTask<(ManualResetEvent, ZooKeeper)> GetZooKeeper()
        {
            var zooKeeper = await _zookeeperClientProvider.GetZooKeeper();
            return zooKeeper;
        }

    }
}
