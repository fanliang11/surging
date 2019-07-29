using Microsoft.AspNetCore.Http;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
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
        public IPFilterAttribute(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public  Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            var address = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            IPNetwork ipnetwork = IPNetwork.Parse("192.168.0.1/24");
            var startUsableIP= ipnetwork.FirstUsable;
            var endUsableIP = ipnetwork.LastUsable;
            return Task.CompletedTask;
        }
    }
}
