using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public abstract class RespondDeviceMessage<T>: CommonDeviceMessage<T> where T : IDeviceMessageReply
    {
        public abstract T NewReply();
        
    }
}
