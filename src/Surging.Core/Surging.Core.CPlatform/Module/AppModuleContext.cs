using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="AppModuleContext" />
    /// </summary>
    public class AppModuleContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AppModuleContext"/> class.
        /// </summary>
        /// <param name="modules">The modules<see cref="List{AbstractModule}"/></param>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <param name="serviceProvoider">The serviceProvoider<see cref="CPlatformContainer"/></param>
        public AppModuleContext(List<AbstractModule> modules,
            string[] virtualPaths,
            CPlatformContainer serviceProvoider)
        {
            Modules = Check.NotNull(modules, nameof(modules));
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            ServiceProvoider = Check.NotNull(serviceProvoider, nameof(serviceProvoider));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Modules
        /// </summary>
        public List<AbstractModule> Modules { get; }

        /// <summary>
        /// Gets the ServiceProvoider
        /// </summary>
        public CPlatformContainer ServiceProvoider { get; }

        /// <summary>
        /// Gets the VirtualPaths
        /// </summary>
        public string[] VirtualPaths { get; }

        #endregion 属性
    }
}