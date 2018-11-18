using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class PingReqMessage:MqttMessage
    {
        public static readonly PingReqMessage Instance = new PingReqMessage();

        public override MessageType MessageType => MessageType.PINGREQ;
    }
}
