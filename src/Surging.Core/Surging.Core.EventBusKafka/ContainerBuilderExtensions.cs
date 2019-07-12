using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.EventBusKafka.Configurations;
using Surging.Core.EventBusKafka.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka
{
    /// <summary>
    /// Defines the <see cref="ContainerBuilderExtensions" />
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The AddKafkaMQAdapt
        /// </summary>
        /// <param name="builder">The builder<see cref="IServiceBuilder"/></param>
        /// <returns>The <see cref="IServiceBuilder"/></returns>
        public static IServiceBuilder AddKafkaMQAdapt(this IServiceBuilder builder)
        {
            return builder.UseKafkaMQEventAdapt(provider =>
             new KafkaSubscriptionAdapt(
                 provider.GetService<IConsumeConfigurator>(),
                 provider.GetService<IEnumerable<IIntegrationEventHandler>>()
                 )
            );
        }

        /// <summary>
        /// The UseKafkaMQEventAdapt
        /// </summary>
        /// <param name="builder">The builder<see cref="IServiceBuilder"/></param>
        /// <param name="adapt">The adapt<see cref="Func{IServiceProvider, ISubscriptionAdapt}"/></param>
        /// <returns>The <see cref="IServiceBuilder"/></returns>
        public static IServiceBuilder UseKafkaMQEventAdapt(this IServiceBuilder builder, Func<IServiceProvider, ISubscriptionAdapt> adapt)
        {
            var services = builder.Services;
            services.RegisterAdapter(adapt);
            return builder;
        }

        /// <summary>
        /// 使用KafkaMQ进行传输。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="options">The options<see cref="Action{KafkaOptions}"/></param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseKafkaMQTransport(this IServiceBuilder builder, Action<KafkaOptions> options)
        {
            AppConfig.Options = new KafkaOptions();
            var section = CPlatform.AppConfig.GetSection("Kafka");
            if (section.Exists())
                AppConfig.Options = section.Get<KafkaOptions>();
            else if (AppConfig.Configuration != null)
                AppConfig.Options = AppConfig.Configuration.Get<KafkaOptions>();
            options.Invoke(AppConfig.Options);
            AppConfig.KafkaConsumerConfig = AppConfig.Options.GetConsumerConfig();
            AppConfig.KafkaProducerConfig = AppConfig.Options.GetProducerConfig();
            var services = builder.Services;
            builder.Services.RegisterType(typeof(Implementation.EventBusKafka)).As(typeof(IEventBus)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultConsumeConfigurator)).As(typeof(IConsumeConfigurator)).SingleInstance();
            builder.Services.RegisterType(typeof(InMemoryEventBusSubscriptionsManager)).As(typeof(IEventBusSubscriptionsManager)).SingleInstance();
            builder.Services.RegisterType(typeof(KafkaProducerPersistentConnection))
           .Named(KafkaConnectionType.Producer.ToString(), typeof(IKafkaPersisterConnection)).SingleInstance();
            builder.Services.RegisterType(typeof(KafkaConsumerPersistentConnection))
            .Named(KafkaConnectionType.Consumer.ToString(), typeof(IKafkaPersisterConnection)).SingleInstance();
            return builder;
        }

        #endregion 方法
    }
}