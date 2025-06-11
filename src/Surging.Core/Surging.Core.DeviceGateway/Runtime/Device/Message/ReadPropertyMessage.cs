using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public class ReadPropertyMessage : CommonDeviceMessage<ReadPropertyMessage>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY;

        public List<String> Properties { get; set; }=new List<String>();
    }
}
