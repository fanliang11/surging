using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GateWay.WebApi
{
    public class ResponeMsg
    {
        public string IsSucceed { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }

    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public CustomExceptionFilterAttribute(
            IHostingEnvironment hostingEnvironment,
            IModelMetadataProvider modelMetadataProvider)
        {
            _hostingEnvironment = hostingEnvironment;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                return;
            }
            var result = ServiceResult<object>.Create(false, errorMessage: "request fail");
            result.StatusCode = 400;
            context.Result = new JsonResult(result);
        }
    }

    public class CustomAuthorizeFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isSuccess = false;
            var _authorizationServerProvider = ServiceLocator.GetService<IAuthorizationServerProvider>();
            var author = context.HttpContext.Request.Headers["Authorization"];
            if(author.Count > 0)
            {
                isSuccess = _authorizationServerProvider.ValidateClientAuthentication(author).Result;
            }
            if (true||!isSuccess)
            {
                context.Result = new UnauthorizedResult();
            }

            /*  var entry =
   Dns.GetHostEntryAsync(context.HttpContext.Connection.RemoteIpAddress)
   .GetAwaiter()
   .GetResult();
              if (!entry.HostName.EndsWith(".MyDomain",
              StringComparison.OrdinalIgnoreCase))
              {
                  context.Result = new UnauthorizedResult();
              }
              */
        }
    }
}

