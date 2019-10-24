using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.ServiceHosting.Extensions.Runtime;
using Surging.Core.ServiceHosting.Extensions.Runtime.Implementation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Extensions
{
    public class ServiceHostModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);
            serviceProvider.GetInstances<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
            {
                var provider = serviceProvider.GetInstances<IBackgroundServiceEntryProvider>();
                var entries =  provider.GetEntries();
                foreach(var entry in entries)
                {
                     var cts = new CancellationTokenSource();
                    Task.Run(() =>
                    {
                        try
                        {
                            entry.Behavior.StartAsync(cts.Token);
                        }
                        catch
                        {
                            entry.Behavior.StopAsync(cts.Token);
                        }
                    });
                }
            });
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.Register(provider =>
            {
                return new DefaultBackgroundServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultBackgroundServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>()
                      );
            }).As(typeof(IBackgroundServiceEntryProvider)).SingleInstance();
        }
    }
}
