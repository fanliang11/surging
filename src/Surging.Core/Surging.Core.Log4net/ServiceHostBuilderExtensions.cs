using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ServiceHosting.Internal;
using System;

namespace Surging.Core.Log4net
{
   public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder,string log4NetConfigFile= "log4net.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.AddConfiguration(CPlatform.AppConfig.GetSection("Logging"));
            });
            return hostBuilder.MapServices(mapper =>
            {
             
                var section = CPlatform.AppConfig.GetSection("Logging");
                if(section.Exists())
                    mapper.Resolve<ILoggerFactory>().AddConsole(section);
                else
                    mapper.Resolve<ILoggerFactory>().AddConsole((c, l) => (int)l >= 3);
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
            });
        }

        public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder, LogLevel minLevel, string log4NetConfigFile = "log4net.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.SetMinimumLevel(minLevel);
            });
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole(minLevel);
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
            });
        }

        public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder, Func<string, LogLevel, bool> filter, string log4NetConfigFile = "log4net.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.AddFilter(filter);
            });
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ILoggerFactory>().AddConsole(filter);
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
            });
        }
    }
}
