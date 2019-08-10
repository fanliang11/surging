using Autofac;
using Microsoft.AspNetCore.Http;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Stage.Filters
{
    public class ActionFilterAttribute : IActionFilter
    {
        private readonly IAuthorizationServerProvider _authorizationServerProvider;
        public ActionFilterAttribute()
        {
            _authorizationServerProvider = ServiceLocator.Current.Resolve<IAuthorizationServerProvider>();
        }

        public Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public async Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            var gatewayAppConfig = AppConfig.Options.ApiGetWay;
            if (filterContext.Message.RoutePath == gatewayAppConfig.AuthorizationRoutePath)
            {
                var token = await _authorizationServerProvider.GenerateTokenCredential(new Dictionary<string, object>(filterContext.Message.Parameters));
                if (token != null)
                {
                    filterContext.Result = HttpResultMessage<object>.Create(true, token);
                    filterContext.Result.StatusCode = (int)ServiceStatusCode.Success;
                }
                else
                {
                    filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                }
            }
            else if (filterContext.Route.ServiceDescriptor.AuthType() == AuthorizationType.AppSecret.ToString())
            {
                if (!ValidateAppSecretAuthentication(filterContext, out HttpResultMessage<object> result))
                {
                    filterContext.Result = result;
                }
            }
        }

        private bool ValidateAppSecretAuthentication(ActionExecutingContext filterContext, out HttpResultMessage<object> result)
        {
            bool isSuccess = true;
            DateTime time;
            result = HttpResultMessage<object>.Create(true,null);
            var author = filterContext.Context.Request.Headers["Authorization"];
            var model = filterContext.Message.Parameters;
            var route = filterContext.Route;
            if (model.ContainsKey("timeStamp") && author.Count > 0)
            {
                if (long.TryParse(model["timeStamp"].ToString(), out long timeStamp))
                {
                    time = DateTimeConverter.UnixTimestampToDateTime(timeStamp);
                    var seconds = (DateTime.Now - time).TotalSeconds;
                    if (seconds <= 3560 && seconds >= 0)
                    {
                        if (GetMD5($"{route.ServiceDescriptor.Token}{time.ToString("yyyy-MM-dd hh:mm:ss") }") != author.ToString())
                        {
                            result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                        isSuccess = false;
                    }
                }
                else
                {
                    result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                    isSuccess = false;
                }
            }
            else
            {
                result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
                isSuccess = false;
            }
            return isSuccess;
        }

        public  string GetMD5(string encypStr)
        {
            try
            {
                var md5 = MD5.Create();
                var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(encypStr));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("X2"));
                } 
                return sb.ToString().ToLower();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
