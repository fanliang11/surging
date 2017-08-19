using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    public interface IFaultTolerantProvider
    {
        Task<IEnumerable<ServiceCommandDescriptor>> GetCommandDescriptor(string [] serviceIds);
    }
}
