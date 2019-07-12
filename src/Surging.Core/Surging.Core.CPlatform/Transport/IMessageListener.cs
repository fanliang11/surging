using Surging.Core.CPlatform.Messages;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    #region 委托

    /// <summary>
    /// 接受到消息的委托。
    /// </summary>
    /// <param name="sender">消息发送者。</param>
    /// <param name="message">接收到的消息。</param>
    public delegate Task ReceivedDelegate(IMessageSender sender, TransportMessage message);

    #endregion 委托

    #region 接口

    /// <summary>
    /// 一个抽象的消息监听者。
    /// </summary>
    public interface IMessageListener
    {
        #region 事件

        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        event ReceivedDelegate Received;

        #endregion 事件

        #region 方法

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        Task OnReceived(IMessageSender sender, TransportMessage message);

        #endregion 方法
    }

    #endregion 接口
}