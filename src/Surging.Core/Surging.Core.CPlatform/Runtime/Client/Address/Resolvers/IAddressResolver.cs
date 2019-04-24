using Surging.Core.CPlatform.Address;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers
{
    /// <summary>
    /// 一个抽象的服务地址解析器。
    /// </summary>
    public interface IAddressResolver
    {
        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        ValueTask<AddressModel> Resolver(string serviceId, string item);
    }
}