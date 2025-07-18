using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Event
{
    public class EventMessage : CommonDeviceMessage<EventMessage>
    {
        public override MessageType MessageType { get; set; } = MessageType.EVENT;

        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();
        public string EventId { get; set; }


    }
}
