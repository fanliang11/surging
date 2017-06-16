using System;
using System.Runtime.InteropServices;

namespace Surging.Core.System.Module.Attributes
{
    /// <summary>
    /// AssemblyDisableStopAndUninstalled 自定义特性类。
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2015/12/8</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
    public sealed class AssemblyDisableStopAndUninstalledAttribute : Attribute
    {
        #region 属性

        /// <summary>
        /// 获取一个值指示是否禁止停止和卸载。
        /// </summary>
        /// <value>
        /// 	如果 <c>true</c> 禁止停止和卸载; 否则, <c>false</c> 允许停止和卸载。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/8</para>
        /// </remarks>
        public bool DisableStopAndUninstalled
        {
            get;
            private set;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化一个新的 <see cref="AssemblyDisableStopAndUninstalledAttribute"/> 类实例。
        /// </summary>
        /// <param name="disableStopAndUninstalled">禁止停止和卸载。</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/8</para>
        /// </remarks>
        public AssemblyDisableStopAndUninstalledAttribute(bool disableStopAndUninstalled)
        {
            DisableStopAndUninstalled = disableStopAndUninstalled;
        }

        #endregion
    }
}
