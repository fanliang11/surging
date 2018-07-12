using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.Protocol.WS.Runtime;
using Surging.Core.Protocol.WS.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS
{
   public class WSProtocolModule : EnginePartModule
    {
        public override void Initialize(CPlatformContainer serviceProvider)
        {
            base.Initialize(serviceProvider);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterType(typeof(DefaultWSServiceEntryProvider)).As(typeof(IWSServiceEntryProvider)).SingleInstance();
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.WS)
            {
                RegisterDefaultProtocol(builder);
            }
            else if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterWSProtocol(builder);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DefaultWSServerMessageListener(
                    provider.Resolve<ILogger<DefaultWSServerMessageListener>>(),
                      provider.Resolve<IWSServiceEntryProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DefaultWSServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, null);

            }).As<IServiceHost>();
        }

        private static void RegisterWSProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DefaultWSServerMessageListener(provider.Resolve<ILogger<DefaultWSServerMessageListener>>(),
                      provider.Resolve<IWSServiceEntryProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DefaultWSServerMessageListener>();
                return new WSServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }
    }
}
