using Surging.Core.CPlatform.Messages;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    #region 接口

    /// <summary>
    /// 一个抽象的发送者。
    /// </summary>
    public interface IMessageSender
    {
        #region 方法

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        Task SendAndFlushAsync(TransportMessage message);

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        Task SendAsync(TransportMessage message);

        #endregion 方法
    }

    #endregion 接口
}