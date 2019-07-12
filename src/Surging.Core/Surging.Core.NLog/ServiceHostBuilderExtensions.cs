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
    /// <summary>
    /// Defines the <see cref="ServiceHostBuilderExtensions" />
    /// </summary>
    public static class ServiceHostBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UseNLog
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <param name="filter">The filter<see cref="Func{string, LogLevel, bool}"/></param>
        /// <param name="nlogConfigFile">The nlogConfigFile<see cref="string"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
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

        /// <summary>
        /// The UseNLog
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <param name="minLevel">The minLevel<see cref="LogLevel"/></param>
        /// <param name="nlogConfigFile">The nlogConfigFile<see cref="string"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder UseNLog(this IServiceHostBuilder hostBuilder, LogLevel minLevel, string nlogConfigFile = "nLog.config")
        {
            hostBuilder.ConfigureLogging(logger =>
            {
                logger.SetMinimumLevel(minLevel);
            });
            return hostBuilder.MapServices(mapper =>
            {
                nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                mapper.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
            });
        }

        /// <summary>
        /// The UseNLog
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <param name="nlogConfigFile">The nlogConfigFile<see cref="string"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
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

        #endregion 方法
    }
}