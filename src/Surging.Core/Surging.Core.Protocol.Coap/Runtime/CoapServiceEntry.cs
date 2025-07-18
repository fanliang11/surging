using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Coap.Runtime
{
    public class CoapServiceEntry
    {
        public string Path { get; set; }

        public Type Service { get; set; }
        public Type Type { get; set; }
        public CoapBehavior Behavior { get; set; }
    }
}
