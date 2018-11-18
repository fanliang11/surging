using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
   public abstract class MessageWithId: MqttMessage
    {
        public int MessageId { get; set; }
    }
}
