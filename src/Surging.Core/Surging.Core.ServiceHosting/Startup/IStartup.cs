using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Startup
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IStartup" />
    /// </summary>
    public interface IStartup
    {
        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="app">The app<see cref="IContainer"/></param>
        void Configure(IContainer app);

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
        IContainer ConfigureServices(ContainerBuilder services);

        #endregion 方法
    }

    #endregion 接口
}