using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IModuleManager" />
    /// </summary>
    public interface IModuleManager
    {
        #region 方法

        /// <summary>
        /// The Delete
        /// </summary>
        /// <param name="module">The module<see cref="AssemblyEntry"/></param>
        void Delete(AssemblyEntry module);

        /// <summary>
        /// The Install
        /// </summary>
        /// <param name="modulePackageFileName">The modulePackageFileName<see cref="string"/></param>
        /// <param name="textWriter">The textWriter<see cref="TextWriter"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool Install(string modulePackageFileName, TextWriter textWriter);

        /// <summary>
        /// The Save
        /// </summary>
        /// <param name="module">The module<see cref="AssemblyEntry"/></param>
        void Save(AssemblyEntry module);

        /// <summary>
        /// The Start
        /// </summary>
        /// <param name="module">The module<see cref="AssemblyEntry"/></param>
        void Start(AssemblyEntry module);

        /// <summary>
        /// The Stop
        /// </summary>
        /// <param name="module">The module<see cref="AssemblyEntry"/></param>
        void Stop(AssemblyEntry module);

        /// <summary>
        /// The Uninstall
        /// </summary>
        /// <param name="module">The module<see cref="AssemblyEntry"/></param>
        void Uninstall(AssemblyEntry module);

        #endregion 方法
    }

    #endregion 接口
}