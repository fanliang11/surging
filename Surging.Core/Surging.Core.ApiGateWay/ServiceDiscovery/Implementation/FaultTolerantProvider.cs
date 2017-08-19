using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Support;
using Surging.Core.System;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    public class FaultTolerantProvider : ServiceBase, IFaultTolerantProvider
    {
        public async Task<IEnumerable<ServiceCommandDescriptor>> GetCommandDescriptor(string[] serviceIds)
        {
             return await GetService<IServiceCommandManager>().GetServiceCommandsAsync(serviceIds);
        }
    }
}
