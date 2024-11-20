using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Grpc.Runtime.Implementation
{
    internal class GrpcNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private readonly ConcurrentDictionary<string, INetwork> _hosts = new ConcurrentDictionary<string, INetwork>();
        private readonly IGrpcServiceEntryProvider _grpcServiceEntryProvider;
        private readonly ILogger<GrpcServerMessageListener> _logger;
        public GrpcNetworkProvider(IGrpcServiceEntryProvider grpcServiceEntryProvider, ILogger<GrpcServerMessageListener> logger)
        {
            _grpcServiceEntryProvider= grpcServiceEntryProvider;
            _logger= logger;
        }

        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var grpcServer = _hosts.GetOrAdd(properties.Id, p => new GrpcServerMessageListener(_logger,_grpcServiceEntryProvider, properties));
            return grpcServer;
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var grpcServer = _hosts.GetOrAdd(properties.Id, p => new GrpcServerMessageListener(new GrpcLogger(subject,properties.Id), _grpcServiceEntryProvider, properties));
            return grpcServer;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string, object>();
        }

        public NetworkType GetNetworkType()
        {
            return NetworkType.Grpc;
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
            if (_hosts.TryGetValue(properties.Id, out INetwork grpcServer))
            {
                grpcServer.Shutdown();
                await grpcServer.StartAsync();
            }
        }

        public void Shutdown(string id)
        {
            if (_hosts.Remove(id, out INetwork grpcServer))
                grpcServer.Shutdown();
        }

         
    }
}
