using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Messages
{
    public class MqttWillMessage
    {
        public string Topic{ get; set; }

        public string WillMessage { get; set; }


        public bool WillRetain { get; set; }

        public int Qos { get; set; }
    }
}
