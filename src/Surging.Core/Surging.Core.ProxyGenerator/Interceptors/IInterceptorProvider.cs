using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IInterceptorProvider" />
    /// </summary>
    public interface IInterceptorProvider
    {
        #region 方法

        /// <summary>
        /// The GetCacheInvocation
        /// </summary>
        /// <param name="proxy">The proxy<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <returns>The <see cref="IInvocation"/></returns>
        IInvocation GetCacheInvocation(object proxy, IDictionary<string, object> parameters, string serviceId, Type returnType);

        /// <summary>
        /// The GetInvocation
        /// </summary>
        /// <param name="proxy">The proxy<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <returns>The <see cref="IInvocation"/></returns>
        IInvocation GetInvocation(object proxy, IDictionary<string, object> parameters, string serviceId, Type returnType);

        #endregion 方法
    }

    #endregion 接口
}