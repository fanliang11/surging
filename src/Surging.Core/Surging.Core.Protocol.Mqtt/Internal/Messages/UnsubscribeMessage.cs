using DotNetty.Codecs.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class UnsubscribeMessage : MessageWithId
    {
        public override MessageType MessageType => MessageType.UNSUBSCRIBE;
        public UnsubscribeMessage(int messageId, params string[] topicFilters)
        {
            this.MessageId = messageId;
            this.TopicFilters = topicFilters;
        }

        public override int Qos => (int)QualityOfService.AtLeastOnce;

        public IEnumerable<string> TopicFilters { get; set; }
    }
}
