using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Internal;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.Consul.WatcherProvider.Implementation;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Mqtt.Implementation;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul
{
    /// <summary>
    /// Defines the <see cref="ConsulMqttServiceRouteManager" />
    /// </summary>
    public class ConsulMqttServiceRouteManager : MqttServiceRouteManagerBase, IDisposable
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
        private readonly ILogger<ConsulMqttServiceRouteManager> _logger;

        /// <summary>
        /// Defines the _manager
        /// </summary>
        private readonly IClientWatchManager _manager;

        /// <summary>
        /// Defines the _mqttServiceFactory
        /// </summary>
        private readonly IMqttServiceFactory _mqttServiceFactory;

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<byte[]> _serializer;

        /// <summary>
        /// Defines the _serviceHeartbeatManager
        /// </summary>
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        /// <summary>
        /// Defines the _stringSerializer
        /// </summary>
        private readonly ISerializer<string> _stringSerializer;

        /// <summary>
        /// Defines the _routes
        /// </summary>
        private MqttServiceRoute[] _routes;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulMqttServiceRouteManager"/> class.
        /// </summary>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <param name="serializer">The serializer<see cref="ISerializer{byte[]}"/></param>
        /// <param name="stringSerializer">The stringSerializer<see cref="ISerializer{string}"/></param>
        /// <param name="manager">The manager<see cref="IClientWatchManager"/></param>
        /// <param name="mqttServiceFactory">The mqttServiceFactory<see cref="IMqttServiceFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ConsulMqttServiceRouteManager}"/></param>
        /// <param name="serviceHeartbeatManager">The serviceHeartbeatManager<see cref="IServiceHeartbeatManager"/></param>
        /// <param name="consulClientFactory">The consulClientFactory<see cref="IConsulClientProvider"/></param>
        public ConsulMqttServiceRouteManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
       ISerializer<string> stringSerializer, IClientWatchManager manager, IMqttServiceFactory mqttServiceFactory,
       ILogger<ConsulMqttServiceRouteManager> logger, IServiceHeartbeatManager serviceHeartbeatManager,
       IConsulClientProvider consulClientFactory) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _mqttServiceFactory = mqttServiceFactory;
            _logger = logger;
            _manager = manager;
            _serviceHeartbeatManager = serviceHeartbeatManager;
            _consulClientFactory = consulClientFactory;
            EnterRoutes().Wait();
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
                //根据前缀获取consul结果
                var queryResult = await client.KV.List(_configInfo.MqttRoutePath);
                var response = queryResult.Response;
                if (response != null)
                {
                    //删除操作
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
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<MqttServiceRoute>> GetRoutesAsync()
        {
            await EnterRoutes();
            return _routes;
        }

        /// <summary>
        /// The RemoveByTopicAsync
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="IEnumerable{AddressModel}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task RemoveByTopicAsync(string topic, IEnumerable<AddressModel> endpoint)
        {
            var routes = await GetRoutesAsync();
            try
            {
                var route = routes.Where(p => p.MqttDescriptor.Topic == topic).SingleOrDefault();
                if (route != null)
                {
                    route.MqttEndpoint = route.MqttEndpoint.Except(endpoint);
                    await base.SetRoutesAsync(new MqttServiceRoute[] { route });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// The RemveAddressAsync
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="IEnumerable{AddressModel}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task RemveAddressAsync(IEnumerable<AddressModel> endpoint)
        {
            var routes = await GetRoutesAsync();
            try
            {
                foreach (var route in routes)
                {
                    route.MqttEndpoint = route.MqttEndpoint.Except(endpoint);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            await base.SetRoutesAsync(routes);
        }

        /// <summary>
        /// The SetRoutesAsync
        /// </summary>
        /// <param name="routes">The routes<see cref="IEnumerable{MqttServiceRoute}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task SetRoutesAsync(IEnumerable<MqttServiceRoute> routes)
        {
            var hostAddr = NetUtils.GetHostAddress();
            var mqttServiceRoutes = await GetRoutes(routes.Select(p => $"{ _configInfo.MqttRoutePath}{p.MqttDescriptor.Topic}"));
            foreach (var route in routes)
            {
                var mqttServiceRoute = mqttServiceRoutes.Where(p => p.MqttDescriptor.Topic == route.MqttDescriptor.Topic).FirstOrDefault();

                if (mqttServiceRoute != null)
                {
                    var addresses = mqttServiceRoute.MqttEndpoint.Concat(
                      route.MqttEndpoint.Except(mqttServiceRoute.MqttEndpoint)).ToList();

                    foreach (var address in route.MqttEndpoint)
                    {
                        addresses.Remove(addresses.Where(p => p.ToString() == address.ToString()).FirstOrDefault());
                        addresses.Add(address);
                    }
                    route.MqttEndpoint = addresses;
                }
            }

            await base.SetRoutesAsync(routes);
        }

        /// <summary>
        /// The SetRoutesAsync
        /// </summary>
        /// <param name="routes">The routes<see cref="IEnumerable{MqttServiceDescriptor}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        protected override async Task SetRoutesAsync(IEnumerable<MqttServiceDescriptor> routes)
        {
            var clients = await _consulClientFactory.GetClients();
            foreach (var client in clients)
            {
                foreach (var serviceRoute in routes)
                {
                    var nodeData = _serializer.Serialize(serviceRoute);
                    var keyValuePair = new KVPair($"{_configInfo.MqttRoutePath}{serviceRoute.MqttDescriptor.Topic}") { Value = nodeData };
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
                _logger.LogDebug($"最新的mqtt节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"旧的mqtt节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"需要被删除的mqtt路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"需要被添加的mqtt路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();

            var routes = _routes.ToArray();
            lock (_routes)
            {
                _routes = _routes
                    //删除无效的节点路由。
                    .Where(i => !deletedChildrens.Contains($"{_configInfo.MqttRoutePath}{i.MqttDescriptor.Topic}"))
                    //连接上新的路由。
                    .Concat(newRoutes)
                    .ToArray();
            }
            //需要删除的路由集合。
            var deletedRoutes = routes.Where(i => deletedChildrens.Contains($"{_configInfo.MqttRoutePath}{i.MqttDescriptor.Topic}")).ToArray();
            //触发删除事件。
            OnRemoved(deletedRoutes.Select(route => new MqttServiceRouteEventArgs(route)).ToArray());

            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new MqttServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.LogInformation("mqtt路由数据更新成功。");
        }

        /// <summary>
        /// 转化topic集合
        /// </summary>
        /// <param name="datas">信息数据集合</param>
        /// <returns>返回路径集合</returns>
        private async Task<string[]> ConvertPaths(string[] datas)
        {
            List<string> topics = new List<string>();
            foreach (var data in datas)
            {
                var result = await GetRouteData(data);
                var topic = result?.MqttDescriptor.Topic;
                if (!string.IsNullOrEmpty(topic))
                    topics.Add(topic);
            }
            return topics.ToArray();
        }

        /// <summary>
        /// The EnterRoutes
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task EnterRoutes()
        {
            if (_routes != null && _routes.Length > 0)
                return;
            Action<string[]> action = null;
            var client = await GetConsulClient();
            if (_configInfo.EnableChildrenMonitor)
            {
                var watcher = new ChildrenMonitorWatcher(GetConsulClient, _manager, _configInfo.MqttRoutePath,
             async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens),
               (result) => ConvertPaths(result).Result);
                action = currentData => watcher.SetCurrentData(currentData);
            }
            if (client.KV.Keys(_configInfo.MqttRoutePath).Result.Response?.Count() > 0)
            {
                var result = await client.GetChildrenAsync(_configInfo.MqttRoutePath);
                var keys = await client.KV.Keys(_configInfo.MqttRoutePath);
                var childrens = result;
                action?.Invoke(ConvertPaths(childrens).Result.Select(key => $"{_configInfo.MqttRoutePath}{key}").ToArray());
                _routes = await GetRoutes(keys.Response);
            }
            else
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
                    _logger.LogWarning($"无法获取路由信息，因为节点：{_configInfo.MqttRoutePath}，不存在。");
                _routes = new MqttServiceRoute[0];
            }
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
        /// The GetRoute
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="Task{MqttServiceRoute}"/></returns>
        private async Task<MqttServiceRoute> GetRoute(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换mqtt服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], MqttServiceDescriptor>(data);
            return (await _mqttServiceFactory.CreateMqttServiceRoutesAsync(new[] { descriptor })).First();
        }

        /// <summary>
        /// The GetRoute
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Task{MqttServiceRoute}"/></returns>
        private async Task<MqttServiceRoute> GetRoute(string path)
        {
            MqttServiceRoute result = null;
            var client = await GetConsulClient();
            var watcher = new NodeMonitorWatcher(GetConsulClient, _manager, path,
                async (oldData, newData) => await NodeChange(oldData, newData), tmpPath =>
                {
                    var index = tmpPath.LastIndexOf("/");
                    return _serviceHeartbeatManager.ExistsWhitelist(tmpPath.Substring(index + 1));
                });

            var queryResult = await client.KV.Keys(path);
            if (queryResult.Response != null)
            {
                var data = (await client.GetDataAsync(path));
                if (data != null)
                {
                    watcher.SetCurrentData(data);
                    result = await GetRoute(data);
                }
            }
            return result;
        }

        /// <summary>
        /// The GetRouteData
        /// </summary>
        /// <param name="data">The data<see cref="string"/></param>
        /// <returns>The <see cref="Task{MqttServiceRoute}"/></returns>
        private async Task<MqttServiceRoute> GetRouteData(string data)
        {
            if (data == null)
                return null;

            var descriptor = _stringSerializer.Deserialize(data, typeof(MqttServiceDescriptor)) as MqttServiceDescriptor;
            return (await _mqttServiceFactory.CreateMqttServiceRoutesAsync(new[] { descriptor })).First();
        }

        /// <summary>
        /// The GetRouteDatas
        /// </summary>
        /// <param name="routes">The routes<see cref="string[]"/></param>
        /// <returns>The <see cref="Task{MqttServiceRoute[]}"/></returns>
        private async Task<MqttServiceRoute[]> GetRouteDatas(string[] routes)
        {
            List<MqttServiceRoute> serviceRoutes = new List<MqttServiceRoute>();
            foreach (var route in routes)
            {
                var serviceRoute = await GetRouteData(route);
                serviceRoutes.Add(serviceRoute);
            }
            return serviceRoutes.ToArray();
        }

        /// <summary>
        /// The GetRoutes
        /// </summary>
        /// <param name="childrens">The childrens<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Task{MqttServiceRoute[]}"/></returns>
        private async Task<MqttServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {
            childrens = childrens.ToArray();
            var routes = new List<MqttServiceRoute>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取mqtt路由信息。");

                var route = await GetRoute(children);
                if (route != null)
                    routes.Add(route);
            }

            return routes.ToArray();
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

            var newRoute = await GetRoute(newData);
            //得到旧的路由。
            var oldRoute = _routes.FirstOrDefault(i => i.MqttDescriptor.Topic == newRoute.MqttDescriptor.Topic);

            lock (_routes)
            {
                //删除旧路由，并添加上新的路由。
                _routes =
                    _routes
                        .Where(i => i.MqttDescriptor.Topic != newRoute.MqttDescriptor.Topic)
                        .Concat(new[] { newRoute }).ToArray();
            }

            //触发路由变更事件。
            OnChanged(new MqttServiceRouteChangedEventArgs(newRoute, oldRoute));
        }

        /// <summary>
        /// The RemoveExceptRoutesAsync
        /// </summary>
        /// <param name="routes">The routes<see cref="IEnumerable{MqttServiceRoute}"/></param>
        /// <param name="hostAddr">The hostAddr<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task RemoveExceptRoutesAsync(IEnumerable<MqttServiceRoute> routes, AddressModel hostAddr)
        {
            routes = routes.ToArray();
            var clients = await _consulClientFactory.GetClients();
            foreach (var client in clients)
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
                            await client.KV.Delete($"{_configInfo.MqttRoutePath}{deletedRouteTopic}");
                    }
                }
            }
        }

        #endregion 方法
    }
}