using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IModuleProvider" />
    /// </summary>
    public interface IModuleProvider
    {
        #region 属性

        /// <summary>
        /// Gets the Modules
        /// </summary>
        List<AbstractModule> Modules { get; }

        /// <summary>
        /// Gets the VirtualPaths
        /// </summary>
        string[] VirtualPaths { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        void Initialize();

        #endregion 方法
    }

    #endregion 接口
}