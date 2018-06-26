using Autofac;
using Surging.Core.ServiceHosting.Internal;

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
