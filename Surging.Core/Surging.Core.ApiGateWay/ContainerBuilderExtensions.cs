using Autofac;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
   public static  class ContainerBuilderExtensions
    {
        public static IServiceBuilder AddApiGateWay(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType<FaultTolerantProvider>().As<IFaultTolerantProvider>().SingleInstance();
            services.RegisterType<DefaultHealthCheckService>().As <IHealthCheckService>().SingleInstance();
            services.RegisterType<ZookeeperServiceDiscoveryProvider>().As<IServiceDiscoveryProvider>().SingleInstance();
            return builder;
        }
    }
}
