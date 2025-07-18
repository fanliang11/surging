﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.OAuth
{
    public class ConfigInfo
    {
        public ConfigInfo(string authorizationRoutePath):this(authorizationRoutePath,null, TimeSpan.FromMinutes(30))
        {

        }
        
        public ConfigInfo(string authorizationRoutePath,string authorizationServiceKey, TimeSpan accessTokenExpireTimeSpan)
        {
            AuthorizationServiceKey = authorizationServiceKey;
            AuthorizationRoutePath = authorizationRoutePath;
            AccessTokenExpireTimeSpan = accessTokenExpireTimeSpan;
        }
        public string AuthorizationServiceKey { get; set; }
        /// <summary>
        /// 授权服务路由地址
        /// </summary>
        public string AuthorizationRoutePath { get; set; }
        /// <summary>
        /// token 有效期
        /// </summary>
        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(30);
    };
}
