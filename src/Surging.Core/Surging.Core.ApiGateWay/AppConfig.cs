using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Surging.Core.ApiGateWay.Configurations;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public static class AppConfig
    {
        #region 字段

        /// <summary>
        /// Defines the _accessTokenExpireTimeSpan
        /// </summary>
        private static TimeSpan _accessTokenExpireTimeSpan = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Defines the _authorizationRoutePath
        /// </summary>
        private static string _authorizationRoutePath;

        /// <summary>
        /// Defines the _authorizationServiceKey
        /// </summary>
        private static string _authorizationServiceKey;

        /// <summary>
        /// Defines the _cacheMode
        /// </summary>
        private static string _cacheMode = "MemoryCache";

        /// <summary>
        /// Defines the _tokenEndpointPath
        /// </summary>
        private static string _tokenEndpointPath = "oauth2/token";

        #endregion 字段

        #region 属性

        /// <summary>
        /// Gets or sets the AccessTokenExpireTimeSpan
        /// </summary>
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

        /// <summary>
        /// Gets or sets the AuthorizationRoutePath
        /// </summary>
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

        /// <summary>
        /// Gets or sets the AuthorizationServiceKey
        /// </summary>
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

        /// <summary>
        /// Gets the CacheMode
        /// </summary>
        public static string CacheMode
        {
            get
            {
                return Configuration["CacheMode"] ?? _cacheMode;
            }
        }

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets the Policy
        /// </summary>
        public static AccessPolicy Policy
        {
            get
            {
                var result = new AccessPolicy();
                var section = Configuration.GetSection("AccessPolicy");
                if (section != null)
                    result = section.Get<AccessPolicy>();
                return result;
            }
        }

        /// <summary>
        /// Gets the Register
        /// </summary>
        public static Register Register
        {
            get
            {
                var result = new Register();
                var section = Configuration.GetSection("Register");
                if (section != null)
                    result = section.Get<Register>();
                return result;
            }
        }

        /// <summary>
        /// Gets the ServicePart
        /// </summary>
        public static ServicePart ServicePart
        {
            get
            {
                var result = new ServicePart();
                var section = Configuration.GetSection("ServicePart");
                if (section != null)
                    result = section.Get<ServicePart>();
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the TokenEndpointPath
        /// </summary>
        public static string TokenEndpointPath
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

        #endregion 属性
    }
}