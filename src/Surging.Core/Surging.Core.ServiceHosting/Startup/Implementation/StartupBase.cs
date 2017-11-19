using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Startup.Implementation
{
    public abstract class StartupBase : IStartup
    {
        public abstract void Configure(IContainer app);

        IContainer IStartup.ConfigureServices(ContainerBuilder services)
        {
            ConfigureServices(services);
            return CreateServiceProvider(services);
        }

        public virtual void ConfigureServices(ContainerBuilder services)
        {
        }

        public virtual IContainer CreateServiceProvider(ContainerBuilder services)
        {
            return services.Build();
        }
    }

    public abstract class StartupBase<TBuilder> : StartupBase
    {
        public override IContainer CreateServiceProvider(ContainerBuilder services)
        {
            return services.Build();
        }

        public virtual void ConfigureContainer(TBuilder builder)
        {
        }
    }
}
