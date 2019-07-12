using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation
{
    /// <summary>
    /// 默认的服务地址解析器。
    /// </summary>
    public class DefaultAddressResolver : IAddressResolver
    {
        #region 字段

        /// <summary>
        /// Defines the _addressSelectors
        /// </summary>
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors = new
            ConcurrentDictionary<string, IAddressSelector>();

        /// <summary>
        /// Defines the _commandProvider
        /// </summary>
        private readonly IServiceCommandProvider _commandProvider;

        /// <summary>
        /// Defines the _concurrent
        /// </summary>
        private readonly ConcurrentDictionary<string, ServiceRoute> _concurrent =
  new ConcurrentDictionary<string, ServiceRoute>();

        /// <summary>
        /// Defines the _container
        /// </summary>
        private readonly CPlatformContainer _container;

        /// <summary>
        /// Defines the _healthCheckService
        /// </summary>
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DefaultAddressResolver> _logger;

        /// <summary>
        /// Defines the _serviceHeartbeatManager
        /// </summary>
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        /// <summary>
        /// Defines the _serviceRouteManager
        /// </summary>
        private readonly IServiceRouteManager _serviceRouteManager;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAddressResolver"/> class.
        /// </summary>
        /// <param name="commandProvider">The commandProvider<see cref="IServiceCommandProvider"/></param>
        /// <param name="serviceRouteManager">The serviceRouteManager<see cref="IServiceRouteManager"/></param>
        /// <param name="logger">The logger<see cref="ILogger{DefaultAddressResolver}"/></param>
        /// <param name="container">The container<see cref="CPlatformContainer"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        /// <param name="serviceHeartbeatManager">The serviceHeartbeatManager<see cref="IServiceHeartbeatManager"/></param>
        public DefaultAddressResolver(IServiceCommandProvider commandProvider, IServiceRouteManager serviceRouteManager, ILogger<DefaultAddressResolver> logger, CPlatformContainer container,
            IHealthCheckService healthCheckService,
            IServiceHeartbeatManager serviceHeartbeatManager)
        {
            _container = container;
            _serviceRouteManager = serviceRouteManager;
            _logger = logger;
            LoadAddressSelectors();
            _commandProvider = commandProvider;
            _healthCheckService = healthCheckService;
            _serviceHeartbeatManager = serviceHeartbeatManager;
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
            serviceRouteManager.Created += ServiceRouteManager_Add;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <param name="item">The item<see cref="string"/></param>
        /// <returns>服务地址模型。</returns>
        public async ValueTask<AddressModel> Resolver(string serviceId, string item)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备为服务id：{serviceId}，解析可用地址。");

            _concurrent.TryGetValue(serviceId, out ServiceRoute descriptor);
            if (descriptor == null)
            {
                var descriptors = await _serviceRouteManager.GetRoutesAsync();
                descriptor = descriptors.FirstOrDefault(i => i.ServiceDescriptor.Id == serviceId);
                if (descriptor != null)
                {
                    _concurrent.GetOrAdd(serviceId, descriptor);
                    _serviceHeartbeatManager.AddWhitelist(serviceId);
                }
                else
                {
                    if (descriptor == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                            _logger.LogWarning($"根据服务id：{serviceId}，找不到相关服务信息。");
                        return null;
                    }
                }
            }

            var address = new List<AddressModel>();
            foreach (var addressModel in descriptor.Address)
            {
                _healthCheckService.Monitor(addressModel);
                var task = _healthCheckService.IsHealth(addressModel);
                if (!(task.IsCompletedSuccessfully ? task.Result : await task))
                {
                    continue;
                }
                address.Add(addressModel);
            }

            if (address.Count == 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务id：{serviceId}，找不到可用的地址。");
                return null;
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"根据服务id：{serviceId}，找到以下可用地址：{string.Join(",", address.Select(i => i.ToString()))}。");
            var vtCommand = _commandProvider.GetCommand(serviceId);
            var command = vtCommand.IsCompletedSuccessfully ? vtCommand.Result : await vtCommand;
            var addressSelector = _addressSelectors[command.ShuntStrategy.ToString()];

            var vt = addressSelector.SelectAsync(new AddressSelectContext
            {
                Descriptor = descriptor.ServiceDescriptor,
                Address = address,
                Item = item
            });
            return vt.IsCompletedSuccessfully ? vt.Result : await vt;
        }

        /// <summary>
        /// The GetCacheKey
        /// </summary>
        /// <param name="descriptor">The descriptor<see cref="ServiceDescriptor"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        /// <summary>
        /// The LoadAddressSelectors
        /// </summary>
        private void LoadAddressSelectors()
        {
            foreach (AddressSelectorMode item in Enum.GetValues(typeof(AddressSelectorMode)))
            {
                _addressSelectors.TryAdd(item.ToString(), _container.GetInstances<IAddressSelector>(item.ToString()));
            }
        }

        /// <summary>
        /// The ServiceRouteManager_Add
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceRouteEventArgs"/></param>
        private void ServiceRouteManager_Add(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            _concurrent.GetOrAdd(key, e.Route);
        }

        /// <summary>
        /// The ServiceRouteManager_Removed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceRouteEventArgs"/></param>
        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            ServiceRoute value;
            _concurrent.TryRemove(key, out value);
        }

        #endregion 方法
    }
}