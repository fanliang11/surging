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
        public virtual bool OnAuthorization(ServiceRouteContext context)
        {
            return context.Route.Address.Any(p => p.Token == context.InvokeMessage.Token || p.DisableAuth==true);
        }

        public virtual void ExecuteAuthorizationFilterAsync(ServiceRouteContext serviceRouteContext, CancellationToken cancellationToken)
        {
            var result = OnAuthorization(serviceRouteContext);
            if (!result)
            {
                serviceRouteContext.ResultMessage.StatusCode = 401;
                serviceRouteContext.ResultMessage.ExceptionMessage = "令牌验证失败.";
            }
        }
    }
}
