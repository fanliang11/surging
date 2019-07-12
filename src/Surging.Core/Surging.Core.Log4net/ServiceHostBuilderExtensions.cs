using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ServiceHosting.Internal;
using System;

namespace Surging.Core.Log4net
{
    /// <summary>
    /// Defines the <see cref="ServiceHostBuilderExtensions" />
    /// </summary>
    public static class ServiceHostBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UseLog4net
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <param name="filter">The filter<see cref="Func{string, LogLevel, bool}"/></param>
        /// <param name="log4NetConfigFile">The log4NetConfigFile<see cref="string"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder, Func<string, LogLevel, bool> filter, string log4NetConfigFile = "log4net.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.AddFilter(filter);
            });
            return hostBuilder.MapServices(mapper =>
            {
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
            });
        }

        /// <summary>
        /// The UseLog4net
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <param name="minLevel">The minLevel<see cref="LogLevel"/></param>
        /// <param name="log4NetConfigFile">The log4NetConfigFile<see cref="string"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder, LogLevel minLevel, string log4NetConfigFile = "log4net.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.SetMinimumLevel(minLevel);
            });
            return hostBuilder.MapServices(mapper =>
            {
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
            });
        }

        /// <summary>
        /// The UseLog4net
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <param name="log4NetConfigFile">The log4NetConfigFile<see cref="string"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder, string log4NetConfigFile = "log4net.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.AddConfiguration(CPlatform.AppConfig.GetSection("Logging"));
            });
            return hostBuilder.MapServices(mapper =>
            {
                var section = CPlatform.AppConfig.GetSection("Logging");
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
            });
        }

        #endregion 方法
    }
}