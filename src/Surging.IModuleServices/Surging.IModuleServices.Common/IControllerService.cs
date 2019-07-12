using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IControllerService" />
    /// </summary>
    [ServiceBundle("Device/{Service}")]
    public interface IControllerService : IServiceKey
    {
        #region 方法

        /// <summary>
        /// The IsOnline
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task<bool> IsOnline(string deviceId);

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="message">The message<see cref="WillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task Publish(string deviceId, WillMessage message);

        #endregion 方法
    }

    #endregion 接口
}