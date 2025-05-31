using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Interceptors
{
    public interface IHttpInterceptor
    {
        Task<bool> Intercept(IHttpInvocation invocation);
    }
}
