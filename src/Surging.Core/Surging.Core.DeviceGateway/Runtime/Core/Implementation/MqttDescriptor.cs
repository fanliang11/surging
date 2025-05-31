using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class MqttDescriptor : ServiceDescriptor
    {
        public static MqttDescriptor Instance(string path)
        {
            return new MqttDescriptor() { RoutePath = path, Id = path };
        }
    }
}