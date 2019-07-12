using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DNS
{
    /// <summary>
    /// Defines the <see cref="DnsServiceHost" />
    /// </summary>
    public class DnsServiceHost : ServiceHostAbstract
    {
        #region 字段

        /// <summary>
        /// Defines the _messageListenerFactory
        /// </summary>
        private readonly Func<EndPoint, Task<IMessageListener>> _messageListenerFactory;

        /// <summary>
        /// Defines the _serverMessageListener
        /// </summary>
        private IMessageListener _serverMessageListener;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsServiceHost"/> class.
        /// </summary>
        /// <param name="messageListenerFactory">The messageListenerFactory<see cref="Func{EndPoint, Task{IMessageListener}}"/></param>
        /// <param name="serviceExecutor">The serviceExecutor<see cref="IServiceExecutor"/></param>
        public DnsServiceHost(Func<EndPoint, Task<IMessageListener>> messageListenerFactory, IServiceExecutor serviceExecutor) : base(serviceExecutor)
        {
            _messageListenerFactory = messageListenerFactory;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public override void Dispose()
        {
            (_serverMessageListener as IDisposable)?.Dispose();
        }

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task StartAsync(EndPoint endPoint)
        {
            if (_serverMessageListener != null)
                return;
            _serverMessageListener = await _messageListenerFactory(endPoint);
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MessageListener.OnReceived(sender, message);
                });
            };
        }

        /// <summary>
        /// The StartAsync
        /// </summary>
        /// <param name="ip">The ip<see cref="string"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task StartAsync(string ip, int port)
        {
            if (_serverMessageListener != null)
                return;
            _serverMessageListener = await _messageListenerFactory(new IPEndPoint(IPAddress.Parse(ip), 53));
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MessageListener.OnReceived(sender, message);
                });
            };
        }

        #endregion 方法
    }
}