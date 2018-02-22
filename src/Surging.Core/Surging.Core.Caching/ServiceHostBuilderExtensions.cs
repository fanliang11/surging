using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServiceCache(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.RegisterServices(mapper =>
            {
                
            });
        }
    }
}
