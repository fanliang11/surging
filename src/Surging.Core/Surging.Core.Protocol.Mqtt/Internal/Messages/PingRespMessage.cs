using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class PingRespMessage: MqttMessage
    {
        public static readonly PingRespMessage Instance = new PingRespMessage();

        public override MessageType MessageType => MessageType.PINGRESP;
    }
}
