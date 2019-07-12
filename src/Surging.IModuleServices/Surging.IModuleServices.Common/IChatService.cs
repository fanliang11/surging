using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.Protocol.WS;
using Surging.Core.Protocol.WS.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IChatService" />
    /// </summary>
    [ServiceBundle("Api/{Service}")]
    [BehaviorContract(IgnoreExtensions = true)]
    public interface IChatService : IServiceKey
    {
        #region 方法

        /// <summary>
        /// The SendMessage
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="data">The data<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task SendMessage(string name, string data);

        #endregion 方法
    }

    #endregion 接口
}