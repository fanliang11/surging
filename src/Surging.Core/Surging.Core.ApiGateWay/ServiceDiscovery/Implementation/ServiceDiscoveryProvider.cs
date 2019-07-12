using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 服务发现提供者
    /// </summary>
    public class ServiceDiscoveryProvider : ServiceBase, IServiceDiscoveryProvider
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDiscoveryProvider"/> class.
        /// </summary>
        public ServiceDiscoveryProvider()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The EditServiceToken
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task EditServiceToken(AddressModel address)
        {
            var routes = await GetService<IServiceRouteManager>().GetRoutesAsync(address.ToString());
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
            await GetService<IServiceRouteManager>().ClearAsync();
            await GetService<IServiceRouteManager>().SetRoutesAsync(serviceRoutes);
        }

        /// <summary>
        /// The GetAddressAsync
        /// </summary>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceAddressModel}}"/></returns>
        public async Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null)
        {
            var result = new List<ServiceAddressModel>();
            var addresses = await GetService<IServiceRouteManager>().GetAddressAsync(condition);
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

        /// <summary>
        /// The GetServiceDescriptorAsync
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceDescriptor}}"/></returns>
        public async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null)
        {
            return await GetService<IServiceRouteManager>().GetServiceDescriptorAsync(address, condition);
        }

        #endregion 方法
    }
}