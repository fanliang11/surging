using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka.Implementation
{
    /// <summary>
    /// Defines the <see cref="KafkaSubscriptionAdapt" />
    /// </summary>
    public class KafkaSubscriptionAdapt : ISubscriptionAdapt
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
        /// Initializes a new instance of the <see cref="KafkaSubscriptionAdapt"/> class.
        /// </summary>
        /// <param name="consumeConfigurator">The consumeConfigurator<see cref="IConsumeConfigurator"/></param>
        /// <param name="integrationEventHandler">The integrationEventHandler<see cref="IEnumerable{IIntegrationEventHandler}"/></param>
        public KafkaSubscriptionAdapt(IConsumeConfigurator consumeConfigurator, IEnumerable<IIntegrationEventHandler> integrationEventHandler)
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