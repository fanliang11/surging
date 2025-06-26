using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal
{
    [ServiceBundle("Consul")]
    public interface IHandlerService: IServiceKey
    { 
        [HttpPost]
        Task<bool> KeyPrefixWatch();

        [HttpPost]
        Task<bool> KeyWatch(string Key, string Value);
    }
}

