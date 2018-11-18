using DotNetty.Codecs.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
   public class SubscribeMessage: MessageWithId
    {
        public SubscribeMessage(int messageId, params SubscriptionRequestData[] requests)
        {
            this.MessageId = messageId;
            this.Requests = requests;
        }
        public override MessageType MessageType => MessageType.SUBSCRIBE;
        public override int Qos => (int)QualityOfService.AtLeastOnce;

        public IReadOnlyList<SubscriptionRequestData> Requests { get; set; }
    }
}
