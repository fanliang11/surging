using Surging.Core.CPlatform.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    #region 接口

    /// <summary>
    /// 一个抽象的传输客户端。
    /// </summary>
    public interface ITransportClient
    {
        #region 方法

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>远程调用消息的传输消息。</returns>
        Task<RemoteInvokeResultMessage> SendAsync(RemoteInvokeMessage message, CancellationToken cancellationToken);

        #endregion 方法
    }

    #endregion 接口
}