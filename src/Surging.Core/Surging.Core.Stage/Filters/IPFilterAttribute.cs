﻿using Microsoft.AspNetCore.Http;
using Surging.Core.ApiGateWay;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using Surging.Core.KestrelHttpServer.Internal;
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
        private readonly IIPChecker _ipChecker;
        public IPFilterAttribute()
        { 
            _ipChecker = ServiceLocator.GetService<IIPChecker>();
        }
        public  Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            var address = ServiceLocator.GetService<IHttpContextAccessor>().HttpContext.Connection.RemoteIpAddress;
            RestContext.GetContext().SetAttachment("RemoteIpAddress", address.ToString());
            if (_ipChecker.IsBlackIp(address,filterContext.Message.RoutePath))
            {
                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Your IP address is not allowed" };
            }
             return Task.CompletedTask;
        }
    }
}
