using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public abstract class MqttBehavior: ServiceBase
    {
        public async Task Publish(string deviceId, MqttWillMessage willMessage)
        {
           await ServiceLocator.GetService<IChannelService>().Publish(deviceId, willMessage);
        }

        public abstract Task<bool> Authorized(string username, string password);
    }
}
