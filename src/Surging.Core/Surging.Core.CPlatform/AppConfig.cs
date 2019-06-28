using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.CPlatform
{
   public class AppConfig
    {
        #region 字段
        private static AddressSelectorMode _loadBalanceMode=AddressSelectorMode.Polling;
        private static SurgingServerOptions _serverOptions=new SurgingServerOptions();
        #endregion

        public static IConfigurationRoot Configuration { get; internal set; }

        /// <summary>
        /// 负载均衡模式
        /// </summary>
        public static AddressSelectorMode LoadBalanceMode
        {
            get
            {
                AddressSelectorMode mode = _loadBalanceMode; ;
                if(Configuration !=null 
                    && Configuration["AccessTokenExpireTimeSpan"]!=null
                    && !Enum.TryParse(Configuration["AccessTokenExpireTimeSpan"], out mode))
                {
                    mode = _loadBalanceMode;
                }
                return mode;
            }
            internal set
            {
                _loadBalanceMode = value;
            }
        }

        public static IConfigurationSection GetSection(string name)
        {
            return Configuration?.GetSection(name);
        }

        public static SurgingServerOptions ServerOptions
        {
            get
            {
                return _serverOptions;
            }
            internal set
            {
                _serverOptions = value;
            }
        }
    }
}
