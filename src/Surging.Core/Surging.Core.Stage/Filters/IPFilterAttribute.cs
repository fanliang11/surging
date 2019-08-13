using Microsoft.AspNetCore.Http;
using Surging.Core.ApiGateWay;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using Surging.Core.Stage.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Stage.Filters
{
    public class IPFilterAttribute : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIPChecker _ipChecker;
        public IPFilterAttribute(IHttpContextAccessor httpContextAccessor, IIPChecker ipChecker)
        {
            _httpContextAccessor = httpContextAccessor;
            _ipChecker = ipChecker;
        }
        public  Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public    Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            var address = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            RpcContext.GetContext().SetAttachment("RemoteIpAddress", address.ToString());
            if (_ipChecker.IsBlackIp(address,filterContext.Message.RoutePath))
            {
                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Your IP address is not allowed" };
            }
             return Task.CompletedTask;
        }
    }
}
