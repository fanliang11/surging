using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
   public class SubAckMessage: MessageWithId
    {
        public override MessageType MessageType => MessageType.SUBACK;
        public IReadOnlyList<int> ReturnCodes { get; set; }
    }
}
