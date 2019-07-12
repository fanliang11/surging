using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceDiscoveryProvider" />
    /// </summary>
    public interface IServiceDiscoveryProvider
    {
        #region 方法

        /// <summary>
        /// The EditServiceToken
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task EditServiceToken(AddressModel address);

        /// <summary>
        /// The GetAddressAsync
        /// </summary>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceAddressModel}}"/></returns>
        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);

        /// <summary>
        /// The GetServiceDescriptorAsync
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceDescriptor}}"/></returns>
        Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null);

        #endregion 方法
    }

    #endregion 接口
}