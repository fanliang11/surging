using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.Protocol.Coap.Configurations;
using Surging.Core.Protocol.Coap.Runtime;
using Surging.Core.Protocol.Coap.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Coap
{
    internal class CoapProtocolModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var options = new CoapOptions();
            var section = AppConfig.GetSection("Coap");
            if (section.Exists())
                options = section.Get<CoapOptions>();
            base.RegisterBuilder(builder);
            builder.RegisterType(typeof(CoapNetworkProvider)).Named(NetworkType.Coap.ToString(), typeof(INetworkProvider<NetworkProperties>)).SingleInstance();
            builder.Register(provider =>
            {
                return new DefaultCoapServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultCoapServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>()
                      );
            }).As(typeof(ICoapServiceEntryProvider)).SingleInstance();
            RegisterDefaultProtocol(builder, options);
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder, CoapOptions options)
        {

            builder.Register(provider =>
            {
                return new DefaultCoapServerMessageListener(
                    provider.Resolve<ILogger<DefaultCoapServerMessageListener>>(),
                      provider.Resolve<ICoapServiceEntryProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DefaultCoapServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    var ipEndPoint = endPoint as IPEndPoint;
                    await messageListener.StartAsync(new IPEndPoint(ipEndPoint.Address, options.Port));
                    return messageListener;
                }, null);

            }).As<IServiceHost>();
        }

    }
}
