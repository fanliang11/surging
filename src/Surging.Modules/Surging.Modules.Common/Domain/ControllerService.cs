using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Interceptors;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class ControllerService : MqttBehavior, IControllerService
    {
        

        public override  Task<bool> Authorized(string clientId, string username, string password)
        {
            bool result = false;
            if (username == "admin" && password == "123456")
                result = true;
            return   Task.FromResult(result);
        }

        public async Task<bool> IsOnline(string deviceId)
        {
            var text = await GetService<IManagerService>().SayHello("fanly");
            return  await base.GetDeviceIsOnine(deviceId);
        }

        public override Task<bool> Load(MqttServiceContext context)
        {
            throw new NotImplementedException();
        }

        public async Task Publish(string deviceId, WillMessage message)
        {
            var willMessage = new MqttWillMessage
            {
                WillMessage = message.Message,
                Qos = message.Qos,
                Topic = message.Topic,
                WillRetain = message.WillRetain
            };
            await Publish(deviceId, willMessage);
            await RemotePublish(deviceId, willMessage);
        }
    }
}
