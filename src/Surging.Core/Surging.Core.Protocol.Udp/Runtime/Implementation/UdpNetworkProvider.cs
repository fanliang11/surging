using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp.Runtime.Implementation
{
    public class UdpNetworkProvider: INetworkProvider<NetworkProperties>
    {
        private readonly ILogger<DotNettyUdpServerMessageListener> _logger;
        private readonly ITransportMessageCodecFactory _codecFactory;
        private readonly IUdpServiceEntryProvider _provider;
        private readonly ConcurrentDictionary<string, DotNettyUdpServerMessageListener> _hosts = new ConcurrentDictionary<string, DotNettyUdpServerMessageListener>();
        public UdpNetworkProvider(ILogger<DotNettyUdpServerMessageListener> logger, ITransportMessageCodecFactory codecFactory, IUdpServiceEntryProvider provider)
        {
            _logger = logger;
            _codecFactory = codecFactory;
            _provider = provider;
        }
        public  INetwork CreateNetwork(NetworkProperties properties)
        {
            IMessageListener updServer = _hosts.GetOrAdd(properties.Id, p => new DotNettyUdpServerMessageListener(_logger, _codecFactory, properties, _provider));
          var serviceExecutor= ServiceLocator.GetService<IServiceExecutor>(CommunicationProtocol.Udp.ToString());
            UdpServiceHost udpServiceHost = new UdpServiceHost(p => Task.FromResult(updServer), serviceExecutor);
             udpServiceHost.StartAsync(null).Wait();
            return updServer as INetwork;
        }

        public IDictionary<string, object> GetConfigMetadata()
        {
            return new Dictionary<string, object>();
        }

        public async void ReloadAsync(NetworkProperties properties)
        {
            if (_hosts.TryGetValue(properties.Id, out DotNettyUdpServerMessageListener udpServer))
            {
                udpServer.Shutdown();
                await udpServer.StartAsync();
            }
        }

        public NetworkType GetNetworkType()
        {
            return NetworkType.Udp;
        }

        public void Shutdown(string id)
        {
            if (_hosts.Remove(id, out DotNettyUdpServerMessageListener udpServer))
                udpServer.Shutdown();
        }

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            IMessageListener updServer = _hosts.GetOrAdd(properties.Id, p => new DotNettyUdpServerMessageListener(new UdpLogger(subject, properties.Id), _codecFactory, properties, _provider));
            var serviceExecutor = ServiceLocator.GetService<IServiceExecutor>(CommunicationProtocol.Udp.ToString());
            UdpServiceHost udpServiceHost = new UdpServiceHost(p => Task.FromResult(updServer), serviceExecutor);
            udpServiceHost.StartAsync(null).Wait();
            return updServer as INetwork;
        }
    }
}
 
