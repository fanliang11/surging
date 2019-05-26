using Surging.Core.DNS.Runtime;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class DnsService : DnsBehavior, IDnsService
    {
        public override Task<IPAddress> Resolve(string domainName)
        {
            if(domainName=="localhost")
            {
                return Task.FromResult<IPAddress>(IPAddress.Parse("127.0.0.1"));
            }
            return Task.FromResult<IPAddress>(null);
        }
    }
}
