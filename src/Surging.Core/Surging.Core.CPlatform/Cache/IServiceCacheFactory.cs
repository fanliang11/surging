using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Cache
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceCacheFactory" />
    /// </summary>
    public interface IServiceCacheFactory
    {
        #region 方法

        /// <summary>
        /// 根据服务路由描述符创建服务路由。
        /// </summary>
        /// <param name="descriptors">服务路由描述符。</param>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceCache>> CreateServiceCachesAsync(IEnumerable<ServiceCacheDescriptor> descriptors);

        #endregion 方法
    }

    #endregion 接口
}