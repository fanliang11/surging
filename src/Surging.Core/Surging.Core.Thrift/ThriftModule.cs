using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.Thrift.Runtime;
using Surging.Core.Thrift.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Thrift
{
    class ThriftModule : EnginePartModule
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
            base.RegisterBuilder(builder);
            builder.Register(provider =>
            {
                IServiceExecutor serviceExecutor = null;
                if (provider.IsRegistered(typeof(IServiceExecutor)))
                    serviceExecutor = provider.Resolve<IServiceExecutor>();
                return new ThriftTransportClientFactory(provider.Resolve<ITransportMessageCodecFactory>(),
                      provider.Resolve<IHealthCheckService>(),
                    provider.Resolve<ILogger<ThriftTransportClientFactory>>(),
                    serviceExecutor);
            }).As(typeof(ITransportClientFactory)).SingleInstance();
            builder.Register(provider =>
            {
                return new DefaultThriftServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultThriftServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>()
                      );
            }).As(typeof(IThriftServiceEntryProvider)).SingleInstance();
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.Tcp ||
                AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterDefaultProtocol(builder);
            }
        }

        private void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new ThriftServerMessageListener(provider.Resolve<ILogger<ThriftServerMessageListener>>(),
                    provider.Resolve<IThriftServiceEntryProvider>(),
                      provider.Resolve<ITransportMessageCodecFactory>());
            }).SingleInstance();
            builder.Register(provider =>
            {
                var serviceExecutor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Tcp.ToString());
                var messageListener = provider.Resolve<ThriftServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);
            }).As<IServiceHost>();
        }
    }
}
