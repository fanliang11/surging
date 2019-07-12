using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IEchoService" />
    /// </summary>
    [ServiceBundle("")]
    public interface IEchoService : IServiceKey
    {
        #region 方法

        /// <summary>
        /// The Locate
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <returns>The <see cref="Task{IpAddressModel}"/></returns>
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task<IpAddressModel> Locate(string key, string routePath);

        #endregion 方法
    }

    #endregion 接口
}