using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
    public static class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }


        private static string _authorizationServiceKey;
        public static string AuthorizationServiceKey
        {
            get
            {
                return Configuration["AuthorizationServiceKey"] ?? _authorizationServiceKey;
            }
            internal set
            {

                _authorizationServiceKey = value;
            }
        }

        private static string _authorizationRoutePath;
        public static string AuthorizationRoutePath
        {
            get
            {
                return Configuration["AuthorizationRoutePath"] ?? _authorizationRoutePath;
            }
            internal set
            {

                _authorizationRoutePath = value;
            }
        }

        private static TimeSpan _accessTokenExpireTimeSpan = TimeSpan.FromMinutes(30);
        public static TimeSpan AccessTokenExpireTimeSpan
        {
            get
            {
                int tokenExpireTime;
                if (Configuration["AccessTokenExpireTimeSpan"] != null && int.TryParse(Configuration["AccessTokenExpireTimeSpan"], out tokenExpireTime))
                {
                    _accessTokenExpireTimeSpan = TimeSpan.FromMinutes(tokenExpireTime);
                }
                return _accessTokenExpireTimeSpan;
            }
            internal set
            {
                _accessTokenExpireTimeSpan = value;
            }
        }

        private static string _tokenEndpointPath = "oauth2/token";

        public static string  TokenEndpointPath
        {
            get
            {
                return Configuration["TokenEndpointPath"] ?? _tokenEndpointPath;
            }
            internal set
            {
                _tokenEndpointPath = value;
            }
        }

        private static string _cacheMode = "MemoryCache";

        public static string CacheMode
        {
            get
            {
                return Configuration["CacheMode"] ?? _cacheMode;
            }
           
        }
    }
}
