using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceRegisterProvider" />
    /// </summary>
    public interface IServiceRegisterProvider
    {
        #region 方法

        /// <summary>
        /// The GetAddressAsync
        /// </summary>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceAddressModel}}"/></returns>
        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);

        #endregion 方法
    }

    #endregion 接口
}