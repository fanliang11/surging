using Autofac;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.ProxyGenerator.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.ProxyGenerator.Diagnostics;
using Surging.Core.CPlatform.Diagnostics;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Protocol;

namespace Surging.Core.ProxyGenerator
{
   public class ServiceProxyModule: EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            serviceProvider.GetInstances<IServiceProxyFactory>();
            if (AppConfig.ServerOptions.ReloadOnChange)
            {
                new ServiceRouteWatch(serviceProvider,
                        async () =>
                        {
                            var builder = new ContainerBuilder();
                            var result = serviceProvider.GetInstances<IServiceEngineBuilder>().ReBuild(builder);
                            if (result != null)
                            {
                                builder.Update(serviceProvider.Current.ComponentRegistry);
                                if(result.Value.Item1!=null)
                                serviceProvider.GetInstances<IServiceEntryProvider>().RegisterType(result.Value.Item1);
                                serviceProvider.GetInstances<IServiceEntryManager>().UpdateEntries(serviceProvider.GetInstances<IEnumerable<IServiceEntryProvider>>());
                                serviceProvider.GetInstances<IServiceRouteProvider>().ResetLocalRoute();
                                     //  serviceProvider.GetInstances<IServiceProxyFactory>().RegisterProxType(result.Value.Item2.ToArray(), result.Value.Item1.ToArray());
                                     // serviceProvider.GetInstances<IServiceRouteProvider>().RegisterRoutes(0);
                                     //  serviceProvider.GetInstances<IServiceProxyFactory>();
                                     await Task.Factory.StartNew(() =>
                                {
                                    serviceProvider.GetInstances<IProtocolSupportProvider>().Initialize();
                                }, TaskCreationOptions.LongRunning);
                            }
                        });
            }
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterType<RpcTransportDiagnosticProcessor>().As<ITracingDiagnosticProcessor>().SingleInstance();
        }
    }
}
