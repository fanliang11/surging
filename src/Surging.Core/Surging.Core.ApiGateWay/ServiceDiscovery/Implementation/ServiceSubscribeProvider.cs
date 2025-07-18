
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 服务订阅提供者
    /// </summary>
    public class ServiceSubscribeProvider :IServiceSubscribeProvider
    {
        public async Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null)
        {
            var result = new List<ServiceAddressModel>();
            var addresses = await ServiceLocator.GetService<IServiceSubscribeManager>().GetAddressAsync(condition);
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
            return await ServiceLocator.GetService<IServiceSubscribeManager>().GetServiceDescriptorAsync(address, condition);
        }
    }
}
