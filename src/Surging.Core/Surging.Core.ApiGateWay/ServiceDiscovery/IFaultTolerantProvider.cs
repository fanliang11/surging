using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IFaultTolerantProvider" />
    /// </summary>
    public interface IFaultTolerantProvider
    {
        #region 方法

        /// <summary>
        /// The GetCommandDescriptor
        /// </summary>
        /// <param name="serviceIds">The serviceIds<see cref="string []"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceCommandDescriptor}}"/></returns>
        Task<IEnumerable<ServiceCommandDescriptor>> GetCommandDescriptor(params string[] serviceIds);

        /// <summary>
        /// The GetCommandDescriptorByAddress
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceCommandDescriptor}}"/></returns>
        Task<IEnumerable<ServiceCommandDescriptor>> GetCommandDescriptorByAddress(string address);

        /// <summary>
        /// The SetCommandDescriptorByAddress
        /// </summary>
        /// <param name="model">The model<see cref="ServiceCommandDescriptor"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SetCommandDescriptorByAddress(ServiceCommandDescriptor model);

        #endregion 方法
    }

    #endregion 接口
}