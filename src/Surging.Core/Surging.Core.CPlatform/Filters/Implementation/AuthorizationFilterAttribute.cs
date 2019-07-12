using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    /// <summary>
    /// Defines the <see cref="AuthorizationFilterAttribute" />
    /// </summary>
    public abstract class AuthorizationFilterAttribute : FilterAttribute, IAuthorizationFilter, IFilter
    {
        #region 方法

        /// <summary>
        /// The ExecuteAuthorizationFilterAsync
        /// </summary>
        /// <param name="serviceRouteContext">The serviceRouteContext<see cref="ServiceRouteContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        public virtual void ExecuteAuthorizationFilterAsync(ServiceRouteContext serviceRouteContext, CancellationToken cancellationToken)
        {
            var result = OnAuthorization(serviceRouteContext);
            if (!result)
            {
                serviceRouteContext.ResultMessage.StatusCode = 401;
                serviceRouteContext.ResultMessage.ExceptionMessage = "令牌验证失败.";
            }
        }

        /// <summary>
        /// The OnAuthorization
        /// </summary>
        /// <param name="context">The context<see cref="ServiceRouteContext"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public virtual bool OnAuthorization(ServiceRouteContext context)
        {
            return true;
        }

        #endregion 方法
    }
}