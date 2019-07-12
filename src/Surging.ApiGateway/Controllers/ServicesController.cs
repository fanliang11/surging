using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GateWayAppConfig = Surging.Core.ApiGateWay.AppConfig;

namespace Surging.ApiGateway.Controllers
{
    /// <summary>
    /// Defines the <see cref="ServicesController" />
    /// </summary>
    public class ServicesController : Controller
    {
        #region 字段

        /// <summary>
        /// Defines the _authorizationServerProvider
        /// </summary>
        private readonly IAuthorizationServerProvider _authorizationServerProvider;

        /// <summary>
        /// Defines the _serviceProxyProvider
        /// </summary>
        private readonly IServiceProxyProvider _serviceProxyProvider;

        /// <summary>
        /// Defines the _serviceRouteProvider
        /// </summary>
        private readonly IServiceRouteProvider _serviceRouteProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicesController"/> class.
        /// </summary>
        /// <param name="serviceProxyProvider">The serviceProxyProvider<see cref="IServiceProxyProvider"/></param>
        /// <param name="serviceRouteProvider">The serviceRouteProvider<see cref="IServiceRouteProvider"/></param>
        /// <param name="authorizationServerProvider">The authorizationServerProvider<see cref="IAuthorizationServerProvider"/></param>
        public ServicesController(IServiceProxyProvider serviceProxyProvider,
            IServiceRouteProvider serviceRouteProvider,
            IAuthorizationServerProvider authorizationServerProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
            _serviceRouteProvider = serviceRouteProvider;
            _authorizationServerProvider = authorizationServerProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetMD5
        /// </summary>
        /// <param name="encypStr">The encypStr<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The Path
        /// </summary>
        /// <param name="servicePartProvider">The servicePartProvider<see cref="IServicePartProvider"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="model">The model<see cref="Dictionary{string, object}"/></param>
        /// <returns>The <see cref="Task{ServiceResult{object}}"/></returns>
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
            ServiceResult<object> result = ServiceResult<object>.Create(false, null);
            var route = await _serviceRouteProvider.GetRouteByPathRegex(path);
            path = String.Compare(route.ServiceDescriptor.RoutePath, GateWayAppConfig.TokenEndpointPath, true) == 0 ?
                GateWayAppConfig.AuthorizationRoutePath : path.ToLower();
            if (!GetAllowRequest(route)) return new ServiceResult<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
            if (servicePartProvider.IsPart(path))
            {
                result = ServiceResult<object>.Create(true, await servicePartProvider.Merge(path, model));
                result.StatusCode = (int)ServiceStatusCode.Success;
            }
            else
            {
                var auth = OnAuthorization(route, model);
                result = auth.Item2;
                if (auth.Item1)
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
                        if (String.Compare(route.ServiceDescriptor.RoutePath, path, true) != 0)
                        {
                            var pamars = RouteTemplateSegmenter.Segment(route.ServiceDescriptor.RoutePath, path);
                            foreach (KeyValuePair<string, object> item in pamars)
                            {
                                model.Add(item.Key, item.Value);
                            }
                        }
                        if (!string.IsNullOrEmpty(serviceKey))
                        {
                            result = ServiceResult<object>.Create(true, await _serviceProxyProvider.Invoke<object>(model, route.ServiceDescriptor.RoutePath, serviceKey));
                            result.StatusCode = (int)ServiceStatusCode.Success;
                        }
                        else
                        {
                            result = ServiceResult<object>.Create(true, await _serviceProxyProvider.Invoke<object>(model, route.ServiceDescriptor.RoutePath));
                            result.StatusCode = (int)ServiceStatusCode.Success;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The ValidateJwtAuthentication
        /// </summary>
        /// <param name="route">The route<see cref="ServiceRoute"/></param>
        /// <param name="model">The model<see cref="Dictionary{string, object}"/></param>
        /// <param name="result">The result<see cref="ServiceResult{object}"/></param>
        /// <returns>The <see cref="bool"/></returns>
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
                    var payload = _authorizationServerProvider.GetPayloadString(author);
                    RpcContext.GetContext().SetAttachment("payload", payload);
                    if (model.Count > 0)
                    {
                        var keyValue = model.FirstOrDefault();
                        if (!(keyValue.Value is IConvertible) || !typeof(IConvertible).GetTypeInfo().IsAssignableFrom(keyValue.Value.GetType()))
                        {
                            dynamic instance = keyValue.Value;
                            instance.Payload = payload;
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

        /// <summary>
        /// The GetAllowRequest
        /// </summary>
        /// <param name="route">The route<see cref="ServiceRoute"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool GetAllowRequest(ServiceRoute route)
        {
            return !route.ServiceDescriptor.DisableNetwork();
        }

        /// <summary>
        /// The OnAuthorization
        /// </summary>
        /// <param name="route">The route<see cref="ServiceRoute"/></param>
        /// <param name="model">The model<see cref="Dictionary{string, object}"/></param>
        /// <returns>The <see cref="(bool, ServiceResult{object})"/></returns>
        private (bool, ServiceResult<object>) OnAuthorization(ServiceRoute route, Dictionary<string, object> model)
        {
            bool isSuccess = true;
            var serviceResult = ServiceResult<object>.Create(false, null);
            if (route.ServiceDescriptor.EnableAuthorization())
            {
                if (route.ServiceDescriptor.AuthType() == AuthorizationType.JWT.ToString())
                {
                    isSuccess = ValidateJwtAuthentication(route, model, ref serviceResult);
                }
                else
                {
                    isSuccess = ValidateAppSecretAuthentication(route, model, ref serviceResult);
                }
            }
            return new ValueTuple<bool, ServiceResult<object>>(isSuccess, serviceResult);
        }

        /// <summary>
        /// The ValidateAppSecretAuthentication
        /// </summary>
        /// <param name="route">The route<see cref="ServiceRoute"/></param>
        /// <param name="model">The model<see cref="Dictionary{string, object}"/></param>
        /// <param name="result">The result<see cref="ServiceResult{object}"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool ValidateAppSecretAuthentication(ServiceRoute route,
            Dictionary<string, object> model, ref ServiceResult<object> result)
        {
            bool isSuccess = true;
            DateTime time;
            var author = HttpContext.Request.Headers["Authorization"];

            if (model.ContainsKey("timeStamp") && author.Count > 0)
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

        #endregion 方法
    }
}