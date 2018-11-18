using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
   public class SubscriptionRequestData
    {
        public SubscriptionRequestData(string topicFilter, int qos)
        { 
            this.TopicFilter = topicFilter;
            this.Qos = qos;
        }
        public string TopicFilter { get;}

        public int Qos { get; }
    }
}
