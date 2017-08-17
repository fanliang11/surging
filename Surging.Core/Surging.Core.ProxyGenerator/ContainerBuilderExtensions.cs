
using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator.Implementation;
using Autofac;
using System;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;

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

        public static IServiceBuilder AddClientIntercepted(this IServiceBuilder builder, Type interceptorServiceTypes )
        {
            var services = builder.Services;
            services.RegisterType(interceptorServiceTypes).As<IInterceptor>().SingleInstance();
            services.RegisterType<InterceptorProvider>().As <IInterceptorProvider>().SingleInstance();
            return builder;
        }

        public static IServiceBuilder AddClient(this ContainerBuilder services)
        {
            return services
                .AddCoreService()
                .AddClientRuntime()
                .AddClientProxy();
        }
    }
}