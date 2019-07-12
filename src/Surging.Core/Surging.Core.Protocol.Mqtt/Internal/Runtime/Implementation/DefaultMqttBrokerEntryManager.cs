using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Mqtt.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultMqttBrokerEntryManager" />
    /// </summary>
    public class DefaultMqttBrokerEntryManager : IMqttBrokerEntryManger
    {
        #region 字段

        /// <summary>
        /// Defines the _brokerEntries
        /// </summary>
        private readonly ConcurrentDictionary<string, IEnumerable<AddressModel>> _brokerEntries =
            new ConcurrentDictionary<string, IEnumerable<AddressModel>>();

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DefaultMqttBrokerEntryManager> _logger;

        /// <summary>
        /// Defines the _mqttServiceRouteManager
        /// </summary>
        private readonly IMqttServiceRouteManager _mqttServiceRouteManager;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMqttBrokerEntryManager"/> class.
        /// </summary>
        /// <param name="mqttServiceRouteManager">The mqttServiceRouteManager<see cref="IMqttServiceRouteManager"/></param>
        /// <param name="logger">The logger<see cref="ILogger{DefaultMqttBrokerEntryManager}"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        public DefaultMqttBrokerEntryManager(IMqttServiceRouteManager mqttServiceRouteManager,
                ILogger<DefaultMqttBrokerEntryManager> logger, IHealthCheckService healthCheckService)
        {
            _mqttServiceRouteManager = mqttServiceRouteManager;
            _logger = logger;
            _mqttServiceRouteManager.Changed += MqttRouteManager_Removed;
            _mqttServiceRouteManager.Removed += MqttRouteManager_Removed;
            healthCheckService.Removed += MqttRouteManager_Removed;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CancellationReg
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="addressModel">The addressModel<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task CancellationReg(string topic, AddressModel addressModel)
        {
            await _mqttServiceRouteManager.RemoveByTopicAsync(topic, new AddressModel[] { addressModel });
        }

        /// <summary>
        /// The GetMqttBrokerAddress
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{IEnumerable{AddressModel}}"/></returns>
        public async ValueTask<IEnumerable<AddressModel>> GetMqttBrokerAddress(string topic)
        {
            _brokerEntries.TryGetValue(topic, out IEnumerable<AddressModel> addresses);
            if (addresses == null || !addresses.Any())
            {
                var routes = await _mqttServiceRouteManager.GetRoutesAsync();
                var route = routes.Where(p => p.MqttDescriptor.Topic == topic).SingleOrDefault();
                if (route != null)
                {
                    _brokerEntries.TryAdd(topic, route.MqttEndpoint);
                    addresses = route.MqttEndpoint;
                }
            }
            return addresses;
        }

        /// <summary>
        /// The Register
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="addressModel">The addressModel<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Register(string topic, AddressModel addressModel)
        {
            await _mqttServiceRouteManager.SetRoutesAsync(new MqttServiceRoute[] { new MqttServiceRoute {
                 MqttDescriptor=new MqttDescriptor{
                      Topic=topic
                 },
                  MqttEndpoint=new AddressModel[]{
                      addressModel
                  }
            }
            });
        }

        /// <summary>
        /// The GetCacheKey
        /// </summary>
        /// <param name="descriptor">The descriptor<see cref="MqttDescriptor"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string GetCacheKey(MqttDescriptor descriptor)
        {
            return descriptor.Topic;
        }

        /// <summary>
        /// The MqttRouteManager_Removed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="HealthCheckEventArgs"/></param>
        private void MqttRouteManager_Removed(object sender, HealthCheckEventArgs e)
        {
            _mqttServiceRouteManager.RemveAddressAsync(new AddressModel[] { e.Address });
        }

        /// <summary>
        /// The MqttRouteManager_Removed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="MqttServiceRouteEventArgs"/></param>
        private void MqttRouteManager_Removed(object sender, MqttServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.MqttDescriptor);
            _brokerEntries.TryRemove(key, out IEnumerable<AddressModel> value);
        }

        #endregion 方法
    }
}