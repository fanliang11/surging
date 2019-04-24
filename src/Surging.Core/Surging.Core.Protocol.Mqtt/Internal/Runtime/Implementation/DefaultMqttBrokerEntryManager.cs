using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Mqtt.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    public class DefaultMqttBrokerEntryManager : IMqttBrokerEntryManger
    {
        private readonly IMqttServiceRouteManager _mqttServiceRouteManager;
        private readonly ILogger<DefaultMqttBrokerEntryManager> _logger;
        private readonly ConcurrentDictionary<string, IEnumerable<AddressModel>> _brokerEntries =
            new ConcurrentDictionary<string, IEnumerable<AddressModel>>();

        public DefaultMqttBrokerEntryManager(IMqttServiceRouteManager mqttServiceRouteManager,
                ILogger<DefaultMqttBrokerEntryManager> logger, IHealthCheckService healthCheckService)
        {
            _mqttServiceRouteManager = mqttServiceRouteManager;
            _logger = logger;
            _mqttServiceRouteManager.Changed += MqttRouteManager_Removed;
            _mqttServiceRouteManager.Removed += MqttRouteManager_Removed;
            healthCheckService.Removed += MqttRouteManager_Removed;
        }

        public async Task CancellationReg(string topic, AddressModel addressModel)
        {
            await _mqttServiceRouteManager.RemoveByTopicAsync(topic, new AddressModel[] { addressModel });
        }

        public async ValueTask<IEnumerable<AddressModel>> GetMqttBrokerAddress(string topic)
        {
            _brokerEntries.TryGetValue(topic, out IEnumerable<AddressModel> addresses);
            if (addresses==null || !addresses.Any())
            {
                var routes = await _mqttServiceRouteManager.GetRoutesAsync();
                var route=  routes.Where(p => p.MqttDescriptor.Topic == topic).SingleOrDefault();
                if (route != null)
                {
                    _brokerEntries.TryAdd(topic, route.MqttEndpoint);
                    addresses = route.MqttEndpoint;
                }
            }
            return addresses;
        }

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

        private static string GetCacheKey(MqttDescriptor descriptor)
        {
            return descriptor.Topic;
        }

        private void MqttRouteManager_Removed(object sender, MqttServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.MqttDescriptor);
            _brokerEntries.TryRemove(key, out IEnumerable<AddressModel> value);
        }

        private void MqttRouteManager_Removed(object sender, HealthCheckEventArgs e)
        {
            _mqttServiceRouteManager.RemveAddressAsync(new AddressModel[] { e.Address });
        }
    }
}
