using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    /// <summary>
    /// Defines the <see cref="MqttRomtePublishService" />
    /// </summary>
    public class MqttRomtePublishService : ServiceBase, IMqttRomtePublishService
    {
        #region 方法

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="message">The message<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Publish(string deviceId, MqttWillMessage message)
        {
            await ServiceLocator.GetService<IChannelService>().Publish(deviceId, message);
        }

        #endregion 方法
    }
}