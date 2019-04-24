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
        #region Field

        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly ILogger<DefaultAddressResolver> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly CPlatformContainer _container;
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors=new
            ConcurrentDictionary<string, IAddressSelector>();
        private readonly IServiceCommandProvider _commandProvider;
        private readonly ConcurrentDictionary<string, ServiceRoute> _concurrent =
  new ConcurrentDictionary<string, ServiceRoute>();
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;
        #endregion Field

        #region Constructor

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

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
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

        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            ServiceRoute value;
            _concurrent.TryRemove(key, out value);
        }

        private void ServiceRouteManager_Add(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            _concurrent.GetOrAdd(key, e.Route);
        }

        private void LoadAddressSelectors()
        {
            foreach (AddressSelectorMode item in Enum.GetValues(typeof(AddressSelectorMode)))
            {
               _addressSelectors.TryAdd( item.ToString(), _container.GetInstances<IAddressSelector>(item.ToString()));
            }
        }

        #endregion Implementation of IAddressResolver
    }
}