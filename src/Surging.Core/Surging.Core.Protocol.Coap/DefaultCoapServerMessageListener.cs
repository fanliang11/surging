using CoAP.Server;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.Coap.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Coap
{
    internal class DefaultCoapServerMessageListener : IMessageListener, INetwork, IDisposable
    {
        private readonly IEnumerable<CoapServiceEntry> _entries;
        private CoapServer _coapServer;
        private readonly ILogger<DefaultCoapServerMessageListener> _logger; 
        private readonly NetworkProperties _networkProperties; 

        public DefaultCoapServerMessageListener(ILogger<DefaultCoapServerMessageListener> logger, ICoapServiceEntryProvider coapServiceEntryProvider) : this(logger,coapServiceEntryProvider, new NetworkProperties())
        {

        }

        public DefaultCoapServerMessageListener(ILogger<DefaultCoapServerMessageListener> logger, ICoapServiceEntryProvider coapServiceEntryProvider, NetworkProperties networkProperties)
        {
            Id = networkProperties?.Id;
            _logger = logger;
            _entries = coapServiceEntryProvider.GetEntries();
            _networkProperties = networkProperties;
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            if (ipEndPoint.Port > 0)
            {
                try
                { 
                    _coapServer = new CoapServer(ipEndPoint);
                    foreach (var entry in _entries)
                    {
                        _coapServer.Add(entry.Behavior);
                    }
                    _coapServer.Start();
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"Coap服务主机启动成功，监听地址：{endPoint}。");
                }
                catch
                {
                    _logger.LogError($"Coap服务主机启动失败，监听地址：{endPoint}。 ");
                }
            }
        }

        public CoapServer Server
        {
            get
            {
                return _coapServer;
            }
        }

        public string Id { get; set; }

        public event ReceivedDelegate Received;

        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _coapServer.Stop();
        }

        public async Task StartAsync()
        {
            await StartAsync(_networkProperties.CreateSocketAddress());
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.WS;
        }

        public void Shutdown()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"coap服务主机已停止。");
            _coapServer?.Stop();
        }

        public bool IsAlive()
        {
            return true;
        }

        public bool IsAutoReload()
        {
            return false;
        }
    }
}
