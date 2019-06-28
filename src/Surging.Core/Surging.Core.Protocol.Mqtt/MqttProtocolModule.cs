using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.Protocol.Mqtt.Implementation;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.Core.Protocol.Mqtt.Internal.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt
{
    public class MqttProtocolModule : EnginePartModule
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
            builder.RegisterType(typeof(DefaultMqttServiceFactory)).As(typeof(IMqttServiceFactory)).SingleInstance();
            builder.RegisterType(typeof(DefaultMqttBrokerEntryManager)).As(typeof(IMqttBrokerEntryManger)).SingleInstance();
            builder.RegisterType(typeof(MqttRemoteInvokeService)).As(typeof(IMqttRemoteInvokeService)).SingleInstance();
            builder.Register(provider =>
            {
                return new WillService(
                            provider.Resolve<ILogger<WillService>>(),
                             provider.Resolve<CPlatformContainer>()
                    );
            }).As<IWillService>().SingleInstance();
            builder.Register(provider =>
            {
                return new MessagePushService(new SacnScheduled());
            }).As<IMessagePushService>().SingleInstance();
            builder.RegisterType(typeof(ClientSessionService)).As(typeof(IClientSessionService)).SingleInstance();
            builder.Register(provider =>
            {
                return new MqttChannelService(
                        provider.Resolve<IMessagePushService>(),
                        provider.Resolve<IClientSessionService>(),
                        provider.Resolve<ILogger<MqttChannelService>>(),
                        provider.Resolve<IWillService>(),
                        provider.Resolve<IMqttBrokerEntryManger>(),
                         provider.Resolve<IMqttRemoteInvokeService>(),
                         provider.Resolve<IServiceIdGenerator>()
                    );
            }).As(typeof(IChannelService)).SingleInstance();
            builder.RegisterType(typeof(DefaultMqttBehaviorProvider)).As(typeof(IMqttBehaviorProvider)).SingleInstance();

            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.Mqtt)
            {
                RegisterDefaultProtocol(builder);
            }
            else if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterMqttProtocol(builder);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DotNettyMqttServerMessageListener(provider.Resolve<ILogger<DotNettyMqttServerMessageListener>>(),
                      provider.Resolve<IChannelService>(),
                      provider.Resolve<IMqttBehaviorProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                
                var messageListener = provider.Resolve<DotNettyMqttServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, null);

            }).As<IServiceHost>();
        }

        private static void RegisterMqttProtocol(ContainerBuilderWrapper builder)
        {

            builder.Register(provider =>
            {
                return new DotNettyMqttServerMessageListener(provider.Resolve<ILogger<DotNettyMqttServerMessageListener>>(),
                     provider.Resolve<IChannelService>(),
                     provider.Resolve<IMqttBehaviorProvider>()
                     );
            }).SingleInstance();
            builder.Register(provider =>
            { 
                var messageListener = provider.Resolve<DotNettyMqttServerMessageListener>();
                return new MqttServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }
    }
}
