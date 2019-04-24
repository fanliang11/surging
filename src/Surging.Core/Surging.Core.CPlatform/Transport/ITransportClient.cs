using Surging.Core.CPlatform.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    /// <summary>
    /// 一个抽象的传输客户端。
    /// </summary>
    public interface ITransportClient
    {
        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>远程调用消息的传输消息。</returns>
        Task<RemoteInvokeResultMessage> SendAsync(RemoteInvokeMessage message, CancellationToken cancellationToken);
    }
}