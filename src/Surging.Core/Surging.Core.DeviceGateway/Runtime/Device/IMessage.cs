using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public  interface IMessage
    {
        public string MessageId { get; internal set; }
    }
}
