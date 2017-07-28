using Surging.Core.ProxyGenerator.Interceptors;
using System.Threading.Tasks;
using System.Linq;

namespace Surging.Core.System.Intercept
{
    public class CacheProviderInterceptor : IInterceptor
    {
        public async Task Intercept(IInvocation invocation)
        {
           var attribute =
                invocation.Attributes.Where(p => p.GetType() == typeof(InterceptMethodAttribute))
                .Select(p=>p as InterceptMethodAttribute).FirstOrDefault();
            var cacheKey=string.Format(attribute.Key, invocation.CacheKey);
             await invocation.Proceed();
        }
    }
}
