using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    public abstract class CacheInterceptor : IInterceptor
    {
        public abstract Task Intercept(ICacheInvocation invocation);

        public async Task Intercept(IInvocation invocation)
        {
           await Intercept(invocation as ICacheInvocation);
        }
    }
}
