
using System.Collections.Generic;
using Surging.Core.CPlatform.Routing;
using System.Threading.Tasks;
using Surging.Core.System;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    public class ZookeeperServiceDiscoveryProvider : ServiceBase, IServiceDiscoveryProvider
    {
        public ZookeeperServiceDiscoveryProvider()
        {

        }
        public async Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null)
        {
            var result = new List<ServiceAddressModel>();
            var addresses= await GetService<IServiceRouteManager>().GetAddressAsync(condition);
            foreach (var address in addresses)
            {
                result.Add(new ServiceAddressModel
                {
                    Address = address,
                    IsHealth = await GetService<IHealthCheckService>().IsHealth(address)
                });
            }
            return result;
        }

        public async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null)
        {
            return await GetService<IServiceRouteManager>().GetServiceDescriptorAsync(address,condition);
        }

    }
}
