using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Autofac;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
   public class HttpRequestFilterAttribute : IActionFilter
    {
        internal const string Http405EndpointDisplayName = "405 HTTP Method Not Supported";
        internal const int Http405EndpointStatusCode = 405;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IServiceEntryLocate _serviceEntryLocate;
        public HttpRequestFilterAttribute()
        {
            _serviceRouteProvider = ServiceLocator.Current.Resolve<IServiceRouteProvider>(); ;
            _serviceEntryLocate = ServiceLocator.Current.Resolve<IServiceEntryLocate>(); ;
        }
        public Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public  async Task OnActionExecuting(ActionExecutingContext filterContext)
        { 
            var serviceEntry= _serviceEntryLocate.Locate(filterContext.Message);
            if (serviceEntry != null)
            {
                var httpMethods = serviceEntry.Methods;
                if (httpMethods.Count()>0 && !httpMethods.Any(p => String.Compare(p, filterContext.Context.Request.Method, true) == 0))
                {
                    filterContext.Result = new HttpResultMessage<object>
                    {
                        IsSucceed = false,
                        StatusCode = Http405EndpointStatusCode,
                        Message = Http405EndpointDisplayName
                    };
                }
            }
            else
            {
                var serviceRoute = await _serviceRouteProvider.GetRouteByPath(filterContext.Message.RoutePath);
                var httpMethods = serviceRoute.ServiceDescriptor.HttpMethod();
                if (!string.IsNullOrEmpty(httpMethods) && !httpMethods.Contains(filterContext.Context.Request.Method))
                {
                    filterContext.Result = new HttpResultMessage<object>
                    {
                        IsSucceed = false,
                        StatusCode = Http405EndpointStatusCode,
                        Message = Http405EndpointDisplayName
                    };
                }
            }
        }
    }
}
