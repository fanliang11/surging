using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class DeviceMessageListenerManager : IDeviceMessageListenerManager
    {
        private static ConcurrentDictionary<string, IDeviceMessageListener> _deviceMessageListener =
         new ConcurrentDictionary<string, IDeviceMessageListener>();
        public IDeviceMessageListener GetMessageListener(string messageId)
        {
            IDeviceMessageListener deviceMessageListener = null;
            try
            { 
                 _deviceMessageListener.TryGetValue(messageId, out deviceMessageListener); 
                    return deviceMessageListener; 
            }
            finally
            {
                if (deviceMessageListener == null)
                    _deviceMessageListener.TryRemove(messageId,out IDeviceMessageListener deviceMessageListener1);
            }
        }

        public  IDeviceMessageListener Register(string messageId)
        {
            var deviceMessageListener = new DeviceMessageListener();
            _deviceMessageListener.AddOrUpdate(messageId,  deviceMessageListener, (k, v) =>deviceMessageListener);
            return deviceMessageListener;
        }

        public IDeviceMessageListener Unregister(string messageId)
        { 
            _deviceMessageListener.TryRemove(messageId, out IDeviceMessageListener deviceMessageListener);
            return deviceMessageListener;
        }
    }
}
