using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    /// <summary>
    /// CacheKeyAttribute 自定义特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false)]
    public abstract class KeyAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
        /// </summary>
        /// <param name="sortIndex">The sortIndex<see cref="int"/></param>
        protected KeyAttribute(int sortIndex)
        {
            SortIndex = sortIndex;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the SortIndex
        /// </summary>
        public int SortIndex { get; set; }

        #endregion 属性
    }
}