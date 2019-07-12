using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="ConfigurationContext" />
    /// </summary>
    public class ConfigurationContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        /// <param name="modules">The modules<see cref="List{AbstractModule}"/></param>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <param name="configuration">The configuration<see cref="IConfigurationRoot"/></param>
        public ConfigurationContext(IServiceCollection services,
            List<AbstractModule> modules,
            string[] virtualPaths,
           IConfigurationRoot configuration)
        {
            Services = Check.NotNull(services, nameof(services));
            Modules = Check.NotNull(modules, nameof(modules));
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            Configuration = Check.NotNull(configuration, nameof(configuration));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Configuration
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Gets the Modules
        /// </summary>
        public List<AbstractModule> Modules { get; }

        /// <summary>
        /// Gets the Services
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the VirtualPaths
        /// </summary>
        public string[] VirtualPaths { get; }

        #endregion 属性
    }
}