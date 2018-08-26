using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus;
using System;
using Autofac;
using Surging.Core.EventBusRabbitMQ.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Microsoft.Extensions.Configuration;

namespace Surging.Core.EventBusRabbitMQ
{
    public static class ContainerBuilderExtensions
    {

        /// <summary>
        /// 使用RabbitMQ进行传输。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseRabbitMQTransport(this IServiceBuilder builder)
        {
            builder.Services.RegisterType(typeof(Implementation.EventBusRabbitMQ)).As(typeof(IEventBus)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultConsumeConfigurator)).As(typeof(IConsumeConfigurator)).SingleInstance();
            builder.Services.RegisterType(typeof(InMemoryEventBusSubscriptionsManager)).As(typeof(IEventBusSubscriptionsManager)).SingleInstance();
            builder.Services.Register(provider =>
            {
                var logger = provider.Resolve<ILogger<DefaultRabbitMQPersistentConnection>>();
                EventBusOption option = new EventBusOption();
                var section = CPlatform.AppConfig.GetSection("EventBus");
                if (section.Exists())
                    option = section.Get<EventBusOption>();
                else if (AppConfig.Configuration != null)
                    option = AppConfig.Configuration.Get<EventBusOption>();
                var factory = new ConnectionFactory()
                {
                    HostName = option.EventBusConnection,
                    UserName = option.EventBusUserName,
                    Password = option.EventBusPassword,
                    VirtualHost = option.VirtualHost,
                    Port = int.Parse(option.Port),
                };
                factory.RequestedHeartbeat = 60;
                AppConfig.BrokerName = option.BrokerName;
                AppConfig.MessageTTL = option.MessageTTL;
                AppConfig.RetryCount = option.RetryCount;
                AppConfig.FailCount = option.FailCount;
                AppConfig.PrefetchCount = option.PrefetchCount;
                return new DefaultRabbitMQPersistentConnection(factory, logger);
            }).As<IRabbitMQPersistentConnection>();
            return builder;
        }

        public static IServiceBuilder UseRabbitMQEventAdapt(this IServiceBuilder builder, Func<IServiceProvider, ISubscriptionAdapt> adapt)
        {
            var services = builder.Services;
            services.RegisterAdapter(adapt);
            return builder;
        }

        public static IServiceBuilder AddRabbitMQAdapt(this IServiceBuilder builder)
        {
            return builder.UseRabbitMQEventAdapt(provider =>
             new RabbitMqSubscriptionAdapt(
                 provider.GetService<IConsumeConfigurator>(),
                 provider.GetService<IEnumerable<IIntegrationEventHandler>>()
                 )
            );
        }
    }
}
