using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime.Implementation
{
    public class TcpNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private   readonly ILogger<TcpNetworkProvider> _logger;
        private readonly ITcpServiceEntryProvider _provider;
        private readonly ConcurrentDictionary<string, DotNettyTcpServerMessageListener> _hosts = new ConcurrentDictionary<string, DotNettyTcpServerMessageListener>();
        public TcpNetworkProvider(ILogger<TcpNetworkProvider> logger, ITcpServiceEntryProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }
        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var tcpServer = _hosts.GetOrAdd(properties.Id, p=>new DotNettyTcpServerMessageListener(_logger, properties.Id, _provider, properties));
            return tcpServer;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string,object>();
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
            if (_hosts.TryGetValue(properties.Id, out DotNettyTcpServerMessageListener tcpServer))
            {
                tcpServer.Shutdown();
                await tcpServer.StartAsync();
            }
        }

       public  NetworkType GetNetworkType()
        {
            return NetworkType.Tcp;
        }

        public void Shutdown(string id)
        {
            if (_hosts.Remove(id, out DotNettyTcpServerMessageListener tcpServer))
                tcpServer.Shutdown();
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var tcpServer = _hosts.GetOrAdd(properties.Id, p => new DotNettyTcpServerMessageListener(new TcpLogger(subject, properties.Id), properties.Id, _provider, properties));
            return tcpServer;
        }
    }
}
