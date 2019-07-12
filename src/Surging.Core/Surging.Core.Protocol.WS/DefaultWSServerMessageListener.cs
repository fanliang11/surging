using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.WS.Configurations;
using Surging.Core.Protocol.WS.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebSocketCore.Server;

namespace Surging.Core.Protocol.WS
{
    /// <summary>
    /// Defines the <see cref="DefaultWSServerMessageListener" />
    /// </summary>
    public class DefaultWSServerMessageListener : IMessageListener, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _entries
        /// </summary>
        private readonly List<WSServiceEntry> _entries;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DefaultWSServerMessageListener> _logger;

        /// <summary>
        /// Defines the _options
        /// </summary>
        private readonly WebSocketOptions _options;

        /// <summary>
        /// Defines the _wssv
        /// </summary>
        private WebSocketServer _wssv;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWSServerMessageListener"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{DefaultWSServerMessageListener}"/></param>
        /// <param name="wsServiceEntryProvider">The wsServiceEntryProvider<see cref="IWSServiceEntryProvider"/></param>
        /// <param name="options">The options<see cref="WebSocketOptions"/></param>
        public DefaultWSServerMessageListener(ILogger<DefaultWSServerMessageListener> logger,
            IWSServiceEntryProvider wsServiceEntryProvider, WebSocketOptions options)
        {
            _logger = logger;
            _entries = wsServiceEntryProvider.GetEntries().ToList();
            _options = options;
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Defines the Received
        /// </summary>
        public event ReceivedDelegate Received;

        #endregion 事件

        #region 属性

        /// <summary>
        /// Gets the Server
        /// </summary>
        public WebSocketServer Server
        {
            get
            {
                return _wssv;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _wssv.Stop();
        }

        /// <summary>
        /// The OnReceived
        /// </summary>
        /// <param name="sender">The sender<see cref="IMessageSender"/></param>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// The StartAsync
        /// </summary>
        /// <param name="endPoint">The endPoint<see cref="EndPoint"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            _wssv = new WebSocketServer(ipEndPoint.Address, ipEndPoint.Port);
            try
            {
                foreach (var entry in _entries)
                    _wssv.AddWebSocketService(entry.Path, entry.FuncBehavior);
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

        #endregion 方法
    }
}