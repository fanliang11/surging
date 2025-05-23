using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public enum ConnReturnCode
    {
        Accepted = 0x00,
        RefusedUnacceptableProtocolVersion = 0X01,
        RefusedIdentifierRejected = 0x02,
        RefusedServerUnavailable = 0x03,
        RefusedBadUsernameOrPassword = 0x04,
        RefusedNotAuthorized = 0x05
    }
}
