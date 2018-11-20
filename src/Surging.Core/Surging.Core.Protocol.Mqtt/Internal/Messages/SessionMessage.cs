using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Messages
{
   public class SessionMessage
    {
        public string Message { get; set; } 

        private int QoS { get; set; }

        private string TopicName { get; set; }

    }
}
