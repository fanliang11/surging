using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    public interface IServiceSubscribeProvider
    {
        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);

        Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null);
    }
}
