using Autofac;
using Surging.Core.ApiGateWay.Aggregation;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
    public class ApiGeteWayModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            builder.RegisterType<FaultTolerantProvider>().As<IFaultTolerantProvider>().SingleInstance();
            builder.RegisterType<DefaultHealthCheckService>().As<IHealthCheckService>().SingleInstance();
            builder.RegisterType<ServiceDiscoveryProvider>().As<IServiceDiscoveryProvider>().SingleInstance();
            builder.RegisterType<ServiceRegisterProvider>().As<IServiceRegisterProvider>().SingleInstance();
            builder.RegisterType<ServiceSubscribeProvider>().As<IServiceSubscribeProvider>().SingleInstance();
            builder.RegisterType<ServiceCacheProvider>().As<IServiceCacheProvider>().SingleInstance();
            builder.RegisterType<ServicePartProvider>().As<IServicePartProvider>().SingleInstance();
   
            builder.Register(provider =>
            {
                var serviceProxyProvider = provider.Resolve<IServiceProxyProvider>();
                var serviceRouteProvider = provider.Resolve<IServiceRouteProvider>();
                var serviceProvider = provider.Resolve<CPlatformContainer>();
                return new AuthorizationServerProvider(serviceProxyProvider, serviceRouteProvider, serviceProvider);
            }).As<IAuthorizationServerProvider>().SingleInstance();
        }
    }

}
