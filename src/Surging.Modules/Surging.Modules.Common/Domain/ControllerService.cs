using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class ControllerService : MqttBehavior, IControllerService
    {
        public override async Task<bool> Authorized(string username, string password)
        {
            bool result = false;
            if (username == "admin" && password == "123456")
                result= true;
            return await Task.FromResult(result);
        }

       public async Task<bool> IsOnline(string deviceId)
        {
            var status= await base.GetDeviceStatus(deviceId);
            return status == SessionStatus.OPEN;
        }

        public async Task Publish(string deviceId, WillMessage message)
        {
            await Publish(deviceId, new MqttWillMessage
            {
                WillMessage = message.Message,
                Qos = message.Qos,
                Topic = message.Topic,
                WillRetain = message.WillRetain
            });
        }
    }
}
