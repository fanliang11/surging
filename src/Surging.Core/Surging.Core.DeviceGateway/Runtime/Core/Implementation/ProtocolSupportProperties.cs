using Surging.Core.CPlatform.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class ProtocolSupportProperties: IProtocolSupport
    {
        public string Script { get; set; }
        public MessageTransport Transport { get; set; }
    }
}
