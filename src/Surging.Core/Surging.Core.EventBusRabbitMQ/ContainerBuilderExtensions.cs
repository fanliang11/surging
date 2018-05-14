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
            var services = builder.Services;
            builder.Services.RegisterType(typeof(Implementation.EventBusRabbitMQ)).As(typeof(IEventBus)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultConsumeConfigurator)).As(typeof(IConsumeConfigurator)).SingleInstance();
            builder.Services.RegisterType(typeof(InMemoryEventBusSubscriptionsManager)).As(typeof(IEventBusSubscriptionsManager)).SingleInstance();
            builder.Services.Register(provider =>
            {
                var logger = provider.Resolve<ILogger<DefaultRabbitMQPersistentConnection>>();        
                var factory = new ConnectionFactory()
                {
                    HostName = AppConfig.HostName,
                    UserName =  AppConfig.RabbitUserName,
                    Password = AppConfig.RabbitPassword,
                    VirtualHost= AppConfig.VirtualHost,
                    Port = int.Parse(AppConfig.Port),
                };
                factory.RequestedHeartbeat = 60;
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
