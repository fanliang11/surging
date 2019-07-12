using Surging.Core.CPlatform.Address;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务地址解析器。
    /// </summary>
    public interface IAddressResolver
    {
        #region 方法

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <param name="item">The item<see cref="string"/></param>
        /// <returns>服务地址模型。</returns>
        ValueTask<AddressModel> Resolver(string serviceId, string item);

        #endregion 方法
    }

    #endregion 接口
}