using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Nlog
{
   public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, string nlogConfigFile = "nLog.config")
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole((c, l) => (int)l >= 3);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, LogLevel minLevel, string nlogConfigFile = "nLog.config")
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole(minLevel);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, Func<string, LogLevel, bool> filter, string nlogConfigFile = "nLog.config")
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole(filter);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }
    }
}
