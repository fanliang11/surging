using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Ioc
{
    /// <summary>
    /// Defines the <see cref="ModuleNameAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleNameAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleNameAttribute"/> class.
        /// </summary>
        public ModuleNameAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleNameAttribute"/> class.
        /// </summary>
        /// <param name="moduleName">The moduleName<see cref="string"/></param>
        public ModuleNameAttribute(string moduleName)
        {
            ModuleName = moduleName;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the ModuleName
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the Version
        /// </summary>
        public string Version { get; set; }

        #endregion 属性
    }
}