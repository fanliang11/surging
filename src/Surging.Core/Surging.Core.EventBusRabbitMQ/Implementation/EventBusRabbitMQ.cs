using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.EventBusRabbitMQ.Attributes;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.EventBusRabbitMQ.Implementation
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly string BROKER_NAME;

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;


        private IModel _consumerChannel;
        private string _queueName;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger, IEventBusSubscriptionsManager subsManager)
        {
            BROKER_NAME = AppConfig.BrokerName;
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, ValueTuple<string, string> tuple)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: tuple.Item1,
                    exchange: BROKER_NAME,
                    routingKey: tuple.Item2);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex.ToString());
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = @event.GetType()
                    .Name;

                channel.ExchangeDeclare(exchange: BROKER_NAME,
                                    type: "direct");
           
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    channel.BasicPublish(exchange: BROKER_NAME,
                                     routingKey: eventName,
                                     basicProperties: null,
                                     body: body);
                });
            }
        }

        public void Subscribe<T, TH>(Func<TH> handler)
            where TH : IIntegrationEventHandler<T>
        {  
            var eventName = typeof(T).Name;
            var queueConsumerAttr = typeof(TH).GetCustomAttribute<QueueConsumerAttribute>();
            if (queueConsumerAttr == null)
                throw new ArgumentNullException("QueueConsumerAttribute");
            var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                _consumerChannel = CreateConsumerChannel(queueConsumerAttr.QueueName);
                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: queueConsumerAttr.QueueName,
                                      exchange: BROKER_NAME,
                                      routingKey: eventName); 
                      var properties = channel.CreateBasicProperties();  
                      properties.DeliveryMode = 2;  
                }
            }

            _subsManager.AddSubscription<T, TH>(handler, queueConsumerAttr.QueueName);

        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        private static Func<IIntegrationEventHandler> FindHandlerByType(Type handlerType, IEnumerable<Func<IIntegrationEventHandler>> handlers)
        {
            foreach (var func in handlers)
            {
                if (func.GetMethodInfo().ReturnType == handlerType)
                {
                    return func;
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }
            
            _subsManager.Clear();
        }

        private IModel CreateConsumerChannel(string queueName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();
         
            channel.ExchangeDeclare(exchange: BROKER_NAME,
                                 type: "direct");

            channel.QueueDeclare(queueName,true,false,false,null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                await ProcessEvent(eventName, message);
            };

            channel.BasicConsume(queue: queueName,
                                  autoAck:true,
                                 consumer: consumer);
         
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel(queueName);
            };

            return channel;
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
                    var handler = handlerfactory.DynamicInvoke();
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                }
            }
        }
    }
}
