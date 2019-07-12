using Autofac;
using System;
using System.Diagnostics;

namespace Surging.Core.ServiceHosting.Startup.Implementation
{
    /// <summary>
    /// Defines the <see cref="StartupMethods" />
    /// </summary>
    public class StartupMethods
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupMethods"/> class.
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="configure">The configure<see cref="Action{IContainer}"/></param>
        /// <param name="configureServices">The configureServices<see cref="Func{ContainerBuilder, IContainer}"/></param>
        public StartupMethods(object instance, Action<IContainer> configure, Func<ContainerBuilder, IContainer> configureServices)
        {
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            StartupInstance = instance;
            ConfigureDelegate = configure;
            ConfigureServicesDelegate = configureServices;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ConfigureDelegate
        /// </summary>
        public Action<IContainer> ConfigureDelegate { get; }

        /// <summary>
        /// Gets the ConfigureServicesDelegate
        /// </summary>
        public Func<ContainerBuilder, IContainer> ConfigureServicesDelegate { get; }

        /// <summary>
        /// Gets the StartupInstance
        /// </summary>
        public object StartupInstance { get; }

        #endregion 属性
    }
}