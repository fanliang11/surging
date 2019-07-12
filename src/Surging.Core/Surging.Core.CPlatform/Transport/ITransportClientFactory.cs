using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    #region 接口

    /// <summary>
    /// 一个抽象的传输客户端工厂。
    /// </summary>
    public interface ITransportClientFactory
    {
        #region 方法

        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        Task<ITransportClient> CreateClientAsync(EndPoint endPoint);

        #endregion 方法
    }

    #endregion 接口
}