using Surging.Core.CPlatform.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    /// <summary>
    /// 服务路由接口
    /// </summary>
    public interface IServiceRouteProvider
    {
        /// <summary>
        /// 根据服务id找到相关服务信息
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        Task<ServiceRoute> Locate(string serviceId);

        ValueTask<ServiceRoute> GetLocalRouteByPathRegex(string path);
        /// <summary>
        /// 根据服务路由路径获取路由信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        ValueTask<ServiceRoute> GetRouteByPath(string path);

        /// <summary>
        /// 根据服务路由路径找到相关服务信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<ServiceRoute> SearchRoute(string path);

        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="processorTime"></param>
        /// <returns></returns>
        Task RegisterRoutes(decimal processorTime);

        ValueTask<ServiceRoute> GetRouteByPathRegex(string path);
    }
}
