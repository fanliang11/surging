using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.WebService.Runtime
{
    public class WebServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public Type BaseType { get; set; }

        public WebServiceBehavior Behavior { get; set; }
    }
}
