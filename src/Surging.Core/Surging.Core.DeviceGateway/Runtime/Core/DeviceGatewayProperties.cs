using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public class DeviceGatewayProperties
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Provider { get; set; }

        public string ChannelId { get; set; }

        public string Protocol { get; set; }

        public string Transport { get; set; }


        public IDictionary<string, object> Configuration { get; set; }


    }
}
