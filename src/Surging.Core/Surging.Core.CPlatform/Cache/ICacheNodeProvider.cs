using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Cache
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ICacheNodeProvider" />
    /// </summary>
    public interface ICacheNodeProvider
    {
        #region 方法

        /// <summary>
        /// The GetServiceCaches
        /// </summary>
        /// <returns>The <see cref="IEnumerable{ServiceCache}"/></returns>
        IEnumerable<ServiceCache> GetServiceCaches();

        #endregion 方法
    }

    #endregion 接口
}