using Autofac;
using Surging.Core.ApiGateWay.Aggregation;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
   public static  class ContainerBuilderExtensions
    {
        public static IServiceBuilder AddApiGateWay(this IServiceBuilder builder, ConfigInfo config=null)
        {
            var services = builder.Services;
            services.RegisterType<FaultTolerantProvider>().As<IFaultTolerantProvider>().SingleInstance();
            services.RegisterType<DefaultHealthCheckService>().As<IHealthCheckService>().SingleInstance();
            services.RegisterType<ServiceDiscoveryProvider>().As<IServiceDiscoveryProvider>().SingleInstance();
            services.RegisterType<ServiceRegisterProvider>().As<IServiceRegisterProvider>().SingleInstance();
            services.RegisterType<ServiceSubscribeProvider>().As<IServiceSubscribeProvider>().SingleInstance();
            services.RegisterType<ServiceCacheProvider>().As<IServiceCacheProvider>().SingleInstance();
            services.RegisterType<ServicePartProvider>().As<IServicePartProvider>().SingleInstance();
            if (config != null)
            {
                AppConfig.AccessTokenExpireTimeSpan = config.AccessTokenExpireTimeSpan;
                AppConfig.AuthorizationRoutePath = config.AuthorizationRoutePath;
                AppConfig.AuthorizationServiceKey = config.AuthorizationServiceKey;
            }
            builder.Services.Register(provider =>
            {
                var serviceProxyProvider = provider.Resolve<IServiceProxyProvider>();
                var serviceRouteProvider = provider.Resolve<IServiceRouteProvider>();
                var serviceProvider = provider.Resolve<CPlatformContainer>();
                return new AuthorizationServerProvider(config, serviceProxyProvider, serviceRouteProvider, serviceProvider);
            }).As<IAuthorizationServerProvider>().SingleInstance();
            return builder;
        }
    }
}
