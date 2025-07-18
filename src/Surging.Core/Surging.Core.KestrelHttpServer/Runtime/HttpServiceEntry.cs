using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Runtime
{
    public class HttpServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public Func<HttpBehavior> Behavior { get; set; }
    }
}
