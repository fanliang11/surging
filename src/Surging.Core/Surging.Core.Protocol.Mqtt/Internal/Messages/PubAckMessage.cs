using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class PubAckMessage:MessageWithId
    {
        public override MessageType MessageType => MessageType.PUBACK;
    }
}
