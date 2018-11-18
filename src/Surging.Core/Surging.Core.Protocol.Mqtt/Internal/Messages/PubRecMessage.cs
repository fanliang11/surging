using DotNetty.Codecs.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class PubRecMessage:MessageWithId
    {
        public override MessageType MessageType => MessageType.PUBREC;
    }
}
