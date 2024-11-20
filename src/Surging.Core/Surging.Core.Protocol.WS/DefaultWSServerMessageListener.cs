using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using WebSocketCore.Server;
using Surging.Core.Protocol.WS.Runtime;
using Microsoft.Extensions.Logging;
using Surging.Core.Protocol.WS.Configurations;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.Protocol.WS
{
    public class DefaultWSServerMessageListener : IMessageListener, INetwork, IDisposable
    {
        private readonly IEnumerable<WSServiceEntry> _entries;
        private  WebSocketServer _wssv;
        private readonly ILogger<DefaultWSServerMessageListener> _logger;
        private readonly WebSocketOptions _options;
        private readonly NetworkProperties _networkProperties;
        private readonly IWSServiceEntryProvider _wsServiceEntryProvider;

        public DefaultWSServerMessageListener(ILogger<DefaultWSServerMessageListener> logger,
      IWSServiceEntryProvider wsServiceEntryProvider, WebSocketOptions options):this(logger,wsServiceEntryProvider,options,new NetworkProperties())
        {

        }

        public DefaultWSServerMessageListener(ILogger<DefaultWSServerMessageListener> logger,
            IWSServiceEntryProvider wsServiceEntryProvider, WebSocketOptions options, NetworkProperties networkProperties)
        {
            Id = networkProperties?.Id;
            _logger = logger;
            _entries = wsServiceEntryProvider.GetEntries();
            _wsServiceEntryProvider =wsServiceEntryProvider;
            _options = options;
            _networkProperties=networkProperties;
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            _wssv = new WebSocketServer(ipEndPoint.Address, ipEndPoint.Port);
            try
            { 
                foreach (var entry in _entries)
                {   
                    _wsServiceEntryProvider.ChangeEntry(entry.Path, () =>
                    {
                        WebSocketSessionManager result = null; 
                        if (_wssv.WebSocketServices.TryGetServiceHost(entry.Path, out WebSocketServiceHostBase webSocketServiceHost))
                            result = webSocketServiceHost.Sessions;
                        return result;
                    });
                    _wssv.AddWebSocketService(entry.Path, entry.FuncBehavior);
                
                }
                _wssv.KeepClean = _options.KeepClean;
                _wssv.WaitTime = TimeSpan.FromSeconds(_options.WaitTime); 
                //允许转发请求
                _wssv.AllowForwardedRequest = true;  
                _wssv.Start();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"WS服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"WS服务主机启动失败，监听地址：{endPoint}。 ");
            }
         
        }

        public WebSocketServer  Server
        {
            get
            {
                return _wssv;
            }
        }

        public string Id { get ; set; }

        public event ReceivedDelegate Received;

        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _wssv.Stop();
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
                _logger.LogDebug($"websocket服务主机已停止。");
            _wssv?.Stop();
        }

        public bool IsAlive()
        {
            return _wssv.IsListening;
        }

        public bool IsAutoReload()
        {
            return false;
        }
    }
}
