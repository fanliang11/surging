using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching
{
    /// <summary>
    /// Defines the <see cref="IdentifyCacheAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IdentifyCacheAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifyCacheAttribute"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="CacheTargetType"/></param>
        public IdentifyCacheAttribute(CacheTargetType name)
        {
            this.Name = name;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public CacheTargetType Name { get; set; }

        #endregion 属性
    }
}