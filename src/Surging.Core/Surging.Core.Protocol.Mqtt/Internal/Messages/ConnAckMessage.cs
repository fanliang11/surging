using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class ConnAckMessage:MqttMessage
    {
        public override MessageType MessageType => MessageType.CONNACK;

        public bool SessionPresent { get; set; }

        public ConnReturnCode ReturnCode { get; set; }
    }
}
