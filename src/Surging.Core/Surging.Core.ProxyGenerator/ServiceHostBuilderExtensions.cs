using Surging.Core.ServiceHosting.Internal;
using Autofac;
using Surging.Core.ProxyGenerator.Implementation;

namespace Surging.Core.ProxyGenerator
{
   public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseProxy(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceProxyFactory>();
            });
        }
    }
}
