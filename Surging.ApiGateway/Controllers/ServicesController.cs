using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Surging.Core.ApiGateWay;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform;
using System.Diagnostics;

namespace Surging.ApiGateway.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private readonly IServiceRouteProvider _serviceRouteProvider;

        public ServicesController()
        {
            _serviceProxyProvider = ServiceLocator.GetService<IServiceProxyProvider>();
            _serviceRouteProvider = ServiceLocator.GetService<IServiceRouteProvider>();
        }
       
        public async Task<ServiceResult<object>> Path(string path, [FromQuery]string serviceKey, [FromBody]Dictionary<string, object> model)
        {
            ServiceResult<object> result = ServiceResult<object>.Create(false,null);
        
            if ( OnAuthorization(path, model,ref result))
            {
                
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    
                    result = ServiceResult<object>.Create(true, await _serviceProxyProvider.Invoke<object>(model, path, serviceKey));
                    result.StatusCode = (int)ServiceStatusCode.Success;
                }
                else
                {
                    result = ServiceResult<object>.Create(true, await _serviceProxyProvider.Invoke<object>(model, path));
                    result.StatusCode = (int)ServiceStatusCode.Success;
                }
            }
            return result;
        }

        private bool OnAuthorization(string path, Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true;
            var author = HttpContext.Request.Headers["Authorization"];
            DateTime time;
            var routes = _serviceRouteProvider.GetRouteByPath(path).Result;
            if (routes.ServiceDescriptor.EnableAuthorization())
            {
                if (routes.Address.Any(p => p.DisableAuth == false))
                {
                    if (!string.IsNullOrEmpty(path) && model.ContainsKey("timeStamp"))
                    {
                        if (DateTime.TryParse(model["timeStamp"].ToString(), out time))
                        {
                            var seconds = (DateTime.Now - time).TotalSeconds;
                            if (seconds <= 3560 && seconds >= 0)
                            {
                                if (!routes.Address.Any(p => GetMD5($"{p.Token}{time.ToString("yyyy-MM-dd hh:mm:ss") }") == author.ToString()))
                                {
                                    result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                                    isSuccess = false;
                                }
                            }
                            else
                            {
                                result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                                isSuccess = false;
                            }
                        }
                        else
                        {
                            result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
                        isSuccess = false;
                    }
                }
            }
            return isSuccess;
        }

        public static string GetMD5(string encypStr)
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
                //所有字符转为大写
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
