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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Core.Zookeeper.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultZookeeperClientProvider" />
    /// </summary>
    public class DefaultZookeeperClientProvider : IZookeeperClientProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _addressSelectors
        /// </summary>
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors = new
            ConcurrentDictionary<string, IAddressSelector>();

        /// <summary>
        /// Defines the _healthCheckService
        /// </summary>
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DefaultZookeeperClientProvider> _logger;

        /// <summary>
        /// Defines the _zookeeperAddressSelector
        /// </summary>
        private readonly IZookeeperAddressSelector _zookeeperAddressSelector;

        /// <summary>
        /// Defines the _zookeeperClients
        /// </summary>
        private readonly ConcurrentDictionary<AddressModel, ValueTuple<ManualResetEvent, ZooKeeper>> _zookeeperClients = new
           ConcurrentDictionary<AddressModel, ValueTuple<ManualResetEvent, ZooKeeper>>();

        /// <summary>
        /// Defines the _config
        /// </summary>
        private ConfigInfo _config;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultZookeeperClientProvider"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="ConfigInfo"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        /// <param name="zookeeperAddressSelector">The zookeeperAddressSelector<see cref="IZookeeperAddressSelector"/></param>
        /// <param name="logger">The logger<see cref="ILogger{DefaultZookeeperClientProvider}"/></param>
        public DefaultZookeeperClientProvider(ConfigInfo config, IHealthCheckService healthCheckService, IZookeeperAddressSelector zookeeperAddressSelector,
      ILogger<DefaultZookeeperClientProvider> logger)
        {
            _config = config;
            _healthCheckService = healthCheckService;
            _zookeeperAddressSelector = zookeeperAddressSelector;
            _logger = logger;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Check
        /// </summary>
        /// <returns>The <see cref="ValueTask"/></returns>
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

        /// <summary>
        /// The GetZooKeeper
        /// </summary>
        /// <returns>The <see cref="ValueTask{(ManualResetEvent, ZooKeeper)}"/></returns>
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

        /// <summary>
        /// The GetZooKeepers
        /// </summary>
        /// <returns>The <see cref="ValueTask{IEnumerable{(ManualResetEvent, ZooKeeper)}}"/></returns>
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

        /// <summary>
        /// The CreateZooKeeper
        /// </summary>
        /// <param name="ipAddress">The ipAddress<see cref="IpAddressModel"/></param>
        /// <returns>The <see cref="(ManualResetEvent, ZooKeeper)"/></returns>
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
                _zookeeperClients.AddOrUpdate(ipAddress, result, (k, v) => result);
            }
            return result;
        }

        #endregion 方法
    }
}