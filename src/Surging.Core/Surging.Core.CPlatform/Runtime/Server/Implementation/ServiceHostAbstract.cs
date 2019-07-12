using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Implementation;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation
{
    /// <summary>
    /// 服务主机基类。
    /// </summary>
    public abstract class ServiceHostAbstract : IServiceHost
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceExecutor
        /// </summary>
        private readonly IServiceExecutor _serviceExecutor;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHostAbstract"/> class.
        /// </summary>
        /// <param name="serviceExecutor">The serviceExecutor<see cref="IServiceExecutor"/></param>
        protected ServiceHostAbstract(IServiceExecutor serviceExecutor)
        {
            _serviceExecutor = serviceExecutor;
            MessageListener.Received += MessageListener_Received;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ServiceExecutor
        /// </summary>
        public IServiceExecutor ServiceExecutor { get => _serviceExecutor; }

        /// <summary>
        /// Gets the MessageListener
        /// 消息监听者。
        /// </summary>
        protected IMessageListener MessageListener { get; } = new MessageListener();

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public abstract Task StartAsync(EndPoint endPoint);

        /// <summary>
        /// The StartAsync
        /// </summary>
        /// <param name="ip">The ip<see cref="string"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task StartAsync(string ip, int port);

        /// <summary>
        /// The MessageListener_Received
        /// </summary>
        /// <param name="sender">The sender<see cref="IMessageSender"/></param>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            await _serviceExecutor.ExecuteAsync(sender, message);
        }

        #endregion 方法
    }
}