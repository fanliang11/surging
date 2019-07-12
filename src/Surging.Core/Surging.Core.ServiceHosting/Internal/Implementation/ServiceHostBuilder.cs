using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceHostBuilder" />
    /// </summary>
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        #region 字段

        /// <summary>
        /// Defines the _configureDelegates
        /// </summary>
        private readonly List<Action<IConfigurationBuilder>> _configureDelegates;

        /// <summary>
        /// Defines the _configureServicesDelegates
        /// </summary>
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;

        /// <summary>
        /// Defines the _mapServicesDelegates
        /// </summary>
        private readonly List<Action<IContainer>> _mapServicesDelegates;

        /// <summary>
        /// Defines the _registerServicesDelegates
        /// </summary>
        private readonly List<Action<ContainerBuilder>> _registerServicesDelegates;

        /// <summary>
        /// Defines the _loggingDelegate
        /// </summary>
        private Action<ILoggingBuilder> _loggingDelegate;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHostBuilder"/> class.
        /// </summary>
        public ServiceHostBuilder()
        {
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
            _registerServicesDelegates = new List<Action<ContainerBuilder>>();
            _configureDelegates = new List<Action<IConfigurationBuilder>>();
            _mapServicesDelegates = new List<Action<IContainer>>();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <returns>The <see cref="IServiceHost"/></returns>
        public IServiceHost Build()
        {
            var services = BuildCommonServices();
            var config = Configure();
            if (_loggingDelegate != null)
                services.AddLogging(_loggingDelegate);
            else
                services.AddLogging();
            services.AddSingleton(typeof(IConfigurationBuilder), config);
            var hostingServices = RegisterServices();
            var applicationServices = services.Clone();
            var hostingServiceProvider = services.BuildServiceProvider();
            hostingServices.Populate(services);
            var hostLifetime = hostingServiceProvider.GetService<IHostLifetime>();
            var host = new ServiceHost(hostingServices, hostingServiceProvider, hostLifetime, _mapServicesDelegates);
            var container = host.Initialize();
            return host;
        }

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="builder">The builder<see cref="Action{IConfigurationBuilder}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public IServiceHostBuilder Configure(Action<IConfigurationBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _configureDelegates.Add(builder);
            return this;
        }

        /// <summary>
        /// The ConfigureLogging
        /// </summary>
        /// <param name="configure">The configure<see cref="Action{ILoggingBuilder}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public IServiceHostBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            _loggingDelegate = configure;
            return this;
        }

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="configureServices">The configureServices<see cref="Action{IServiceCollection}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }
            _configureServicesDelegates.Add(configureServices);
            return this;
        }

        /// <summary>
        /// The MapServices
        /// </summary>
        /// <param name="mapper">The mapper<see cref="Action{IContainer}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public IServiceHostBuilder MapServices(Action<IContainer> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            _mapServicesDelegates.Add(mapper);
            return this;
        }

        /// <summary>
        /// The RegisterServices
        /// </summary>
        /// <param name="builder">The builder<see cref="Action{ContainerBuilder}"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _registerServicesDelegates.Add(builder);
            return this;
        }

        /// <summary>
        /// The BuildCommonServices
        /// </summary>
        /// <returns>The <see cref="IServiceCollection"/></returns>
        private IServiceCollection BuildCommonServices()
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(services);
            }
            return services;
        }

        /// <summary>
        /// The Configure
        /// </summary>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        private IConfigurationBuilder Configure()
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);
            foreach (var configure in _configureDelegates)
            {
                configure(config);
            }
            return config;
        }

        /// <summary>
        /// The RegisterServices
        /// </summary>
        /// <returns>The <see cref="ContainerBuilder"/></returns>
        private ContainerBuilder RegisterServices()
        {
            var hostingServices = new ContainerBuilder();
            foreach (var registerServices in _registerServicesDelegates)
            {
                registerServices(hostingServices);
            }
            return hostingServices;
        }

        #endregion 方法
    }
}