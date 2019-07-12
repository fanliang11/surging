using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IInterceptor" />
    /// </summary>
    public interface IInterceptor
    {
        #region 方法

        /// <summary>
        /// The Intercept
        /// </summary>
        /// <param name="invocation">The invocation<see cref="IInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Intercept(IInvocation invocation);

        #endregion 方法
    }

    #endregion 接口
}