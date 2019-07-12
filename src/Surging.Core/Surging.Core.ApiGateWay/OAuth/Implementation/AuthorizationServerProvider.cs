using Newtonsoft.Json;
using Surging.Core.Caching;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.OAuth
{
    /// <summary>
    /// 授权服务提供者
    /// </summary>
    public class AuthorizationServerProvider : IAuthorizationServerProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _cacheProvider
        /// </summary>
        private readonly ICacheProvider _cacheProvider;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

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
        /// Initializes a new instance of the <see cref="AuthorizationServerProvider"/> class.
        /// </summary>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <param name="serviceProxyProvider">The serviceProxyProvider<see cref="IServiceProxyProvider"/></param>
        /// <param name="serviceRouteProvider">The serviceRouteProvider<see cref="IServiceRouteProvider"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public AuthorizationServerProvider(ConfigInfo configInfo, IServiceProxyProvider serviceProxyProvider
           , IServiceRouteProvider serviceRouteProvider
            , CPlatformContainer serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceProxyProvider = serviceProxyProvider;
            _serviceRouteProvider = serviceRouteProvider;
            _cacheProvider = CacheContainer.GetService<ICacheProvider>(AppConfig.CacheMode);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GenerateTokenCredential
        /// </summary>
        /// <param name="parameters">The parameters<see cref="Dictionary{string, object}"/></param>
        /// <returns>The <see cref="Task{string}"/></returns>
        public async Task<string> GenerateTokenCredential(Dictionary<string, object> parameters)
        {
            string result = null;
            var payload = await _serviceProxyProvider.Invoke<object>(parameters, AppConfig.AuthorizationRoutePath, AppConfig.AuthorizationServiceKey);
            if (payload != null && !payload.Equals("null"))
            {
                var jwtHeader = JsonConvert.SerializeObject(new JWTSecureDataHeader() { TimeStamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") });
                var base64Payload = ConverBase64String(JsonConvert.SerializeObject(payload));
                var encodedString = $"{ConverBase64String(jwtHeader)}.{base64Payload}";
                var route = await _serviceRouteProvider.GetRouteByPath(AppConfig.AuthorizationRoutePath);
                var signature = HMACSHA256(encodedString, route.ServiceDescriptor.Token);
                result = $"{encodedString}.{signature}";
                _cacheProvider.Add(base64Payload, result, AppConfig.AccessTokenExpireTimeSpan);
            }
            return result;
        }

        /// <summary>
        /// The GetPayloadString
        /// </summary>
        /// <param name="token">The token<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public string GetPayloadString(string token)
        {
            string result = null;
            var jwtToken = token.Split('.');
            if (jwtToken.Length == 3)
            {
                result = Encoding.UTF8.GetString(Convert.FromBase64String(jwtToken[1]));
            }
            return result;
        }

        /// <summary>
        /// The ValidateClientAuthentication
        /// </summary>
        /// <param name="token">The token<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> ValidateClientAuthentication(string token)
        {
            bool isSuccess = false;
            var jwtToken = token.Split('.');
            if (jwtToken.Length == 3)
            {
                isSuccess = await _cacheProvider.GetAsync<string>(jwtToken[1]) == token;
            }
            return isSuccess;
        }

        /// <summary>
        /// The ConverBase64String
        /// </summary>
        /// <param name="str">The str<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string ConverBase64String(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// The HMACSHA256
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="secret">The secret<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string HMACSHA256(string message, string secret)
        {
            secret = secret ?? "";
            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        #endregion 方法
    }
}