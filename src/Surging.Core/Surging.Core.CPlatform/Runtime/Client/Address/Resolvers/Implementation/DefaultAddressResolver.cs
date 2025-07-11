﻿using Microsoft.Extensions.Logging;
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
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors=new
            ConcurrentDictionary<string, IAddressSelector>();
        private readonly IServiceCommandProvider _commandProvider;
        private readonly ConcurrentDictionary<string, ServiceRoute> _concurrent =
  new ConcurrentDictionary<string, ServiceRoute>();
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;
        #endregion Field

        #region Constructor

        public DefaultAddressResolver(IServiceCommandProvider commandProvider, IServiceRouteProvider serviceRouteProvider, ILogger<DefaultAddressResolver> logger, CPlatformContainer container,
            IHealthCheckService healthCheckService,
            IServiceHeartbeatManager serviceHeartbeatManager)
        {
            _container = container;
            _serviceRouteProvider = serviceRouteProvider;
            _logger = logger;
            LoadAddressSelectors();
            _commandProvider = commandProvider;
            _healthCheckService = healthCheckService;
            _serviceHeartbeatManager = serviceHeartbeatManager;
        }

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        /// 1.从字典中拿到serviceroute对象
        /// 2.从字典中拿到服务描述符集合
        /// 3.获取或添加serviceroute
        /// 4.添加服务id到白名单
        /// 5.根据服务描述符得到地址并判断地址是否是可用的（地址应该是多个）
        /// 6.添加到集合中
        /// 7.拿到服务命今
        /// 8.根据负载分流策略拿到一个选择器
        /// 9.返回addressmodel
        public async ValueTask<AddressModel> Resolver(string serviceId, string item)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备为服务id：{serviceId}，解析可用地址。");

           var serviceRouteTask=   _serviceRouteProvider.Locate(serviceId);
            var serviceRoute = serviceRouteTask.IsCompletedSuccessfully ? serviceRouteTask.Result : await serviceRouteTask;
            if (serviceRoute == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务id：{serviceId}，找不到相关服务信息。");
                return null;
            }
            _serviceHeartbeatManager.AddWhitelist(serviceId);
            var addresses = serviceRoute.Address;
            var address = new List<AddressModel>(addresses.Count());
            foreach (var addressModel in addresses)
            {
                var task = _healthCheckService.MonitorHealth(addressModel); 
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
                Descriptor = serviceRoute.ServiceDescriptor,
                Address = address,
                Item = item
            });
            return vt.IsCompletedSuccessfully ? vt.Result : await vt;
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