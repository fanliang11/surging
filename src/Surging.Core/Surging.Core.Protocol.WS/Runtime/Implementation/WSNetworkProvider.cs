using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.WS.Configurations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.WS.Runtime.Implementation
{
    internal class WSNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private readonly ConcurrentDictionary<string, INetwork> _hosts = new ConcurrentDictionary<string, INetwork>();
        private readonly IServiceEntryProvider _serviceEntryProvider;
        private readonly CPlatformContainer _serviceProvider;
        private readonly ILogger<DefaultWSServiceEntryProvider> _wsServiceEntryLogger;
        private readonly ILogger<DefaultWSServerMessageListener> _messageListenerLogger;
        public WSNetworkProvider(IServiceEntryProvider serviceEntryProvider, CPlatformContainer serviceProvider, 
            ILogger<DefaultWSServiceEntryProvider> logger, ILogger<DefaultWSServerMessageListener> mssageListenerLogger)
        {
            _serviceEntryProvider = serviceEntryProvider;
            _serviceProvider= serviceProvider;
            _wsServiceEntryLogger = logger;
            _messageListenerLogger = mssageListenerLogger;
        }

        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var wsServer = _hosts.GetOrAdd(properties.Id, p => {

                var options = new WebSocketOptions();
                var section = AppConfig.GetSection("WebSocket");
                if (section.Exists())
                    options = section.Get<WebSocketOptions>();
                var provider = new DefaultWSServiceEntryProvider(_serviceEntryProvider, _wsServiceEntryLogger, _serviceProvider, options);
                return new DefaultWSServerMessageListener(_messageListenerLogger, provider, options,properties);

            });
            return wsServer;
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var wsServer = _hosts.GetOrAdd(properties.Id, p => {

                var options = new WebSocketOptions();
                var section = AppConfig.GetSection("WebSocket");
                if (section.Exists())
                    options = section.Get<WebSocketOptions>();
                var logger = new WebSocketLogger(subject, properties.Id);
                var provider = new DefaultWSServiceEntryProvider(_serviceEntryProvider, logger, _serviceProvider, options);
                return new DefaultWSServerMessageListener(logger, provider, options, properties);

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
