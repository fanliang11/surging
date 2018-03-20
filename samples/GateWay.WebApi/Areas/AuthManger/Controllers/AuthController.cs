using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.Interface.Auth;
using Application.Interface.Org;
using Application.Service.Auth.Dto;
using DTO.Core;
using GateWay.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.Caching.DependencyResolution;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Implementation;
using GateWayAppConfig = Surging.Core.ApiGateWay.AppConfig;

namespace GateWay.WebApi.Areas.OrgManger.Controllers
{

    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IAuthorizationServerProvider _authorizationServerProvider;

        private IAuthAppService _authProxy;
        public AuthController()
        {
            _authProxy = serviceProxyFactory.CreateProxy<IAuthAppService>();
            _serviceProxyProvider = ServiceLocator.GetService<IServiceProxyProvider>();
            _serviceRouteProvider = ServiceLocator.GetService<IServiceRouteProvider>(); //serviceRouteProvider;
            _authorizationServerProvider = ServiceLocator.GetService<IAuthorizationServerProvider>();// authorizationServerProvider;

        }
        #region 角色
        [HttpGet]
        private async Task<object> Get()
        {
            Dictionary<string, object> model = new Dictionary<string, object>();
            model.Add("req",JsonConvert.SerializeObject( new {
                UserName= "SuperMan",
                Pwd= "123456",
                CorporationKeyId= Guid.Parse("8C100FEE-2D1D-4B70-A27D-13D65DCD5E38")
            }));
            string path = "api/authapp/signin";
            string serviceKey = "Auth";
            var serviceProxyProvider = ServiceLocator.GetService<IServiceProxyProvider>();
            var res = await serviceProxyProvider.Invoke<object>(model, path, serviceKey);
            return res;
        }

        [HttpGet]
        public async Task<ServiceResult<object>> DomainPermissions(CommonCMDReq req)
        {
            var result = await _authProxy.FindDomainPermissions(req);
            return ServiceResult<object>.Create(true, result);
        }

        
        [HttpPost("SignIn")]
        public async Task<ServiceResult<object>> SignIn(LoginReq req)
        {
            //要注意参数类型
            var model = new Dictionary<string, object>();
            model.Add("req", JsonConvert.SerializeObject(req));
            var serviceKey = "Auth";
            ServiceResult<object> result = ServiceResult<object>.Create(false, null);
            var path = GateWayAppConfig.AuthorizationRoutePath;
            if (OnAuthorization(path, model, ref result))
            {
                if (path == GateWayAppConfig.AuthorizationRoutePath)
                {
                    var token = await _authorizationServerProvider.GenerateTokenCredential(model);
                    if (token != null)
                    {
                        //查询当前用户的权限，返回给客户端
                        var tmp  = JsonConvert.DeserializeObject<string>(_authorizationServerProvider.GetPayloadString(token));
                        var identify = JsonConvert.DeserializeObject<TokenDto>(tmp);
                        Dictionary<string, object> reqQueryUserPermission = new Dictionary<string, object>();
                        reqQueryUserPermission.Add("req", JsonConvert.SerializeObject(new
                        {
                            Identify = identify
                        })); 
                        string servicePath = "api/orgapp/QueryUserPermission";
                        var res = await _serviceProxyProvider.Invoke<BaseListResponseDto>(reqQueryUserPermission, servicePath);
                        if (res!=null&&res.OperateFlag)
                        {
                            result = ServiceResult<object>.Create(true,new { token = token, auth=res.Result });
                            result.StatusCode = (int)ServiceStatusCode.Success;
                        }
                        else
                        {
                            result = new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };
                        }
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
        #endregion

        #region Private
        private bool OnAuthorization(string path, Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true;
            var route = _serviceRouteProvider.GetRouteByPath(path).Result;
            if (route.ServiceDescriptor.EnableAuthorization())
            {
                if (route.ServiceDescriptor.AuthType() == AuthorizationType.JWT.ToString())
                {
                    isSuccess = ValidateJwtAuthentication(route, model, ref result);
                }
                else
                {
                    isSuccess = ValidateAppSecretAuthentication(route, path, model, ref result);
                }

            }
            return isSuccess;
        }

        private bool ValidateJwtAuthentication(ServiceRoute route, Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true;
            var author = HttpContext.Request.Headers["Authorization"];
            if (author.Count > 0)
            {
                if (route.Address.Any(p => p.DisableAuth == false))
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
                            model.Remove(keyValue.Key);
                            model.Add(keyValue.Key, instance);
                        }
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
            if (route.Address.Any(p => p.DisableAuth == false))
            {
                if (!string.IsNullOrEmpty(path) && model.ContainsKey("timeStamp") && author.Count > 0)
                {
                    if (DateTime.TryParse(model["timeStamp"].ToString(), out time))
                    {
                        var seconds = (DateTime.Now - time).TotalSeconds;
                        if (seconds <= 3560 && seconds >= 0)
                        {
                            if (!route.Address.Any(p => GetMD5($"{p.Token}{time.ToString("yyyy-MM-dd hh:mm:ss") }") == author.ToString()))
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
            return isSuccess;
        }

        private string GetMD5(string encypStr)
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
        #endregion
    }
}