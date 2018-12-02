using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public abstract class MqttBehavior: ServiceBase
    {
        public void Publish(string deviceId, MqttWillMessage willMessage)
        {
            ServiceLocator.GetService<IChannelService>().Publish(deviceId, willMessage);
        }

        public abstract bool Authorized(string username, string password);
    }
}
