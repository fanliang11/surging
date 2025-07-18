using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{
    public class MessageEncodeContext
    {
      public IDeviceMessage Message  { get; set; } 

       public  Action<IDeviceMessage> Reply { get; set; }

        public MessageEncodeContext(IDeviceMessage message, Action<IDeviceMessage> func)
        {
            Message = message;
            Reply = func;
        }
    }
}
