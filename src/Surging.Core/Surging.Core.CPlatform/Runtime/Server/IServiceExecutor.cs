using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务执行器。
    /// </summary>
    public interface IServiceExecutor
    {
        #region 方法

        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">调用消息。</param>
        /// <returns>The <see cref="Task"/></returns>
        Task ExecuteAsync(IMessageSender sender, TransportMessage message);

        #endregion 方法
    }

    #endregion 接口
}