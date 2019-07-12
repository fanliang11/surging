using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Filters
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IAuthorizationFilter" />
    /// </summary>
    public interface IAuthorizationFilter : IFilter
    {
        #region 方法

        /// <summary>
        /// The OnAuthorization
        /// </summary>
        /// <param name="serviceRouteContext">The serviceRouteContext<see cref="AuthorizationFilterContext"/></param>
        void OnAuthorization(AuthorizationFilterContext serviceRouteContext);

        #endregion 方法
    }

    #endregion 接口
}