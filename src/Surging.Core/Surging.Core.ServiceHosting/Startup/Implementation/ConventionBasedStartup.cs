using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Surging.Core.ServiceHosting.Startup.Implementation
{
    /// <summary>
    /// Defines the <see cref="ConventionBasedStartup" />
    /// </summary>
    public class ConventionBasedStartup : IStartup
    {
        #region 字段

        /// <summary>
        /// Defines the _methods
        /// </summary>
        private readonly StartupMethods _methods;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionBasedStartup"/> class.
        /// </summary>
        /// <param name="methods">The methods<see cref="StartupMethods"/></param>
        public ConventionBasedStartup(StartupMethods methods)
        {
            _methods = methods;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="app">The app<see cref="IContainer"/></param>
        public void Configure(IContainer app)
        {
            try
            {
                _methods.ConfigureDelegate(app);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
        public IContainer ConfigureServices(ContainerBuilder services)
        {
            try
            {
                return _methods.ConfigureServicesDelegate(services);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        #endregion 方法
    }
}