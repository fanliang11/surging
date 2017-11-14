using Surging.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    public class ServiceSubscribeProvider : ServiceBase, IServiceSubscribeProvider
    {
        public async Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null)
        {
            var result = new List<ServiceAddressModel>();
            var addresses = await GetService<IServiceSubscribeManager>().GetAddressAsync(condition);
            foreach (var address in addresses)
            {
                result.Add(new ServiceAddressModel
                {
                    Address = address,
                });
            }
            return result;
        }

        public async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null)
        {
            return await GetService<IServiceSubscribeManager>().GetServiceDescriptorAsync(address, condition);
        }
    }
}
