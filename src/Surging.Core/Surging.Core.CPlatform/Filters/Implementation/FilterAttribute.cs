using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    /// <summary>
    /// Defines the <see cref="FilterAttribute" />
    /// </summary>
    public abstract class FilterAttribute : Attribute, IFilter
    {
        #region 字段

        /// <summary>
        /// Defines the _filterAttribute
        /// </summary>
        private readonly bool _filterAttribute;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterAttribute"/> class.
        /// </summary>
        protected FilterAttribute()
        {
            _filterAttribute = true;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether AllowMultiple
        /// </summary>
        public virtual bool AllowMultiple { get => _filterAttribute; }

        #endregion 属性
    }
}