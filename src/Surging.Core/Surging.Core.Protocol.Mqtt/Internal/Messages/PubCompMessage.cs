using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
     public class PubCompMessage:MessageWithId
    {
        public override MessageType MessageType => MessageType.PUBCOMP;
    }
}
