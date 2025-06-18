using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public class DeviceOnlineMessage : CommonDeviceMessage<DeviceOnlineMessage>
    {
       public  static  HeaderKey<string> LoginToken = new HeaderKey<string>("token", null);
        public override MessageType MessageType { get; set; } = MessageType.ONLINE;
    }
}
