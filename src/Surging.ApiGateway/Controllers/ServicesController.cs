using Microsoft.AspNetCore.Mvc;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using GateWayAppConfig = Surging.Core.ApiGateWay.AppConfig;
using System.Reflection;
using Surging.Core.CPlatform.Utilities;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform.Transport.Implementation;

namespace Surging.ApiGateway.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IAuthorizationServerProvider _authorizationServerProvider;

     
        public ServicesController(IServiceProxyProvider serviceProxyProvider, 
            IServiceRouteProvider serviceRouteProvider,
            IAuthorizationServerProvider authorizationServerProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
            _serviceRouteProvider = serviceRouteProvider;
            _authorizationServerProvider = authorizationServerProvider;
        }
       
        public async Task<ServiceResult<object>> Path([FromServices]IServicePartProvider servicePartProvider, string path, [FromBody]Dictionary<string, object> model)
        {
            string serviceKey = this.Request.Query["servicekey"];
            path = path.IndexOf("/") < 0 ? $"/{path}" : path;
            if (model == null)
            {
                model = new Dictionary<string, object>();
            }
            foreach (string n in this.Request.Query.Keys)
            {
                model[n] = this.Request.Query[n].ToString();
            }
            ServiceResult<object> result = ServiceResult<object>.Create(false,null);
            path = path.ToLower() == GateWayAppConfig.TokenEndpointPath.ToLower() ? 
                GateWayAppConfig.AuthorizationRoutePath : path.ToLower();
            if( await GetAllowRequest(path)==false) return new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
            if (servicePartProvider.IsPart(path))
            {
                result = ServiceResult<object>.Create(true, await servicePartProvider.Merge(path, model));
                result.StatusCode = (int)ServiceStatusCode.Success;
            }
            else
            if ( OnAuthorization(path, model,ref result))
            {
                if (path == GateWayAppConfig.AuthorizationRoutePath)
                {
                    var token = await _authorizationServerProvider.GenerateTokenCredential(model);
                    if (token != null)
                    {
                        result = ServiceResult<object>.Create(true, token);
                        result.StatusCode = (int)ServiceStatusCode.Success;
                    }
                    else
                    {
                        result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                    }
                }
                else
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
            }
            return result;
        }

        private async Task<bool> GetAllowRequest(string path)
        { 
            var route = await _serviceRouteProvider.GetRouteByPath(path);
            return !route.ServiceDescriptor.DisableNetwork();
        }

        private bool OnAuthorization(string path, Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true;
            var route = _serviceRouteProvider.GetRouteByPath(path).Result;
            if (route.ServiceDescriptor.EnableAuthorization())
            {
                if(route.ServiceDescriptor.AuthType()== AuthorizationType.JWT.ToString())
                {
                    isSuccess= ValidateJwtAuthentication(route,model, ref result);
                }
                else
                {
                    isSuccess = ValidateAppSecretAuthentication(route, path, model, ref result);
                }

            }
            return isSuccess;
        }

        public bool ValidateJwtAuthentication(ServiceRoute route, Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true; 
            var author = HttpContext.Request.Headers["Authorization"];
            if (author.Count > 0)
            {
                isSuccess = _authorizationServerProvider.ValidateClientAuthentication(author).Result;
                if (!isSuccess)
                {
                    result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                }
                else
                {
                    var keyValue = model.FirstOrDefault();
                    if (!(keyValue.Value is IConvertible) || !typeof(IConvertible).GetTypeInfo().IsAssignableFrom(keyValue.Value.GetType()))
                    {
                        dynamic instance = keyValue.Value;
                        instance.Payload = _authorizationServerProvider.GetPayloadString(author);
                        RpcContext.GetContext().SetAttachment("payload", instance.Payload.ToString());
                        model.Remove(keyValue.Key);
                        model.Add(keyValue.Key, instance);
                    }
                }
            }
            else
            {
                result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
                isSuccess = false;
            }
            return isSuccess;
        }

        private bool ValidateAppSecretAuthentication(ServiceRoute route, string path,
            Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true;
            DateTime time;
            var author = HttpContext.Request.Headers["Authorization"];
            
                if (!string.IsNullOrEmpty(path) && model.ContainsKey("timeStamp") && author.Count>0)
                {
                    if (DateTime.TryParse(model["timeStamp"].ToString(), out time))
                    {
                        var seconds = (DateTime.Now - time).TotalSeconds;
                        if (seconds <= 3560 && seconds >= 0)
                        {
                            if (GetMD5($"{route.ServiceDescriptor.Token}{time.ToString("yyyy-MM-dd hh:mm:ss") }") != author.ToString())
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
