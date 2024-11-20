using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime.Implementation
{
    internal class TcpClientNetworkProvider : INetworkProvider<NetworkProperties>
    {
        private readonly ILogger<DotNettyTcpTransportClientFactory> _logger;
        private readonly ConcurrentDictionary<string, DotNettyTcpTransportClientFactory> _hosts = new ConcurrentDictionary<string, DotNettyTcpTransportClientFactory>();
        public TcpClientNetworkProvider(ILogger<DotNettyTcpTransportClientFactory> logger)
        {
            _logger = logger;
        }

        public INetwork CreateNetwork(NetworkProperties properties)
        {
            var tcpClient = _hosts.GetOrAdd(properties.Id, p => new DotNettyTcpTransportClientFactory(_logger, properties));
            return tcpClient;
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            var tcpClient = _hosts.GetOrAdd(properties.Id, p => new DotNettyTcpTransportClientFactory(new TcpClientLogger(subject, properties.Id), properties));
            return tcpClient;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string, object>();
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
            if (_hosts.TryGetValue(properties.Id, out DotNettyTcpTransportClientFactory tcpClient))
            {
                tcpClient.Shutdown();
                await tcpClient.StartAsync();
            }
        }

        public NetworkType GetNetworkType()
        {
            return NetworkType.TcpClient;
        }

        public void Shutdown(string id)
        {
            if (_hosts.Remove(id, out DotNettyTcpTransportClientFactory tcpClient))
                tcpClient.Shutdown();
        }

    }
}
