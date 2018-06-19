using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;
        private readonly List<Action<ContainerBuilder>> _registerServicesDelegates;
        private readonly List<Action<IConfigurationBuilder>> _configureDelegates;
        private readonly List<Action<IContainer>> _mapServicesDelegates;
        private  Action<ILoggingBuilder> _loggingDelegate;

        public ServiceHostBuilder()
        {
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
            _registerServicesDelegates = new List<Action<ContainerBuilder>>();
            _configureDelegates = new List<Action<IConfigurationBuilder>>();
            _mapServicesDelegates = new List<Action<IContainer>>();

        }

        public IServiceHost Build()
        {
           
            var services = BuildCommonServices();
            var config = Configure();
            if(_loggingDelegate!=null)
            services.AddLogging(_loggingDelegate);
            else
                services.AddLogging();
            services.AddSingleton(typeof(IConfigurationBuilder), config);
            var hostingServices = RegisterServices();
            var applicationServices = services.Clone();
            var hostingServiceProvider = services.BuildServiceProvider();
            hostingServices.Populate(services);
            var hostLifetime = hostingServiceProvider.GetService<IHostLifetime>();
            var host = new ServiceHost(hostingServices,hostingServiceProvider, hostLifetime,_mapServicesDelegates);
            var container= host.Initialize();
            return host;
        }

        public IServiceHostBuilder MapServices(Action<IContainer> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            _mapServicesDelegates.Add(mapper);
            return this;
        }

        public IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _registerServicesDelegates.Add(builder);
            return this;
        }
        
        public IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }
            _configureServicesDelegates.Add(configureServices);
            return this;
        }

        public IServiceHostBuilder Configure(Action<IConfigurationBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _configureDelegates.Add(builder);
            return this; 
        }

        private IServiceCollection BuildCommonServices()
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(services);
            }
            return services;
        }

        private IConfigurationBuilder Configure()
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory); 
            foreach (var configure in _configureDelegates)
            {
                configure(config);
            }
            return config;
        }
        
        private ContainerBuilder RegisterServices()
        {
            var hostingServices = new ContainerBuilder();
            foreach (var registerServices in _registerServicesDelegates)
            {
                registerServices(hostingServices);
            }
            return hostingServices;
        }

        public IServiceHostBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            _loggingDelegate=configure;
            return this;
        }
    }
}
