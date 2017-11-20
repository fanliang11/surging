using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator
{
   public interface  IServiceProxyProvider
    {
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath);

        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath,string serviceKey);
    }
}
