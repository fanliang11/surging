using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceHeartbeatManager" />
    /// </summary>
    public interface IServiceHeartbeatManager
    {
        #region 方法

        /// <summary>
        /// The AddWhitelist
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        void AddWhitelist(string serviceId);

        /// <summary>
        /// The ExistsWhitelist
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool ExistsWhitelist(string serviceId);

        #endregion 方法
    }

    #endregion 接口
}