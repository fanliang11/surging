using Autofac;
using Surging.Core.CPlatform.Engines;
using Surging.Core.ServiceHosting.Internal;

namespace Surging.Core.ProxyGenerator
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseProxy(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
                 {
                     mapper.Resolve<IServiceProxyFactory>();
                 }); 
            });
        }
    }
}
