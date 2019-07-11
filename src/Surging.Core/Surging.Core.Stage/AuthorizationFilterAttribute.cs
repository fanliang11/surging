using Microsoft.AspNetCore.Authorization;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System.Threading.Tasks;

namespace Surging.Core.Stage
{
    public class AuthorizationFilterAttribute : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if(filterContext.Route.ServiceDescriptor.AuthType() == AuthorizationType.JWT.ToString())
            {

            }

        }
    }
}
 
