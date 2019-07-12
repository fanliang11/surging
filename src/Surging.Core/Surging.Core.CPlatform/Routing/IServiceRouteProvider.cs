using Surging.Core.CPlatform.Routing;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    #region 接口

    /// <summary>
    /// 服务路由接口
    /// </summary>
    public interface IServiceRouteProvider
    {
        #region 方法

        /// <summary>
        /// 根据服务路由路径获取路由信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        ValueTask<ServiceRoute> GetRouteByPath(string path);

        /// <summary>
        /// The GetRouteByPathRegex
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{ServiceRoute}"/></returns>
        ValueTask<ServiceRoute> GetRouteByPathRegex(string path);

        /// <summary>
        /// 根据服务id找到相关服务信息
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        Task<ServiceRoute> Locate(string serviceId);

        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="processorTime"></param>
        /// <returns></returns>
        Task RegisterRoutes(decimal processorTime);

        /// <summary>
        /// 根据服务路由路径找到相关服务信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<ServiceRoute> SearchRoute(string path);

        #endregion 方法
    }

    #endregion 接口
}