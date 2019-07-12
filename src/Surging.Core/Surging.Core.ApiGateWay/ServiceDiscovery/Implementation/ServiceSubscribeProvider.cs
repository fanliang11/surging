using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 服务订阅提供者
    /// </summary>
    public class ServiceSubscribeProvider : ServiceBase, IServiceSubscribeProvider
    {
        #region 方法

        /// <summary>
        /// The GetAddressAsync
        /// </summary>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceAddressModel}}"/></returns>
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

        /// <summary>
        /// The GetServiceDescriptorAsync
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceDescriptor}}"/></returns>
        public async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null)
        {
            return await GetService<IServiceSubscribeManager>().GetServiceDescriptorAsync(address, condition);
        }

        #endregion 方法
    }
}