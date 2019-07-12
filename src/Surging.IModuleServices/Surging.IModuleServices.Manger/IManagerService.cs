using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Attributes;
using System.Threading.Tasks;

namespace Surging.IModuleServices.User
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IManagerService" />
    /// </summary>
    [ServiceBundle("api/{Service}")]
    public interface IManagerService : IServiceKey
    {
        #region 方法

        /// <summary>
        /// The SayHello
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="Task{string}"/></returns>
        [Command(Strategy = StrategyType.Injection, ShuntStrategy = AddressSelectorMode.HashAlgorithm, ExecutionTimeoutInMilliseconds = 2500, BreakerRequestVolumeThreshold = 3, Injection = @"return 1;", RequestCacheEnabled = false)]
        Task<string> SayHello(string name);

        #endregion 方法
    }

    #endregion 接口
}