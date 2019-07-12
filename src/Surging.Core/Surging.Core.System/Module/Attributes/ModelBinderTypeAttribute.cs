using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module.Attributes
{
    /// <summary>
    /// ModelBinderTypeAttribute 自定义特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ModelBinderTypeAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetTypes">目标类型列表</param>
        public ModelBinderTypeAttribute(params Type[] targetTypes)
        {
            if (targetTypes == null) throw new ArgumentNullException("targetTypes");
            TargetTypes = targetTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetType">目标类型</param>
        public ModelBinderTypeAttribute(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            TargetTypes = new Type[] { targetType };
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the TargetTypes
        /// 目标类型
        /// </summary>
        public IEnumerable<Type> TargetTypes { get; private set; }

        #endregion 属性
    }
}