using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.Grpc.Runtime;
using Surging.Core.Grpc.Runtime.Implementation;

namespace Surging.Core.Grpc
{
    public  class GrpcModule : EnginePartModule
    { 
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }
 
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DefaultGrpcServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultGrpcServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>()
                      );
            }).As(typeof(IGrpcServiceEntryProvider)).SingleInstance();
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.WS)
            {
                RegisterDefaultProtocol(builder);
            }
            else if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterGrpcProtocol(builder);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {

            builder.Register(provider =>
            {
                return new GrpcServerMessageListener(
                    provider.Resolve<ILogger<GrpcServerMessageListener>>(),
                      provider.Resolve<IGrpcServiceEntryProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<GrpcServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, null);

            }).As<IServiceHost>();
        }

        private static void RegisterGrpcProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new GrpcServerMessageListener(provider.Resolve<ILogger<GrpcServerMessageListener>>(),
                      provider.Resolve<IGrpcServiceEntryProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<GrpcServerMessageListener>();
                return new GrpcServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }
    }
}
 