using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Intercept
{
    public class LogProviderInterceptor : IInterceptor
    {
        public async Task Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = invocation.Proceed();
            await Task.CompletedTask;
        }
    }
}
