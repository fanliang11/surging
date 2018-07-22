using Autofac;
using Microsoft.Extensions.Configuration;
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
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.AddConfiguration(CPlatform.AppConfig.GetSection("Logging"));
            });
            return hostBuilder.MapServices(mapper =>
            {
                var section = CPlatform.AppConfig.GetSection("Logging");
                nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, LogLevel minLevel, string nlogConfigFile = "nLog.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.SetMinimumLevel(minLevel);
            });
            return hostBuilder.MapServices(mapper =>
            {
                nlogConfigFile =EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, Func<string, LogLevel, bool> filter, string nlogConfigFile = "nLog.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.AddFilter(filter);
            });
            return hostBuilder.MapServices(mapper =>
            {
                nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }
    }
}
