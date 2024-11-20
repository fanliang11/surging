using Surging.Core.DeviceGateway.Runtime.Device.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public interface IDeviceMessage:IMessage
    {
        ConcurrentDictionary<String, Object> Headers { get; set; }
        string DeviceId { get; set; }

        MessageType MessageType { get; set; }
        long? Timestamp { get; set; }

        IDeviceMessage AddHeader(string header, object value);
    }
}
