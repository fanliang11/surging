using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    /// <summary>
    /// Defines the <see cref="CacheInterceptor" />
    /// </summary>
    public abstract class CacheInterceptor : IInterceptor
    {
        #region 方法

        /// <summary>
        /// The Intercept
        /// </summary>
        /// <param name="invocation">The invocation<see cref="ICacheInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Intercept(ICacheInvocation invocation);

        /// <summary>
        /// The Intercept
        /// </summary>
        /// <param name="invocation">The invocation<see cref="IInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Intercept(IInvocation invocation)
        {
            await Intercept(invocation as ICacheInvocation);
        }

        #endregion 方法
    }
}