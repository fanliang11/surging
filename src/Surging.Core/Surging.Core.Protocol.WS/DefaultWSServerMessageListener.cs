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

namespace Surging.Core.Protocol.WS
{
    public class DefaultWSServerMessageListener : IMessageListener, IDisposable
    {
        private readonly List<WSServiceEntry> _entries;
        private  WebSocketServer _wssv;
        private readonly ILogger<DefaultWSServerMessageListener> _logger;
        public DefaultWSServerMessageListener(ILogger<DefaultWSServerMessageListener> logger,IWSServiceEntryProvider wsServiceEntryProvider)
        {
            _logger = logger;
            _entries = wsServiceEntryProvider.GetEntries().ToList();
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            _wssv = new WebSocketServer(ipEndPoint.Address, ipEndPoint.Port);
            try
            {
                foreach (var entry in _entries)
                    _wssv.AddWebSocketService(entry.Path, entry.Behavior);
                _wssv.KeepClean = false;  
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

        public event ReceivedDelegate Received;

        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _wssv.Stop();
        }
    }
}
