using Microsoft.AspNetCore.Authorization;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System.Threading.Tasks;

namespace Surging.Core.Stage
{
    /// <summary>
    /// Defines the <see cref="AuthorizationFilterAttribute" />
    /// </summary>
    public class AuthorizationFilterAttribute : IAuthorizationFilter
    {
        #region 方法

        /// <summary>
        /// The OnAuthorization
        /// </summary>
        /// <param name="filterContext">The filterContext<see cref="AuthorizationFilterContext"/></param>
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext.Route.ServiceDescriptor.AuthType() == AuthorizationType.JWT.ToString())
            {
            }
        }

        #endregion 方法
    }
}