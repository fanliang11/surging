using System;
using System.Runtime.InteropServices;

namespace Surging.Core.System.Module.Attributes
{
    /// <summary>
    /// Defines the <see cref="AssemblyModuleTypeAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
    public sealed class AssemblyModuleTypeAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyModuleTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">The type<see cref="ModuleType"/></param>
        public AssemblyModuleTypeAttribute(ModuleType type)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyModuleTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">模块类型。</param>
        /// <param name="serialNumber">序号 </param>
        public AssemblyModuleTypeAttribute(ModuleType type, int serialNumber)
        {
            Type = type;
            SerialNumber = serialNumber;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the SerialNumber
        /// </summary>
        public int SerialNumber { get; private set; }

        /// <summary>
        /// Gets the Type
        /// 获取模块类型
        /// </summary>
        public ModuleType Type { get; private set; }

        #endregion 属性
    }
}