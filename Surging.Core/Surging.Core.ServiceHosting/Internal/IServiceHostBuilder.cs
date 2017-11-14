using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal
{
    public  interface IServiceHostBuilder
    {
        IServiceHost Build();

        IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder);

        IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        IServiceHostBuilder MapServices(Action<IContainer> mapper);
    }
}
