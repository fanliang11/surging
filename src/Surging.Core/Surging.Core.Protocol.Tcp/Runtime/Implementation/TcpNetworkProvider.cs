using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime.Implementation
{
    public class TcpNetworkProvider : INetworkProvider<TcpServerProperties>
    {
        private   readonly ILogger<TcpNetworkProvider> _logger; 
        public TcpNetworkProvider(ILogger<TcpNetworkProvider> logger)
        {
            _logger = logger;
        }
        public INetwork CreateNetwork(TcpServerProperties properties)
        {
            var tcpServer = new DotNettyTcpServerMessageListener(_logger, properties.Id, properties);
            return tcpServer;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string,object>();
        }

        public async void Reload(INetwork network, TcpServerProperties properties)
        {
            var tcpServer =  network as DotNettyTcpServerMessageListener;
            if (tcpServer != null)
            {
                tcpServer.Shutdown();
                await tcpServer.StartAsync();
            }
        }

       public  NetworkType GetNetworkType()
        {
            return NetworkType.TcpServer;
        }
    }
}
