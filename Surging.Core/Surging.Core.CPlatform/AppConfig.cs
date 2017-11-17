using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        internal static IConfigurationRoot Configuration { get; set; }
        
        private static AddressSelectorMode _loadBalanceMode=AddressSelectorMode.Polling;
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
    }
}
