using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module.Attributes
{
    /// <summary>
    /// ModuleDescriptionAttribute 自定义特性类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ModuleDescriptionAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="identifier">标识符 GUID 。</param>
        /// <param name="title">标题。</param>
        /// <param name="description">描述。</param>
        public ModuleDescriptionAttribute(string identifier, string title, string description)
        {
            Identifier = identifier;
            Title = title;
            Description = description;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Description
        /// 获取描述信息。
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the Identifier
        /// 获取标识符 GUID 。
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the Title
        /// 获取标题信息。
        /// </summary>
        public string Title { get; private set; }

        #endregion 属性
    }
}