
using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator.Implementation;
using Autofac;

namespace Surging.Core.ProxyGenerator
{
    public static class ContainerBuilderExtensions
    {
        public static IServiceBuilder AddClientProxy(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType<ServiceProxyGenerater>().As<IServiceProxyGenerater>().SingleInstance();
            services.RegisterType<ServiceProxyFactory>().As<IServiceProxyFactory>().SingleInstance();
            return builder;
        }

        public static IServiceBuilder AddClient(this ContainerBuilder services)
        {
            return services
                .AddCoreServce()
                .AddClientRuntime()
                .AddClientProxy();
        }
    }
}