using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.DNS
{
   public class DnsTransportMessage
    {
        public IDnsResponse DnsResponse { get; set; }

        public IDnsQuestion DnsQuestion { get; set; }

        public IPAddress Address { get; set; }
    }
}
