using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Stage.Internal.Implementation
{
   public class IPNetworkSegment
    {
        public IPAddress LastUsable { get; set; }
        public long LongLastUsable { get; set; }
        public byte Cidr { get; set; }
        public IPAddress FirstUsable { get; set; }
        public long LongFirstUsable { get; set; }
    }
}
