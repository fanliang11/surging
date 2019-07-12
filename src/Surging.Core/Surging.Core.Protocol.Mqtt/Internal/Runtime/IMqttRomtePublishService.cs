using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMqttRomtePublishService" />
    /// </summary>
    [ServiceBundle("Device")]
    public interface IMqttRomtePublishService : IServiceKey
    {
        #region 方法

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="message">The message<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task Publish(string deviceId, MqttWillMessage message);

        #endregion 方法
    }

    #endregion 接口
}