using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
   public interface IMqttBehaviorProvider
    {
        MqttBehavior GetMqttBehavior();
    }
}
