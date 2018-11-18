using DotNetty.Codecs.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class PubRelMessage:MessageWithId
    {
        public override MessageType MessageType => MessageType.PUBREL;
        public override int Qos => (int)QualityOfService.AtLeastOnce;
    }
}
