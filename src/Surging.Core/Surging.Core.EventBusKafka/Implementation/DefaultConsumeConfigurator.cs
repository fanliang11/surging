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
    /// <summary>
    /// Defines the <see cref="DefaultConsumeConfigurator" />
    /// </summary>
    public class DefaultConsumeConfigurator : IConsumeConfigurator
    {
        #region 字段

        /// <summary>
        /// Defines the _container
        /// </summary>
        private readonly CPlatformContainer _container;

        /// <summary>
        /// Defines the _eventBus
        /// </summary>
        private readonly IEventBus _eventBus;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConsumeConfigurator"/> class.
        /// </summary>
        /// <param name="eventBus">The eventBus<see cref="IEventBus"/></param>
        /// <param name="container">The container<see cref="CPlatformContainer"/></param>
        public DefaultConsumeConfigurator(IEventBus eventBus, CPlatformContainer container)
        {
            _eventBus = eventBus;
            _container = container;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="consumers">The consumers<see cref="List{Type}"/></param>
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

        /// <summary>
        /// The Unconfigure
        /// </summary>
        /// <param name="consumers">The consumers<see cref="List{Type}"/></param>
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

        /// <summary>
        /// The ConsumerTo
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TConsumer"></typeparam>
        protected void ConsumerTo<TEvent, TConsumer>()
            where TConsumer : IIntegrationEventHandler<TEvent>
            where TEvent : class
        {
            _eventBus.Subscribe<TEvent, TConsumer>
              (() => (TConsumer)_container.GetInstances(typeof(TConsumer)));
        }

        /// <summary>
        /// The RemoveConsumer
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TConsumer"></typeparam>
        protected void RemoveConsumer<TEvent, TConsumer>()
  where TConsumer : IIntegrationEventHandler<TEvent>
  where TEvent : class
        {
            _eventBus.Unsubscribe<TEvent, TConsumer>();
        }

        #endregion 方法
    }
}