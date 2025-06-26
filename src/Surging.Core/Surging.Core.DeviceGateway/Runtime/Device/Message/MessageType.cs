using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public enum MessageType
    {
        READ_PROPERTY = 1,
        ONLINE = 2,
        READ_PROPERTY_REPLY = 3,
        REPORT_PROPERTY = 4,
        REGISTER = 12,
        UN_REGISTER = 5,
        CALL_FUNCTION = 6,
        CALL_FUNCTION_REPLY = 7,
        EVENT = 8,
        WRITE_PROPERTY = 10,
        WRITE_PROPERTY_REPLY = 11,
        DISCONNECT = 9,
        SUBSCRIBE = 101,
    }
}
