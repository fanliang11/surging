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
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public class AppConfig
    {
        #region 字段

        /// <summary>
        /// Defines the _loadBalanceMode
        /// </summary>
        private static AddressSelectorMode _loadBalanceMode = AddressSelectorMode.Polling;

        /// <summary>
        /// Defines the _serverOptions
        /// </summary>
        private static SurgingServerOptions _serverOptions = new SurgingServerOptions();

        #endregion 字段

        #region 属性

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        public static IConfigurationRoot Configuration { get; internal set; }

        /// <summary>
        /// Gets or sets the LoadBalanceMode
        /// 负载均衡模式
        /// </summary>
        public static AddressSelectorMode LoadBalanceMode
        {
            get
            {
                AddressSelectorMode mode = _loadBalanceMode; ;
                if (Configuration != null
                    && Configuration["AccessTokenExpireTimeSpan"] != null
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

        /// <summary>
        /// Gets or sets the ServerOptions
        /// </summary>
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

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetSection
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="IConfigurationSection"/></returns>
        public static IConfigurationSection GetSection(string name)
        {
            return Configuration?.GetSection(name);
        }

        #endregion 方法
    }
}