using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="ApplicationInitializationContext" />
    /// </summary>
    public class ApplicationInitializationContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInitializationContext"/> class.
        /// </summary>
        /// <param name="builder">The builder<see cref="IApplicationBuilder"/></param>
        /// <param name="modules">The modules<see cref="List{AbstractModule}"/></param>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <param name="configuration">The configuration<see cref="IConfigurationRoot"/></param>
        public ApplicationInitializationContext(IApplicationBuilder builder,
    List<AbstractModule> modules,
    string[] virtualPaths,
   IConfigurationRoot configuration)
        {
            Builder = Check.NotNull(builder, nameof(builder));
            Modules = Check.NotNull(modules, nameof(modules));
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            Configuration = Check.NotNull(configuration, nameof(configuration));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Builder
        /// </summary>
        public IApplicationBuilder Builder { get; }

        /// <summary>
        /// Gets the Configuration
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Gets the Modules
        /// </summary>
        public List<AbstractModule> Modules { get; }

        /// <summary>
        /// Gets the VirtualPaths
        /// </summary>
        public string[] VirtualPaths { get; }

        #endregion 属性
    }
}