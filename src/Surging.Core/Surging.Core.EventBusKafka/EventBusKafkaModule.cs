using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Module;
using Surging.Core.EventBusKafka.Configurations;
using Surging.Core.EventBusKafka.Implementation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.EventBusKafka
{
    /// <summary>
    /// Defines the <see cref="EventBusKafkaModule" />
    /// </summary>
    public class EventBusKafkaModule : EnginePartModule
    {
        #region 方法

        /// <summary>
        /// The AddKafkaMQAdapt
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <returns>The <see cref="EventBusKafkaModule"/></returns>
        public EventBusKafkaModule AddKafkaMQAdapt(ContainerBuilderWrapper builder)
        {
            UseKafkaMQEventAdapt(builder, provider =>
            new KafkaSubscriptionAdapt(
                provider.GetService<IConsumeConfigurator>(),
                provider.GetService<IEnumerable<IIntegrationEventHandler>>()
                )
          );
            return this;
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);
            serviceProvider.GetInstances<ISubscriptionAdapt>().SubscribeAt();
            serviceProvider.GetInstances<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
             {
                 KafkaConsumerPersistentConnection connection = serviceProvider.GetInstances<IKafkaPersisterConnection>(KafkaConnectionType.Consumer.ToString()) as KafkaConsumerPersistentConnection;
                 connection.Listening(TimeSpan.FromMilliseconds(AppConfig.Options.Timeout));
             });
        }

        /// <summary>
        /// The UseKafkaMQEventAdapt
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="adapt">The adapt<see cref="Func{IServiceProvider, ISubscriptionAdapt}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseKafkaMQEventAdapt(ContainerBuilderWrapper builder, Func<IServiceProvider, ISubscriptionAdapt> adapt)
        {
            builder.RegisterAdapter(adapt);
            return builder;
        }

        /// <summary>
        /// The UseKafkaMQTransport
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <returns>The <see cref="EventBusKafkaModule"/></returns>
        public EventBusKafkaModule UseKafkaMQTransport(ContainerBuilderWrapper builder)
        {
            AppConfig.Options = new KafkaOptions();
            var section = CPlatform.AppConfig.GetSection("EventBus_Kafka");
            if (section.Exists())
                AppConfig.Options = section.Get<KafkaOptions>();
            else if (AppConfig.Configuration != null)
                AppConfig.Options = AppConfig.Configuration.Get<KafkaOptions>();
            AppConfig.KafkaConsumerConfig = AppConfig.Options.GetConsumerConfig();
            AppConfig.KafkaProducerConfig = AppConfig.Options.GetProducerConfig();
            builder.RegisterType(typeof(Implementation.EventBusKafka)).As(typeof(IEventBus)).SingleInstance();
            builder.RegisterType(typeof(DefaultConsumeConfigurator)).As(typeof(IConsumeConfigurator)).SingleInstance();
            builder.RegisterType(typeof(InMemoryEventBusSubscriptionsManager)).As(typeof(IEventBusSubscriptionsManager)).SingleInstance();
            builder.RegisterType(typeof(KafkaProducerPersistentConnection))
           .Named(KafkaConnectionType.Producer.ToString(), typeof(IKafkaPersisterConnection)).SingleInstance();
            builder.RegisterType(typeof(KafkaConsumerPersistentConnection))
            .Named(KafkaConnectionType.Consumer.ToString(), typeof(IKafkaPersisterConnection)).SingleInstance();
            return this;
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            UseKafkaMQTransport(builder).AddKafkaMQAdapt(builder);
        }

        #endregion 方法
    }
}