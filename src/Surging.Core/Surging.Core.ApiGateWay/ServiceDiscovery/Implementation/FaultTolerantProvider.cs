using Surging.Core.CPlatform.Support;
using Surging.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 容错机制提供者
    /// </summary>
    public class FaultTolerantProvider : ServiceBase, IFaultTolerantProvider
    {
        #region 方法

        /// <summary>
        /// The GetCommandDescriptor
        /// </summary>
        /// <param name="serviceIds">The serviceIds<see cref="string[]"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceCommandDescriptor}}"/></returns>
        public async Task<IEnumerable<ServiceCommandDescriptor>> GetCommandDescriptor(params string[] serviceIds)
        {
            return await GetService<IServiceCommandManager>().GetServiceCommandsAsync(serviceIds);
        }

        /// <summary>
        /// The GetCommandDescriptorByAddress
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceCommandDescriptor}}"/></returns>
        public async Task<IEnumerable<ServiceCommandDescriptor>> GetCommandDescriptorByAddress(string address)
        {
            var services = await GetService<IServiceDiscoveryProvider>().GetServiceDescriptorAsync(address);
            return await GetService<IServiceCommandManager>().GetServiceCommandsAsync(services.Select(p => p.Id).ToArray());
        }

        /// <summary>
        /// The SetCommandDescriptorByAddress
        /// </summary>
        /// <param name="model">The model<see cref="ServiceCommandDescriptor"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SetCommandDescriptorByAddress(ServiceCommandDescriptor model)
        {
            await GetService<IServiceCommandManager>().SetServiceCommandsAsync(new ServiceCommandDescriptor[] { model });
        }

        #endregion 方法
    }
}