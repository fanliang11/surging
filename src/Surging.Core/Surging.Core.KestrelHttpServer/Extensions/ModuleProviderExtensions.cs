using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Extensions
{
    /// <summary>
    /// Defines the <see cref="ModuleProviderExtensions" />
    /// </summary>
    public static class ModuleProviderExtensions
    {
        #region 方法

        /// <summary>
        /// The ConfigureHost
        /// </summary>
        /// <param name="moduleProvider">The moduleProvider<see cref="IModuleProvider"/></param>
        /// <param name="context">The context<see cref="WebHostContext"/></param>
        public static void ConfigureHost(this IModuleProvider moduleProvider, WebHostContext context)
        {
            moduleProvider.Modules.ForEach(p =>
            {
                try
                {
                    if (p.Enable)
                    {
                        var module = p as KestrelHttpModule;
                        module?.RegisterBuilder(context);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="moduleProvider">The moduleProvider<see cref="IModuleProvider"/></param>
        /// <param name="context">The context<see cref="ConfigurationContext"/></param>
        public static void ConfigureServices(this IModuleProvider moduleProvider, ConfigurationContext context)
        {
            moduleProvider.Modules.ForEach(p =>
            {
                try
                {
                    if (p.Enable)
                    {
                        var module = p as KestrelHttpModule;
                        module?.RegisterBuilder(context);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="moduleProvider">The moduleProvider<see cref="IModuleProvider"/></param>
        /// <param name="builder">The builder<see cref="ApplicationInitializationContext"/></param>
        public static void Initialize(this IModuleProvider moduleProvider, ApplicationInitializationContext builder)
        {
            moduleProvider.Modules.ForEach(p =>
            {
                try
                {
                    using (var abstractModule = p)
                        if (abstractModule.Enable)
                        {
                            var module = abstractModule as KestrelHttpModule;
                            module?.Initialize(builder);
                        }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        #endregion 方法
    }
}