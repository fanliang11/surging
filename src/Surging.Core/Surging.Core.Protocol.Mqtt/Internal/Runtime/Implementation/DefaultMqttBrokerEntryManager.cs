using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    public class DefaultMqttBrokerEntryManager : IMqttBrokerEntryManger
    {
        private readonly IMqttServiceRouteManager _mqttServiceRouteManager;
        private readonly ILogger<DefaultMqttBrokerEntryManager> _logger;
        private readonly ConcurrentDictionary<string, IEnumerable<AddressModel>> _brokerEntries =
            new ConcurrentDictionary<string, IEnumerable<AddressModel>>();

        public DefaultMqttBrokerEntryManager(IMqttServiceRouteManager mqttServiceRouteManager,
                ILogger<DefaultMqttBrokerEntryManager> logger)
        {
            _mqttServiceRouteManager = mqttServiceRouteManager;
            _logger = logger;
        }

        public async Task CancellationReg(string topic, AddressModel addressModel)
        {
            await _mqttServiceRouteManager.RemoveByTopicAsync(topic, new AddressModel[] { addressModel });
        }

        public async ValueTask<IEnumerable<AddressModel>> GetMqttBrokerAddress(string topic)
        {
            _brokerEntries.TryGetValue(topic, out IEnumerable<AddressModel> addresses);
            if (!addresses.Any())
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
    }
}
