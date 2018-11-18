using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class UnsubAckMessage : MessageWithId
    {
        public override MessageType MessageType => MessageType.UNSUBACK;
    }
}
