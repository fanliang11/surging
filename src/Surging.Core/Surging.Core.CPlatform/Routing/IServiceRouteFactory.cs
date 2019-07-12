using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务路由工厂。
    /// </summary>
    public interface IServiceRouteFactory
    {
        #region 方法

        /// <summary>
        /// 根据服务路由描述符创建服务路由。
        /// </summary>
        /// <param name="descriptors">服务路由描述符。</param>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> CreateServiceRoutesAsync(IEnumerable<ServiceRouteDescriptor> descriptors);

        #endregion 方法
    }

    #endregion 接口
}