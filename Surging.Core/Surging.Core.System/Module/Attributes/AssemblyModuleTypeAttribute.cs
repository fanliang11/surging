
using System;
using System.Runtime.InteropServices;

namespace Surging.Core.System.Module.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
    public sealed class  AssemblyModuleTypeAttribute:Attribute
    {
        #region 属性

        /// <summary>
        /// 获取模块类型
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/8</para>
        /// </remarks>
        public ModuleType Type
        {
            get;
            private set;
        }

        public int SerialNumber { get; private set; }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化一个新的 <see cref="AssemblyModuleTypeAttribute"/> 类实例。
        /// </summary>
        /// <param name="type">模块类型。</param>
        /// <param name="serialNumber">序号 </param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/8</para>
        /// </remarks>
        public AssemblyModuleTypeAttribute(ModuleType type, int serialNumber)
        {
            Type = type;
            SerialNumber = serialNumber;
        }


        public AssemblyModuleTypeAttribute(ModuleType type)
        {
            Type = type;
        }
        #endregion
    }
}
