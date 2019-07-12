using System;
using System.Runtime.InteropServices;

namespace Surging.Core.System.Module.Attributes
{
    /// <summary>
    /// AssemblyDisableStopAndUninstalled 自定义特性类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
    public sealed class AssemblyDisableStopAndUninstalledAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDisableStopAndUninstalledAttribute"/> class.
        /// </summary>
        /// <param name="disableStopAndUninstalled">禁止停止和卸载。</param>
        public AssemblyDisableStopAndUninstalledAttribute(bool disableStopAndUninstalled)
        {
            DisableStopAndUninstalled = disableStopAndUninstalled;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether DisableStopAndUninstalled
        /// 获取一个值指示是否禁止停止和卸载。
        /// </summary>
        public bool DisableStopAndUninstalled { get; private set; }

        #endregion 属性
    }
}