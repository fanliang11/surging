using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Mqtt
{
    public class TopicPayload
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }
    }
}
