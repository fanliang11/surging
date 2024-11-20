using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    public abstract class AuthorizationFilterAttribute : FilterAttribute,IAuthorizationFilter, IFilter
    {
        public virtual Task<bool> OnAuthorization(ServiceRouteContext context)
        {
            return Task.FromResult(true);
        }

        public virtual async Task ExecuteAuthorizationFilterAsync(ServiceRouteContext serviceRouteContext, CancellationToken cancellationToken)
        {
            var result = await OnAuthorization(serviceRouteContext);
            if (!result)
            {
                serviceRouteContext.ResultMessage.StatusCode = 401;
                serviceRouteContext.ResultMessage.ExceptionMessage = "令牌验证失败.";
            }

        }
    }
}
