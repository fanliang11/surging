using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    internal class MqttNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private readonly ILogger<DotNettyMqttServerMessageListener> _logger;
        private readonly IChannelService _channelService;
        private readonly IMqttBehaviorProvider _mqttBehaviorProvider;
        private readonly ConcurrentDictionary<string, INetwork> _hosts = new ConcurrentDictionary<string, INetwork>();
        public MqttNetworkProvider(ILogger<DotNettyMqttServerMessageListener> logger, 
            IChannelService channelService,
            IMqttBehaviorProvider mqttBehaviorProvider) {
            _logger = logger;
            _channelService=channelService;
            _mqttBehaviorProvider = mqttBehaviorProvider;
        }
        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var mqttServer = _hosts.GetOrAdd(properties.Id, p => new DotNettyMqttServerMessageListener(_logger, _channelService, _mqttBehaviorProvider, properties));
            return mqttServer;
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var mqttServer = _hosts.GetOrAdd(properties.Id, p => new DotNettyMqttServerMessageListener(new MqttLogger(subject,properties.Id), _channelService, _mqttBehaviorProvider, properties));
            return mqttServer;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string, object>();
        }

        public NetworkType GetNetworkType()
        {
            return NetworkType.Mqtt;
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
            if (_hosts.TryGetValue(properties.Id, out INetwork mqttServer))
            {
                mqttServer.Shutdown();
                await mqttServer.StartAsync();
            }
        }

        public void Shutdown(string id)
        {
            if (_hosts.Remove(id, out INetwork mqttServer))
                mqttServer.Shutdown();
        }
    }
}
