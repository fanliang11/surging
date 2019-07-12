using Surging.Core.CPlatform.Messages;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// Defines the <see cref="LogProviderInterceptor" />
    /// </summary>
    public class LogProviderInterceptor : IInterceptor
    {
        #region 方法

        /// <summary>
        /// The Intercept
        /// </summary>
        /// <param name="invocation">The invocation<see cref="IInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Intercept(IInvocation invocation)
        {
            var watch = Stopwatch.StartNew();
            await invocation.Proceed();
            var result = invocation.ReturnValue;
        }

        #endregion 方法
    }
}