using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using Surging.Core.EventBusRabbitMQ.Attributes;
using System.Linq;

namespace Surging.Core.EventBusRabbitMQ.Implementation
{
    public class RabbitMqSubscriptionAdapt : ISubscriptionAdapt
    {
        private readonly IConsumeConfigurator _consumeConfigurator;
        private readonly IEnumerable<IIntegrationEventHandler> _integrationEventHandler;
        public RabbitMqSubscriptionAdapt(IConsumeConfigurator consumeConfigurator, IEnumerable<IIntegrationEventHandler> integrationEventHandler)
        {
            this._consumeConfigurator = consumeConfigurator;
            this._integrationEventHandler = integrationEventHandler;
        }
    
        public void SubscribeAt()
        {
            _consumeConfigurator.Configure(GetQueueConsumers());
        }

       public void Unsubscribe()
        {
            _consumeConfigurator.Unconfigure(GetQueueConsumers());
        }


        #region 私有方法
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
        #endregion
    }
}
