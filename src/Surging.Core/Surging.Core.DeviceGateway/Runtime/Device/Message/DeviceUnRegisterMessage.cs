using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public class DeviceUnRegisterMessage : CommonDeviceMessage<DeviceUnRegisterMessage>
    {
        public override MessageType MessageType { get; set; } = MessageType.UN_REGISTER;
    }
}
