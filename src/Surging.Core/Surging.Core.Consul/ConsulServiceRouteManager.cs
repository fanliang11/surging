using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Internal;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.Consul.WatcherProvider.Implementation;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Consul
{
    public class ConsulServiceRouteManager : ServiceRouteManagerBase, IDisposable
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly IServiceRouteFactory _serviceRouteFactory;
        private readonly ILogger<ConsulServiceRouteManager> _logger;
        private readonly ISerializer<string> _stringSerializer;
        private readonly IClientWatchManager _manager;
        private ServiceRoute[] _routes;
        private readonly IConsulClientProvider _consulClientProvider;
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        public ConsulServiceRouteManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
       ISerializer<string> stringSerializer, IClientWatchManager manager, IServiceRouteFactory serviceRouteFactory,
       ILogger<ConsulServiceRouteManager> logger,
       IServiceHeartbeatManager serviceHeartbeatManager, IConsulClientProvider consulClientProvider) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceRouteFactory = serviceRouteFactory;
            _logger = logger;
            _consulClientProvider = consulClientProvider;
            _manager = manager;
            _serviceHeartbeatManager = serviceHeartbeatManager;
            EnterRoutes().Wait();
        }

        public override async Task ClearAsync()
        {
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                var queryResult = await client.KV.List(_configInfo.RoutePath);
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

        public void Dispose()
        {
        }

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            await EnterRoutes();
            return _routes;
        }

        public override async Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            var locks = await CreateLock();
            try
            {
                await _consulClientProvider.Check();
                var hostAddr = NetUtils.GetHostAddress();
                var serviceRoutes = await GetRoutes(routes.Select(p => $"{ _configInfo.RoutePath}{p.ServiceDescriptor.Id}"));
                foreach (var route in routes)
                {
                    var serviceRoute = serviceRoutes.Where(p => p.ServiceDescriptor.Id == route.ServiceDescriptor.Id).FirstOrDefault();

                    if (serviceRoute != null)
                    {
                        var addresses = serviceRoute.Address.Concat(
                          route.Address.Except(serviceRoute.Address)).ToList();

                        foreach (var address in route.Address)
                        {
                            addresses.Remove(addresses.Where(p => p.ToString() == address.ToString()).FirstOrDefault());
                            addresses.Add(address);
                        }
                        route.Address = addresses;
                    }
                }
                await RemoveExceptRoutesAsync(routes, hostAddr);
                await base.SetRoutesAsync(routes);
            }
            finally
            {
                locks.ForEach(p => p.Release());
            }
        }

        public override async Task RemveAddressAsync(IEnumerable<AddressModel> Address)
        {
            var routes = await GetRoutesAsync();
            try
            {
                foreach (var route in routes)
                {
                    route.Address = route.Address.Except(Address);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            await base.SetRoutesAsync(routes);
        }

        protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                foreach (var serviceRoute in routes)
                {
                    var nodeData = _serializer.Serialize(serviceRoute);
                    var keyValuePair = new KVPair($"{_configInfo.RoutePath}{serviceRoute.ServiceDescriptor.Id}") { Value = nodeData };
                    await client.KV.Put(keyValuePair);
                }
            }
        }

        #region 私有方法

        private async Task RemoveExceptRoutesAsync(IEnumerable<ServiceRoute> routes, AddressModel hostAddr)
        {
            routes = routes.ToArray();
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                if (_routes != null)
                {
                    var oldRouteIds = _routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var newRouteIds = routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var deletedRouteIds = oldRouteIds.Except(newRouteIds).ToArray();
                    foreach (var deletedRouteId in deletedRouteIds)
                    {
                        var addresses = _routes.Where(p => p.ServiceDescriptor.Id == deletedRouteId).Select(p => p.Address).FirstOrDefault();
                        if (addresses.Contains(hostAddr))
                            await client.KV.Delete($"{_configInfo.RoutePath}{deletedRouteId}");
                    }
                }
            }
        }

        private async Task<List<IDistributedLock>> CreateLock()
        {
            var result = new List<IDistributedLock>();
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                var distributedLock = await client.AcquireLock($"lock_{_configInfo.RoutePath}", _configInfo.LockDelay == 0 ?
                    default :
                     new CancellationTokenSource(TimeSpan.FromSeconds(_configInfo.LockDelay)).Token);
                result.Add(distributedLock);
            }
            return result;
        }

        private async Task<ServiceRoute> GetRoute(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceRouteDescriptor>(data);
            return (await _serviceRouteFactory.CreateServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceRoute[]> GetRouteDatas(string[] routes)
        {
            List<ServiceRoute> serviceRoutes = new List<ServiceRoute>();
            foreach (var route in routes)
            {
                var serviceRoute = await GetRouteData(route);
                serviceRoutes.Add(serviceRoute);
            }
            return serviceRoutes.ToArray();
        }

        private async Task<ServiceRoute> GetRouteData(string data)
        {
            if (data == null)
                return null;

            var descriptor = _stringSerializer.Deserialize(data, typeof(ServiceRouteDescriptor)) as ServiceRouteDescriptor;
            return (await _serviceRouteFactory.CreateServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {

            childrens = childrens.ToArray();
            var routes = new List<ServiceRoute>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取路由信息。");

                var route = await GetRoute(children);
                if (route != null)
                    routes.Add(route);
            }

            return routes.ToArray();
        }

        private async Task<ServiceRoute> GetRoute(string path)
        {
            ServiceRoute result = null;
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

        private async ValueTask<ConsulClient> GetConsulClient()
        {
            var client = await _consulClientProvider.GetClient();
            return client;
        }

        private async Task EnterRoutes()
        {
            if (_routes != null && _routes.Length > 0)
                return;
            Action<string[]> action = null;
            var client = await GetConsulClient();
            if (_configInfo.EnableChildrenMonitor)
            {
                var watcher = new ChildrenMonitorWatcher(GetConsulClient, _manager, _configInfo.RoutePath,
             async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens),
               (result) => ConvertPaths(result).Result);
                action = currentData => watcher.SetCurrentData(currentData);
            }
            if (client.KV.Keys(_configInfo.RoutePath).Result.Response?.Count() > 0)
            {
                var result = await client.GetChildrenAsync(_configInfo.RoutePath);
                var keys = await client.KV.Keys(_configInfo.RoutePath);
                var childrens = result;
                action?.Invoke(ConvertPaths(childrens).Result.Select(key => $"{_configInfo.RoutePath}{key}").ToArray());
                _routes = await GetRoutes(keys.Response);
            }
            else
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
                    _logger.LogWarning($"无法获取路由信息，因为节点：{_configInfo.RoutePath}，不存在。");
                _routes = new ServiceRoute[0];
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

        /// <summary>
        /// 转化路径集合
        /// </summary>
        /// <param name="datas">信息数据集合</param>
        /// <returns>返回路径集合</returns>
        private async Task<string[]> ConvertPaths(string[] datas)
        {
            List<string> paths = new List<string>();
            foreach (var data in datas)
            {
                var result = await GetRouteData(data);
                var serviceId = result?.ServiceDescriptor.Id;
                if (!string.IsNullOrEmpty(serviceId))
                    paths.Add(serviceId);
            }
            return paths.ToArray();
        }

        private async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newRoute = await GetRoute(newData);
            //得到旧的路由。
            var oldRoute = _routes.FirstOrDefault(i => i.ServiceDescriptor.Id == newRoute.ServiceDescriptor.Id);

            lock (_routes)
            {
                //删除旧路由，并添加上新的路由。
                _routes =
                    _routes
                        .Where(i => i.ServiceDescriptor.Id != newRoute.ServiceDescriptor.Id)
                        .Concat(new[] { newRoute }).ToArray();
            }

            //触发路由变更事件。
            OnChanged(new ServiceRouteChangedEventArgs(newRoute, oldRoute));
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
                _logger.LogDebug($"需要被删除的路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"需要被添加的路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();

            var routes = _routes.ToArray();
            lock (_routes)
            {
                _routes = _routes
                    //删除无效的节点路由。
                    .Where(i => !deletedChildrens.Contains($"{_configInfo.RoutePath}{i.ServiceDescriptor.Id}"))
                    //连接上新的路由。
                    .Concat(newRoutes)
                    .ToArray();
            }
            //需要删除的路由集合。
            var deletedRoutes = routes.Where(i => deletedChildrens.Contains($"{_configInfo.RoutePath}{i.ServiceDescriptor.Id}")).ToArray();
            //触发删除事件。
            OnRemoved(deletedRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.LogInformation("路由数据更新成功。");
        }
        #endregion
    }
}