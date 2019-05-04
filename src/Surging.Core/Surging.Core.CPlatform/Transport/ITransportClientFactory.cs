using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    /// <summary>
    /// 一个抽象的传输客户端工厂。
    /// </summary>
    public interface ITransportClientFactory
    {
        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        Task<ITransportClient> CreateClientAsync(EndPoint endPoint);
    }
}