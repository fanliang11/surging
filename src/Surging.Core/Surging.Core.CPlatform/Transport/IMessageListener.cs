using Surging.Core.CPlatform.Messages;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    /// <summary>
    /// 接受到消息的委托。
    /// </summary>
    /// <param name="sender">消息发送者。</param>
    /// <param name="message">接收到的消息。</param>
    public delegate Task ReceivedDelegate(IMessageSender sender, TransportMessage message);

    /// <summary>
    /// 一个抽象的消息监听者。
    /// </summary>
    public interface IMessageListener
    {
        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        event ReceivedDelegate Received;
        
        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        Task OnReceived(IMessageSender sender, TransportMessage message);
    }
}