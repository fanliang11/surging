using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class DisconnectMessage:MqttMessage
    {
        public static readonly DisconnectMessage Instance = new DisconnectMessage();

        public override MessageType MessageType => MessageType.DISCONNECT;
    }
}
