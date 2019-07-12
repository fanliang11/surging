using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Startup.Implementation
{
    /// <summary>
    /// Defines the <see cref="StartupBase" />
    /// </summary>
    public abstract class StartupBase : IStartup
    {
        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="app">The app<see cref="IContainer"/></param>
        public abstract void Configure(IContainer app);

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        public virtual void ConfigureServices(ContainerBuilder services)
        {
        }

        /// <summary>
        /// The CreateServiceProvider
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
        public virtual IContainer CreateServiceProvider(ContainerBuilder services)
        {
            return services.Build();
        }

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
        IContainer IStartup.ConfigureServices(ContainerBuilder services)
        {
            ConfigureServices(services);
            return CreateServiceProvider(services);
        }

        #endregion 方法
    }

    /// <summary>
    /// Defines the <see cref="StartupBase{TBuilder}" />
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    public abstract class StartupBase<TBuilder> : StartupBase
    {
        #region 方法

        /// <summary>
        /// The ConfigureContainer
        /// </summary>
        /// <param name="builder">The builder<see cref="TBuilder"/></param>
        public virtual void ConfigureContainer(TBuilder builder)
        {
        }

        /// <summary>
        /// The CreateServiceProvider
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
        public override IContainer CreateServiceProvider(ContainerBuilder services)
        {
            return services.Build();
        }

        #endregion 方法
    }
}