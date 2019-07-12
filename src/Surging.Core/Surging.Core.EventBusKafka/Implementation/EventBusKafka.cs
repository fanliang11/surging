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
    /// <summary>
    /// Defines the <see cref="EventBusKafka" />
    /// </summary>
    public class EventBusKafka : IEventBus, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _consumerConnection
        /// </summary>
        private readonly IKafkaPersisterConnection _consumerConnection;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<EventBusKafka> _logger;

        /// <summary>
        /// Defines the _producerConnection
        /// </summary>
        private readonly IKafkaPersisterConnection _producerConnection;

        /// <summary>
        /// Defines the _subsManager
        /// </summary>
        private readonly IEventBusSubscriptionsManager _subsManager;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBusKafka"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{EventBusKafka}"/></param>
        /// <param name="subsManager">The subsManager<see cref="IEventBusSubscriptionsManager"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public EventBusKafka(ILogger<EventBusKafka> logger,
            IEventBusSubscriptionsManager subsManager,
            CPlatformContainer serviceProvider)
        {
            this._logger = logger;
            this._producerConnection = serviceProvider.GetInstances<IKafkaPersisterConnection>(KafkaConnectionType.Producer.ToString());
            this._consumerConnection = serviceProvider.GetInstances<IKafkaPersisterConnection>(KafkaConnectionType.Consumer.ToString());
            _subsManager = subsManager;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Defines the OnShutdown
        /// </summary>
        public event EventHandler OnShutdown;

        #endregion 事件

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _producerConnection.Dispose();
            _consumerConnection.Dispose();
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="@event">The event<see cref="IntegrationEvent"/></param>
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

        /// <summary>
        /// The Subscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler">The handler<see cref="Func{TH}"/></param>
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

        /// <summary>
        /// The Unsubscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void Unsubscribe<T, TH>() where TH : IIntegrationEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        /// <summary>
        /// The ConsumerClient_OnMessage
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="Message{Null, string}"/></param>
        private void ConsumerClient_OnMessage(object sender, Message<Null, string> e)
        {
            ProcessEvent(e.Topic, e.Value).Wait();
        }

        /// <summary>
        /// The ProcessEvent
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The SubsManager_OnEventRemoved
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="tuple">The tuple<see cref="ValueTuple{string,string}"/></param>
        private void SubsManager_OnEventRemoved(object sender, ValueTuple<string, string> tuple)
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

        #endregion 方法
    }
}