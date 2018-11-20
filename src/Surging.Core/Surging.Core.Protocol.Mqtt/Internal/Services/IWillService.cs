using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public interface IWillService
    {
        void Add(string deviceid, MqttWillMessage willMessage);

        void SendWillMessage(string deviceId);

        void Remove(string deviceid);
    }
}
