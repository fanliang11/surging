using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public  interface IDeviceMessageListenerManager
    {
        public IDeviceMessageListener Register(string messageId);
        public IDeviceMessageListener Unregister(string messageId);
    
        public IDeviceMessageListener GetMessageListener(string messageId);
    
    }
}
