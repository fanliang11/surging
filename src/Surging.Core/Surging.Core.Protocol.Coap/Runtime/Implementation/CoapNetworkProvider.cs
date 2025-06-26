using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Coap.Runtime.Implementation
{
    internal class CoapNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private readonly ConcurrentDictionary<string, INetwork> _hosts = new ConcurrentDictionary<string, INetwork>();
        private readonly IServiceEntryProvider _serviceEntryProvider;
        private readonly CPlatformContainer _serviceProvider;
        private readonly ILogger<DefaultCoapServiceEntryProvider> _coapServiceEntryLogger;
        private readonly ILogger<DefaultCoapServerMessageListener> _messageListenerLogger;
        public CoapNetworkProvider(IServiceEntryProvider serviceEntryProvider, CPlatformContainer serviceProvider,
            ILogger<DefaultCoapServiceEntryProvider> logger, ILogger<DefaultCoapServerMessageListener> messageListenerLogger)
        {
            _serviceEntryProvider = serviceEntryProvider;
            _serviceProvider = serviceProvider;
            _coapServiceEntryLogger = logger;
            _messageListenerLogger = messageListenerLogger;
        }

        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var wsServer = _hosts.GetOrAdd(properties.Id, p => {
                var provider = new DefaultCoapServiceEntryProvider(_serviceEntryProvider, _coapServiceEntryLogger, _serviceProvider);
                return new DefaultCoapServerMessageListener(_messageListenerLogger, provider, properties);
            });
            return wsServer;
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var wsServer = _hosts.GetOrAdd(properties.Id, p => {
                 
                var logger = new CoapLogger(subject, properties.Id);
                var provider = new DefaultCoapServiceEntryProvider(_serviceEntryProvider, logger, _serviceProvider);
                return new DefaultCoapServerMessageListener(logger, provider, properties);

            });
            return wsServer;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string, object>();
        }

        public NetworkType GetNetworkType()
        {
            return NetworkType.WS;
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
            if (_hosts.TryGetValue(properties.Id, out INetwork wsServer))
            {
                wsServer.Shutdown();
                await wsServer.StartAsync();
            }
        }

        public void Shutdown(string id)
        {
            if (_hosts.Remove(id, out INetwork wsServer))
                wsServer.Shutdown();
        }
    }
}
