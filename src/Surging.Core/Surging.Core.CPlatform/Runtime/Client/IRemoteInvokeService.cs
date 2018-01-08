using Surging.Core.CPlatform.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client
{
    /// <summary>
    /// 一个抽象的远程调用服务。
    /// </summary>
    public interface IRemoteInvokeService
    {
        /// <summary>
        /// 调用。
        /// </summary>
        /// <param name="context">调用上下文。</param>
        /// <returns>远程调用结果消息模型。</returns>
        Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context);

        /// <summary>
        /// 调用。
        /// </summary>
        /// <param name="context">调用上下文。</param>
        /// <param name="cancellationToken">取消操作通知实例。</param>
        /// <returns>远程调用结果消息模型。</returns>
        Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken);

        /// <summary>
        /// 调用。
        /// </summary>
        /// <param name="context">调用上下文。</param>
        /// <param name="cancellationToken">超时时间。</param>
        /// <returns>远程调用结果消息模型。</returns>
        Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, int requestTimeout);
    }
}