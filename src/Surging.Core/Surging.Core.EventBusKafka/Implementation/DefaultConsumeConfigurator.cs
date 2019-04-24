using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.EventBusKafka.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.EventBusKafka.Implementation
{
   public class DefaultConsumeConfigurator : IConsumeConfigurator
    {
        private readonly IEventBus _eventBus;
        private readonly CPlatformContainer _container;
        public DefaultConsumeConfigurator(IEventBus eventBus, CPlatformContainer container)
        {
            _eventBus = eventBus;
            _container = container;
        }

        public void Configure(List<Type> consumers)
        {
            foreach (var consumer in consumers)
            {
                if (consumer.GetTypeInfo().IsGenericType)
                {
                    continue;
                }
                var consumerType = consumer.GetInterfaces()
                    .Where(
                        d =>
                            d.GetTypeInfo().IsGenericType &&
                            d.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                    .Select(d => d.GetGenericArguments().Single())
                    .First();
                try
                {
                    var type = consumer;
                    this.FastInvoke(new[] { consumerType, consumer },
                        x => x.ConsumerTo<object, IIntegrationEventHandler<object>>());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void Unconfigure(List<Type> consumers)
        {
            foreach (var consumer in consumers)
            {
                if (consumer.GetTypeInfo().IsGenericType)
                {
                    continue;
                }
                var consumerType = consumer.GetInterfaces()
                    .Where(
                        d =>
                            d.GetTypeInfo().IsGenericType &&
                            d.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                    .Select(d => d.GetGenericArguments().Single())
                    .First();
                try
                {
                    var type = consumer;
                    this.FastInvoke(new[] { consumerType, consumer },
                        x => x.RemoveConsumer<object, IIntegrationEventHandler<object>>());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        protected void ConsumerTo<TEvent, TConsumer>()
            where TConsumer : IIntegrationEventHandler<TEvent>
            where TEvent : class
        {
            _eventBus.Subscribe<TEvent, TConsumer>
              (() => (TConsumer)_container.GetInstances(typeof(TConsumer)));
        }

        protected void RemoveConsumer<TEvent, TConsumer>()
  where TConsumer : IIntegrationEventHandler<TEvent>
  where TEvent : class
        {
            _eventBus.Unsubscribe<TEvent, TConsumer>();
        }
    }
}
    
