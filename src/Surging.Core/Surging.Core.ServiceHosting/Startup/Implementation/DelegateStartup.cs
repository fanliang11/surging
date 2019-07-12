using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Startup.Implementation
{
    /// <summary>
    /// Defines the <see cref="DelegateStartup" />
    /// </summary>
    public class DelegateStartup : StartupBase<ContainerBuilder>
    {
        #region 字段

        /// <summary>
        /// Defines the _configureApp
        /// </summary>
        private Action<IContainer> _configureApp;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateStartup"/> class.
        /// </summary>
        /// <param name="configureApp">The configureApp<see cref="Action{IContainer}"/></param>
        public DelegateStartup(Action<IContainer> configureApp) : base()
        {
            _configureApp = configureApp;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="app">The app<see cref="IContainer"/></param>
        public override void Configure(IContainer app) => _configureApp(app);

        #endregion 方法
    }
}