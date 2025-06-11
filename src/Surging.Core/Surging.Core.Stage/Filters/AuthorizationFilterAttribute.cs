using Microsoft.AspNetCore.Authorization;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System.Threading.Tasks;
using Autofac;
using System;
using Surging.Core.KestrelHttpServer.Internal;

namespace Surging.Core.Stage.Filters
{
    public class AuthorizationFilterAttribute : IAuthorizationFilter
    {
        private readonly IAuthorizationServerProvider _authorizationServerProvider;
        public AuthorizationFilterAttribute()
        {
            _authorizationServerProvider = ServiceLocator.Current.Resolve<IAuthorizationServerProvider>();
        }
        public async Task OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var gatewayAppConfig = AppConfig.Options.ApiGetWay;
            if (filterContext.Route != null && filterContext.Route.ServiceDescriptor.DisableNetwork())
                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
            else
            {
                if (filterContext.Route != null && filterContext.Route.ServiceDescriptor.EnableAuthorization())
                {
                    if (filterContext.Route.ServiceDescriptor.AuthType() == AuthorizationType.JWT.ToString())
                    {
                        var author = filterContext.Context.Request.Headers["Authorization"];
                        if (author.Count > 0)
                        {
                            var isSuccess = await _authorizationServerProvider.ValidateClientAuthentication(author);
                            if (!isSuccess)
                            {
                                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                            }
                            else
                            {
                                var payload = _authorizationServerProvider.GetPayloadString(author);
                                RestContext.GetContext().SetAttachment("payload", payload);
                            }
                        }
                        else
                            filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };

                    }
                }
            }
          
            if (String.Compare(filterContext.Path.ToLower(), gatewayAppConfig.TokenEndpointPath, true) == 0)
                filterContext.Context.Items.Add("path", gatewayAppConfig.AuthorizationRoutePath);

        }
    }
}
 
