using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.EventBusRabbitMQ.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.EventBusRabbitMQ.Implementation
{
    /// <summary>
    /// Defines the <see cref="RabbitMqSubscriptionAdapt" />
    /// </summary>
    public class RabbitMqSubscriptionAdapt : ISubscriptionAdapt
    {
        #region 字段

        /// <summary>
        /// Defines the _consumeConfigurator
        /// </summary>
        private readonly IConsumeConfigurator _consumeConfigurator;

        /// <summary>
        /// Defines the _integrationEventHandler
        /// </summary>
        private readonly IEnumerable<IIntegrationEventHandler> _integrationEventHandler;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqSubscriptionAdapt"/> class.
        /// </summary>
        /// <param name="consumeConfigurator">The consumeConfigurator<see cref="IConsumeConfigurator"/></param>
        /// <param name="integrationEventHandler">The integrationEventHandler<see cref="IEnumerable{IIntegrationEventHandler}"/></param>
        public RabbitMqSubscriptionAdapt(IConsumeConfigurator consumeConfigurator, IEnumerable<IIntegrationEventHandler> integrationEventHandler)
        {
            this._consumeConfigurator = consumeConfigurator;
            this._integrationEventHandler = integrationEventHandler;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SubscribeAt
        /// </summary>
        public void SubscribeAt()
        {
            _consumeConfigurator.Configure(GetQueueConsumers());
        }

        /// <summary>
        /// The Unsubscribe
        /// </summary>
        public void Unsubscribe()
        {
            _consumeConfigurator.Unconfigure(GetQueueConsumers());
        }

        /// <summary>
        /// The GetQueueConsumers
        /// </summary>
        /// <returns>The <see cref="List{Type}"/></returns>
        private List<Type> GetQueueConsumers()
        {
            var result = new List<Type>();
            foreach (var consumer in _integrationEventHandler)
            {
                var type = consumer.GetType();
                result.Add(type);
            }
            return result;
        }

        #endregion 方法
    }
}