
using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator.Implementation;
using Autofac;
using System;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Convertibles;

namespace Surging.Core.ProxyGenerator
{
    public static class ContainerBuilderExtensions
    {
        public static IServiceBuilder AddClientProxy(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType<ServiceProxyGenerater>().As<IServiceProxyGenerater>().SingleInstance();
            services.RegisterType<ServiceProxyProvider>().As<IServiceProxyProvider>().SingleInstance();
            builder.Services.Register(provider =>new ServiceProxyFactory(
                 provider.Resolve<IRemoteInvokeService>(),
                 provider.Resolve<ITypeConvertibleService>(),
                 provider.Resolve<IServiceProvider>(),
                 builder.GetInterfaceService()
                 )).As<IServiceProxyFactory>().SingleInstance();
            return builder;
        }

        public static IServiceBuilder AddClientIntercepted(this IServiceBuilder builder, Type interceptorServiceTypes )
        {
            var services = builder.Services;
            services.RegisterType(interceptorServiceTypes).As<IInterceptor>().SingleInstance();
            services.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
            return builder;
        }
         
        public static IServiceBuilder AddClient(this ContainerBuilder services)
        {
            return services
                .AddCoreService()
                .AddClientRuntime()
                .AddClientProxy();
        }

       public  static IServiceBuilder AddRelateService(this IServiceBuilder builder)
        {
            return builder.AddRelateServiceRuntime().AddClientProxy();
        }

        public static IServiceBuilder AddClient(this IServiceBuilder builder)
        {
            return builder
                .RegisterServices()
                .RegisterRepositories()
                .RegisterServiceBus()
                .AddClientRuntime()
                .AddClientProxy();
        }
    }
}