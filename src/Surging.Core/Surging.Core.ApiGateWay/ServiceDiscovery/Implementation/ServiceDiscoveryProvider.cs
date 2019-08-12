
using System.Collections.Generic;
using Surging.Core.CPlatform.Routing;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using System.Linq;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 服务发现提供者
    /// </summary>
    public class ServiceDiscoveryProvider : IServiceDiscoveryProvider
    {
        public ServiceDiscoveryProvider()
        {

        }
        public async Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null)
        {
            var result = new List<ServiceAddressModel>();
            var addresses = await ServiceLocator.GetService<IServiceRouteManager>().GetAddressAsync(condition);
            foreach (var address in addresses)
            {
                result.Add(new ServiceAddressModel
                {
                    Address = address,
                    IsHealth = await ServiceLocator.GetService<IHealthCheckService>().IsHealth(address)
                });
            }
            return result;
        }

        public async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null)
        {
            return await ServiceLocator.GetService<IServiceRouteManager>().GetServiceDescriptorAsync(address, condition);
        }

        public async Task EditServiceToken(AddressModel address)
        {
            var routes = await ServiceLocator.GetService<IServiceRouteManager>().GetRoutesAsync(address.ToString());
            routes = routes.ToList();
            List<ServiceRoute> serviceRoutes = new List<ServiceRoute>();
            routes.ToList().ForEach(route =>
            {
                var addresses = new List<AddressModel>();

                serviceRoutes.Add(new ServiceRoute()
                {
                    ServiceDescriptor = route.ServiceDescriptor,
                    Address = addresses
                });
            });
            await ServiceLocator.GetService<IServiceRouteManager>().ClearAsync();
            await ServiceLocator.GetService<IServiceRouteManager>().SetRoutesAsync(serviceRoutes);

        }

    }
}
