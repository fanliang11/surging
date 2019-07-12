using Autofac;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.ProxyGenerator.Implementation;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System;

namespace Surging.Core.ProxyGenerator
{
    /// <summary>
    /// Defines the <see cref="ContainerBuilderExtensions" />
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The AddClient
        /// </summary>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IServiceBuilder"/></returns>
        public static IServiceBuilder AddClient(this ContainerBuilder services)
        {
            return services
                .AddCoreService()
                .AddClientRuntime()
                .AddClientProxy();
        }

        /// <summary>
        /// 添加客户端属性注入
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <returns>服务构建者</returns>
        public static IServiceBuilder AddClient(this IServiceBuilder builder)
        {
            return builder
                .RegisterServices()
                .RegisterRepositories()
                .RegisterServiceBus()
                .RegisterModules()
                .RegisterInstanceByConstraint()
                .AddClientRuntime()
                .AddClientProxy();
        }

        /// <summary>
        /// 添加客户端拦截
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="interceptorServiceType"></param>
        /// <returns>服务构建者</returns>
        public static IServiceBuilder AddClientIntercepted(this IServiceBuilder builder, Type interceptorServiceType)
        {
            var services = builder.Services;
            services.RegisterType(interceptorServiceType).As<IInterceptor>().SingleInstance();
            services.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
            return builder;
        }

        /// <summary>
        /// The AddClientIntercepted
        /// </summary>
        /// <param name="builder">The builder<see cref="IServiceBuilder"/></param>
        /// <param name="interceptorServiceTypes">The interceptorServiceTypes<see cref="Type[]"/></param>
        /// <returns>The <see cref="IServiceBuilder"/></returns>
        public static IServiceBuilder AddClientIntercepted(this IServiceBuilder builder, params Type[] interceptorServiceTypes)
        {
            var services = builder.Services;
            services.RegisterTypes(interceptorServiceTypes).As<IInterceptor>().SingleInstance();
            services.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
            return builder;
        }

        /// <summary>
        /// 添加客户端代理
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddClientProxy(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType<ServiceProxyGenerater>().As<IServiceProxyGenerater>().SingleInstance();
            services.RegisterType<ServiceProxyProvider>().As<IServiceProxyProvider>().SingleInstance();
            builder.Services.Register(provider => new ServiceProxyFactory(
                 provider.Resolve<IRemoteInvokeService>(),
                 provider.Resolve<ITypeConvertibleService>(),
                 provider.Resolve<IServiceProvider>(),
                 builder.GetInterfaceService(),
                 builder.GetDataContractName()
                 )).As<IServiceProxyFactory>().SingleInstance();
            return builder;
        }

        /// <summary>
        /// 添加关联服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>服务构建者</returns>
        public static IServiceBuilder AddRelateService(this IServiceBuilder builder)
        {
            return builder.AddRelateServiceRuntime().AddClientProxy();
        }

        #endregion 方法
    }
}