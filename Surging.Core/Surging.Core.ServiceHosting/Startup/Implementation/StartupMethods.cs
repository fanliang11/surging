using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Surging.Core.ServiceHosting.Startup.Implementation
{
   public class StartupMethods
    {
        public StartupMethods(object instance, Action<IContainer> configure, Func<ContainerBuilder, IContainer> configureServices)
        {
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            StartupInstance = instance;
            ConfigureDelegate = configure;
            ConfigureServicesDelegate = configureServices;
        }

        public object StartupInstance { get; }
        public Func<ContainerBuilder, IContainer> ConfigureServicesDelegate { get; }
        public Action<IContainer> ConfigureDelegate { get; }

    }
}