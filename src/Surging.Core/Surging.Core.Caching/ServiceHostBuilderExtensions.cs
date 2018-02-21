using Microsoft.Extensions.Configuration;
using Surging.Core.Caching.Models;
using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using System.Reflection;
using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform.Cache;

namespace Surging.Core.Caching
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServiceCache(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(builder =>
            { 
            });
        }
    }
}
