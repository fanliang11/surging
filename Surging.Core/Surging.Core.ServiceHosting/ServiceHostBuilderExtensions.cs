using Surging.Core.ServiceHosting.Internal;
using Surging.Core.ServiceHosting.Startup;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Autofac;
using Surging.Core.ServiceHosting.Startup.Implementation;
using Autofac.Extensions.DependencyInjection;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Surging.Core.ServiceHosting
{
   public static   class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseStartup(this IServiceHostBuilder hostBuilder, Type startupType)
        {
            var startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;

            return hostBuilder
                .ConfigureServices(services =>
                {
                    if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
                    {
                        services.AddSingleton(typeof(IStartup), startupType);
                    }
                    else
                    {
                        services.AddSingleton(typeof(IStartup), sp =>
                        { 
                            return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, ""));
                        });
                       
                    }
                });
        }

        public static IServiceHostBuilder UseStartup<TStartup>(this IServiceHostBuilder hostBuilder) where TStartup : class
        {
            return hostBuilder.UseStartup(typeof(TStartup));
        }
    }
}
