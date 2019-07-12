using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.OAuth
{
    /// <summary>
    /// Defines the <see cref="ConfigInfo" />
    /// </summary>
    public class ConfigInfo
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInfo"/> class.
        /// </summary>
        /// <param name="authorizationRoutePath">The authorizationRoutePath<see cref="string"/></param>
        public ConfigInfo(string authorizationRoutePath) : this(authorizationRoutePath, null, TimeSpan.FromMinutes(30))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInfo"/> class.
        /// </summary>
        /// <param name="authorizationRoutePath">The authorizationRoutePath<see cref="string"/></param>
        /// <param name="authorizationServiceKey">The authorizationServiceKey<see cref="string"/></param>
        /// <param name="accessTokenExpireTimeSpan">The accessTokenExpireTimeSpan<see cref="TimeSpan"/></param>
        public ConfigInfo(string authorizationRoutePath, string authorizationServiceKey, TimeSpan accessTokenExpireTimeSpan)
        {
            AuthorizationServiceKey = authorizationServiceKey;
            AuthorizationRoutePath = authorizationRoutePath;
            AccessTokenExpireTimeSpan = accessTokenExpireTimeSpan;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the AccessTokenExpireTimeSpan
        /// token 有效期
        /// </summary>
        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Gets or sets the AuthorizationRoutePath
        /// 授权服务路由地址
        /// </summary>
        public string AuthorizationRoutePath { get; set; }

        /// <summary>
        /// Gets or sets the AuthorizationServiceKey
        /// </summary>
        public string AuthorizationServiceKey { get; set; }

        #endregion 属性
    }

;
}