using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal.Cluster.HealthChecks
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IHealthCheckService" />
    /// </summary>
    public interface IHealthCheckService
    {
        #region 方法

        /// <summary>
        /// The IsHealth
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        /// <returns>The <see cref="ValueTask{bool}"/></returns>
        ValueTask<bool> IsHealth(AddressModel address);

        /// <summary>
        /// The Monitor
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        void Monitor(AddressModel address);

        #endregion 方法
    }

    #endregion 接口
}