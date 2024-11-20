using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Surging.Core.KestrelHttpServer.Runtime;

namespace Surging.Core.KestrelHttpServer.Internal
{
    internal class HttpNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private readonly ILogger<KestrelHttpMessageListener> _logger;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceEngineLifetime _lifetime;
        private readonly IModuleProvider _moduleProvider;
        private readonly CPlatformContainer _container;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IHttpServiceEntryProvider _httpServiceEntryProvider;
        private readonly ConcurrentDictionary<string, KestrelHttpMessageListener> _hosts = new ConcurrentDictionary<string, KestrelHttpMessageListener>();
        public HttpNetworkProvider(ILogger<KestrelHttpMessageListener> logger,
            ISerializer<string> serializer,
            IServiceEngineLifetime lifetime,
            IModuleProvider moduleProvider,
            IHttpServiceEntryProvider httpServiceEntryProvider,
            IServiceRouteProvider serviceRouteProvider,
            CPlatformContainer container)
        {
            _logger = logger;
            _serializer = serializer;
            _lifetime = lifetime;
            _moduleProvider = moduleProvider;
            _httpServiceEntryProvider = httpServiceEntryProvider;
            _container = container;
            _serviceRouteProvider = serviceRouteProvider;
        }
        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var httpServer = _hosts.GetOrAdd(properties.Id, new KestrelHttpMessageListener(_logger, _serializer, _lifetime, _moduleProvider, _serviceRouteProvider, _httpServiceEntryProvider,_container, properties));
            return httpServer;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string, object>();
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
           if( _hosts.TryGetValue(properties.Id, out KestrelHttpMessageListener httpServer))
            {
                httpServer.Shutdown();
                await httpServer.StartAsync();
            }
        }

        public NetworkType GetNetworkType()
        {
            return NetworkType.Http;
        }

        public void Shutdown(string id)
        {
           if( _hosts.Remove(id, out KestrelHttpMessageListener httpServer))
             httpServer.Shutdown();
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var httpServer = _hosts.GetOrAdd(properties.Id, new KestrelHttpMessageListener(new HttpLogger(subject, properties.Id), _serializer, _lifetime, _moduleProvider, _serviceRouteProvider, _httpServiceEntryProvider, _container, properties));
            return httpServer;
        }
    }
}
 