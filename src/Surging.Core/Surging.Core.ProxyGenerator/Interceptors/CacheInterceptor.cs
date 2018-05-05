using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    public abstract class CacheInterceptor : IInterceptor
    {
        public abstract Task Intercept(IInvocation invocation);
    }
}
