using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ICacheInvocation" />
    /// </summary>
    public interface ICacheInvocation : IInvocation
    {
        #region 属性

        /// <summary>
        /// Gets the Attributes
        /// </summary>
        List<Attribute> Attributes { get; }

        /// <summary>
        /// Gets the CacheKey
        /// </summary>
        string[] CacheKey { get; }

        #endregion 属性
    }

    #endregion 接口
}