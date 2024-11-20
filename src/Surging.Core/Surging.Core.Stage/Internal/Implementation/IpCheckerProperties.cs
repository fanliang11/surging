using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Stage.Internal.Implementation
{
    public class IpCheckerProperties
    { 
        public string RoutePathPattern { get; set; }

        public string WhiteIpAddress { get; set; }

        public string  BlackIpAddress { get; set; }

    }
}
