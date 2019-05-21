using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Core.Zookeeper.Configurations;
using Surging.Core.Zookeeper.Internal.Cluster.HealthChecks;
using Surging.Core.Zookeeper.Internal.Cluster.Implementation.Selectors;
using Surging.Core.Zookeeper.WatcherProvider;
using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Core.Zookeeper.Internal.Implementation
{
    public class DefaultZookeeperClientProvider : IZookeeperClientProvider
    {
        private ConfigInfo _config;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IZookeeperAddressSelector _zookeeperAddressSelector;
        private readonly ILogger<DefaultZookeeperClientProvider> _logger;
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors = new
            ConcurrentDictionary<string, IAddressSelector>();
        private readonly ConcurrentDictionary<AddressModel,ValueTuple<ManualResetEvent, ZooKeeper>> _zookeeperClients = new
           ConcurrentDictionary<AddressModel, ValueTuple<ManualResetEvent, ZooKeeper>>();
        public DefaultZookeeperClientProvider(ConfigInfo config, IHealthCheckService healthCheckService, IZookeeperAddressSelector zookeeperAddressSelector,
      ILogger<DefaultZookeeperClientProvider> logger)
        {
            _config = config;
            _healthCheckService = healthCheckService;
            _zookeeperAddressSelector = zookeeperAddressSelector;
            _logger = logger;
        }
        public async ValueTask Check()
        {
            foreach (var address in _config.Addresses)
            {
                if (!await _healthCheckService.IsHealth(address))
                {
                    throw new RegisterConnectionException(string.Format("注册中心{0}连接异常，请联系管理园", address.ToString()));
                }
            }
        }

        public async ValueTask<(ManualResetEvent, ZooKeeper)> GetZooKeeper()
        {

            (ManualResetEvent, ZooKeeper) result = new ValueTuple<ManualResetEvent, ZooKeeper>();
            var address = new List<AddressModel>();
            foreach (var addressModel in _config.Addresses)
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
                if (_logger.IsEnabled(Level.Warning))
                    _logger.LogWarning($"找不到可用的注册中心地址。");
                return default(ValueTuple<ManualResetEvent, ZooKeeper>);
            }

            var vt = _zookeeperAddressSelector.SelectAsync(new AddressSelectContext
            {
                Descriptor = new ServiceDescriptor { Id = nameof(DefaultZookeeperClientProvider) },
                Address = address
            });
            var addr = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            if (addr != null)
            {
                var ipAddress = addr as IpAddressModel;
                result = CreateZooKeeper(ipAddress);
            }
            return result;
        }

        protected (ManualResetEvent, ZooKeeper) CreateZooKeeper(IpAddressModel ipAddress)
        {
            if (!_zookeeperClients.TryGetValue(ipAddress, out (ManualResetEvent, ZooKeeper) result))
            {
                var connectionWait = new ManualResetEvent(false);
                result = new ValueTuple<ManualResetEvent, ZooKeeper>(connectionWait, new ZooKeeper($"{ipAddress.Ip}:{ipAddress.Port}", (int)_config.SessionTimeout.TotalMilliseconds
                 , new ReconnectionWatcher(
                      () =>
                      {
                          connectionWait.Set();
                      },
                      () =>
                      {
                          connectionWait.Close();
                      },
                      async () =>
                      {
                          connectionWait.Reset();
                          if (_zookeeperClients.TryRemove(ipAddress, out (ManualResetEvent, ZooKeeper) value))
                          { 
                              await value.Item2.closeAsync();
                              value.Item1.Close();
                          }
                          CreateZooKeeper(ipAddress);
                      })));
                _zookeeperClients.AddOrUpdate(ipAddress, result,(k,v)=> result);
            }
            return result;
        }

        public async ValueTask<IEnumerable<(ManualResetEvent, ZooKeeper)>> GetZooKeepers()
        {
            var result = new List<(ManualResetEvent, ZooKeeper)>();
            foreach (var address in _config.Addresses)
            {
                var ipAddress = address as IpAddressModel;
                if (await _healthCheckService.IsHealth(address))
                {
                    result.Add(CreateZooKeeper(ipAddress));

                }
            }
            return result;
        }
    }
}
