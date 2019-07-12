using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// CacheKeyAttribute 自定义特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false)]
    public class CacheKeyAttribute : KeyAttribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKeyAttribute"/> class.
        /// </summary>
        /// <param name="sortIndex">The sortIndex<see cref="int"/></param>
        public CacheKeyAttribute(int sortIndex) : base(sortIndex)
        {
        }

        #endregion 构造函数
    }
}