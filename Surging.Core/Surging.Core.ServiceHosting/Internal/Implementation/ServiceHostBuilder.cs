using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Surging.Core.ServiceHosting.Startup;
using System.Reflection;
using Surging.Core.ServiceHosting.Startup.Implementation;
using System.Runtime.ExceptionServices;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Autofac.Extensions.DependencyInjection;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;
        private ContainerBuilder hostingServices;
        public ServiceHostBuilder()
        {
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
        }
        public IServiceHost Build()
        {
            var services = BuildCommonServices();
            var applicationServices = services.Clone();
            var hostingServiceProvider = services.BuildServiceProvider();
            if (hostingServices == null) hostingServices = new ContainerBuilder();
            hostingServices.Populate(services);
            var host = new ServiceHost(hostingServices,hostingServiceProvider);
            host.Initialize();
            return host;
        }

        public IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder)
        {
            hostingServices = new ContainerBuilder();
            builder.Invoke(hostingServices);
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

        private IServiceCollection BuildCommonServices()
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(services);
            }
            return services;
        }
    }
}
