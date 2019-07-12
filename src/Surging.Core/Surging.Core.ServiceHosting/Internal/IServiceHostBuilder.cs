using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceHostBuilder" />
    /// </summary>
    public interface IServiceHostBuilder
    {
        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <returns>The <see cref="IServiceHost"/></returns>
        IServiceHost Build();

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="builder">The builder<see cref="Action{IConfigurationBuilder}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        IServiceHostBuilder Configure(Action<IConfigurationBuilder> builder);

        /// <summary>
        /// The ConfigureLogging
        /// </summary>
        /// <param name="configure">The configure<see cref="Action{ILoggingBuilder}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        IServiceHostBuilder ConfigureLogging(Action<ILoggingBuilder> configure);

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="configureServices">The configureServices<see cref="Action{IServiceCollection}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        /// <summary>
        /// The MapServices
        /// </summary>
        /// <param name="mapper">The mapper<see cref="Action{IContainer}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        IServiceHostBuilder MapServices(Action<IContainer> mapper);

        /// <summary>
        /// The RegisterServices
        /// </summary>
        /// <param name="builder">The builder<see cref="Action{ContainerBuilder}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder);

        #endregion 方法
    }

    #endregion 接口
}