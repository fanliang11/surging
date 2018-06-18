using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;
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
                nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, LogLevel minLevel, string nlogConfigFile = "nLog.config")
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole(minLevel);
                nlogConfigFile =EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, Func<string, LogLevel, bool> filter, string nlogConfigFile = "nLog.config")
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole(filter);
                nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }
    }
}
