using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.EventBusKafka.Implementation
{
    public class EventBusKafka : IEventBus, IDisposable
    {
        private readonly ILogger<EventBusKafka> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IKafkaPersisterConnection _producerConnection;
        private readonly IKafkaPersisterConnection _consumerConnection;

        public event EventHandler OnShutdown;

        public EventBusKafka( ILogger<EventBusKafka> logger,
            IEventBusSubscriptionsManager subsManager,
            CPlatformContainer serviceProvider)
        { 
            this._logger = logger;
            this._producerConnection = serviceProvider.GetInstances<IKafkaPersisterConnection>(KafkaConnectionType.Producer.ToString());
            this._consumerConnection = serviceProvider.GetInstances<IKafkaPersisterConnection>(KafkaConnectionType.Consumer.ToString());
            _subsManager = subsManager;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, ValueTuple<string,string> tuple)
        {
            if (!_consumerConnection.IsConnected)
            {
                _consumerConnection.TryConnect();
            }

            using (var channel = _consumerConnection.CreateConnect() as Consumer<Null, string>)
            {
                channel.Unsubscribe();
                if (_subsManager.IsEmpty)
                { 
                    _consumerConnection.Dispose();
                }
            }
        }

        public void Dispose()
        {
            _producerConnection.Dispose();
            _consumerConnection.Dispose();
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_producerConnection.IsConnected)
            {
                _producerConnection.TryConnect();
            }
            var eventName = @event.GetType()
                   .Name;
            var body = JsonConvert.SerializeObject(@event);
            var policy = RetryPolicy.Handle<KafkaException>()
               .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   _logger.LogWarning(ex.ToString());
               });

            var conn = _producerConnection.CreateConnect() as Producer<Null, string>;
            policy.Execute(() =>
           {
               conn.ProduceAsync(eventName, null, body).GetAwaiter().GetResult();
           });
        }

        public void Subscribe<T, TH>(Func<TH> handler) where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
            if (!containsKey)
            {
                var channel = _consumerConnection.CreateConnect() as Consumer<Null, string>;
                channel.OnMessage += ConsumerClient_OnMessage;
                channel.Subscribe(eventName);
            }
            _subsManager.AddSubscription<T, TH>(handler, null);
        }
        
        public void Unsubscribe<T, TH>() where TH : IIntegrationEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>();
        }
        
        private void ConsumerClient_OnMessage(object sender, Message<Null, string> e)
        {
            ProcessEvent(e.Topic, e.Value).Wait();
        }
        
        private async Task ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var eventType = _subsManager.GetEventTypeByName(eventName);
                var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                var handlers = _subsManager.GetHandlersForEvent(eventName);

                foreach (var handlerfactory in handlers)
                {
                    try
                    {
                        var handler = handlerfactory.DynamicInvoke();
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
