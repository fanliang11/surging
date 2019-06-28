using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.DNS.Configurations;
using Surging.Core.DNS.Runtime;
using Surging.Core.DNS.Runtime.Implementation;

namespace Surging.Core.DNS
{
   public class DnsProtocolModule : EnginePartModule
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
            var section = CPlatform.AppConfig.GetSection("Dns");
            if (section.Exists())
                AppConfig.DnsOption = section.Get<DnsOption>();
            builder.Register(provider =>
            {
                return new DefaultDnsServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultDnsServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>()
                      );
            }).As(typeof(IDnsServiceEntryProvider)).SingleInstance();
            builder.RegisterType(typeof(DnsServiceExecutor)).As(typeof(IServiceExecutor))
            .Named<IServiceExecutor>(CommunicationProtocol.Dns.ToString()).SingleInstance();
            if (CPlatform.AppConfig.ServerOptions.Protocol == CommunicationProtocol.Dns)
            {
                RegisterDefaultProtocol(builder);
            }
            else if (CPlatform.AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterDnsProtocol(builder);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DotNettyDnsServerMessageListener(provider.Resolve<ILogger<DotNettyDnsServerMessageListener>>(),
                      provider.Resolve<ITransportMessageCodecFactory>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var serviceExecutor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Dns.ToString());
                var messageListener = provider.Resolve<DotNettyDnsServerMessageListener>();
                return new DnsServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);

            }).As<IServiceHost>();
        }

        private static void RegisterDnsProtocol(ContainerBuilderWrapper builder)
        {

            builder.Register(provider =>
            {
                return new DotNettyDnsServerMessageListener(provider.Resolve<ILogger<DotNettyDnsServerMessageListener>>(),
                      provider.Resolve<ITransportMessageCodecFactory>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var serviceExecutor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Dns.ToString());
                var messageListener = provider.Resolve<DotNettyDnsServerMessageListener>();
                return new DnsServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);

            }).As<IServiceHost>();
        }
    }
}
