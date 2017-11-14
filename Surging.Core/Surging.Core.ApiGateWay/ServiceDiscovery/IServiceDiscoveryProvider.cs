using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
   public interface IServiceDiscoveryProvider
    {
        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);

        Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null);
        
        Task EditServiceToken(AddressModel address);
    }
}
