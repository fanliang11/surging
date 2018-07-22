using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;
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
        private readonly int _messageTTL;
        private readonly int _retryCount;
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IDictionary<QueueConsumerMode, string> _exchanges;

        private IDictionary<QueueConsumerMode, IModel> _consumerChannels;
        private string _queueName;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger, IEventBusSubscriptionsManager subsManager)
        {
            BROKER_NAME = AppConfig.BrokerName;
            _messageTTL = AppConfig.MessageTTL;
            _retryCount = AppConfig.RetryCount;
            _exchanges = new Dictionary<QueueConsumerMode, string>();
            _exchanges.Add(QueueConsumerMode.Normal, BROKER_NAME);
            _exchanges.Add(QueueConsumerMode.Retry, $"{BROKER_NAME}@{QueueConsumerMode.Retry.ToString()}");
            _exchanges.Add(QueueConsumerMode.Rollback, $"{BROKER_NAME}@{QueueConsumerMode.Rollback.ToString()}");
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
                    foreach (var key in _consumerChannels.Keys)
                    {
                        _consumerChannels[key].Close();
                    };
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
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    channel.BasicPublish(exchange: BROKER_NAME,
                                     routingKey: eventName,
                                     basicProperties: properties,
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
                foreach (var mode in queueConsumerAttr.Modes)
                    _consumerChannels.Add(mode, CreateConsumerChannel(queueConsumerAttr.QueueName, eventName, mode));
                using (var channel = _persistentConnection.CreateModel())
                {
                    string brokerName = _exchanges[QueueConsumerMode.Retry];
                    string queueName = "";

                    foreach (var mode in queueConsumerAttr.Modes)
                    {
                        if (mode != QueueConsumerMode.Normal)
                            queueName = $"{queueConsumerAttr.QueueName}@{QueueConsumerMode.Retry.ToString()}";
                        else
                            queueName = queueConsumerAttr.QueueName;
                        channel.QueueBind(queue: queueConsumerAttr.QueueName,
                                          exchange: brokerName,
                                          routingKey: eventName);
                    }
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
            foreach (var key in _consumerChannels.Keys)
            {
                if (_consumerChannels[key] != null)
                {
                    _consumerChannels[key].Dispose();
                }
            }
            _subsManager.Clear();
        }

        private IModel CreateConsumerChannel(string queueName, string routeKey, QueueConsumerMode mode)
        {
            IModel result = null;
            switch (mode)
            {
                case QueueConsumerMode.Normal:
                    result = CreateConsumerChannel(queueName);
                    break;
                case QueueConsumerMode.Retry:
                    result = CreateRetryConsumerChannel(queueName, routeKey);
                    break;
                case QueueConsumerMode.Rollback:
                    result = CreateRollBackConsumerChannel(queueName);
                    break;
            }
            return result;
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

            channel.QueueDeclare(queueName, true, false, false, null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                await ProcessEvent(eventName, ea.Body, ea.BasicProperties);
            };

            channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                 consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannels[QueueConsumerMode.Normal].Dispose();
                _consumerChannels[QueueConsumerMode.Normal] = CreateConsumerChannel(queueName);
            };

            return channel;
        }

        private IModel CreateRetryConsumerChannel(string queueName, string routeKey)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
            IDictionary<String, Object> arguments = new Dictionary<String, Object>();
            arguments.Add("x-dead-letter-exchange", _exchanges[QueueConsumerMode.Rollback].ToString());
            arguments.Add("x-message-ttl", _messageTTL);
            arguments.Add("x-dead-letter-routing-key", routeKey);
            var channel = _persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange: _exchanges[QueueConsumerMode.Retry],
                                 type: "direct");
            channel.QueueDeclare($"{queueName}@{QueueConsumerMode.Retry.ToString()}", true, false, false, arguments);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                await ProcessEvent(eventName, ea.Body, ea.BasicProperties);
            };
            channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                 consumer: consumer);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannels[QueueConsumerMode.Retry].Dispose();
                _consumerChannels[QueueConsumerMode.Retry] = CreateConsumerChannel(queueName);
            };
            
            return channel;
        }

        private IModel CreateRollBackConsumerChannel(string queueName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchanges[QueueConsumerMode.Rollback],
                                type: "direct");

            channel.QueueDeclare($"{queueName}@{QueueConsumerMode.Rollback.ToString()}", true, false, false, null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                await ProcessEvent(eventName, ea.Body, ea.BasicProperties);
            };

            channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                 consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannels[QueueConsumerMode.Rollback].Dispose();
                _consumerChannels[QueueConsumerMode.Rollback] = CreateConsumerChannel(queueName);
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, byte[] body, IBasicProperties properties)
        {
            var message = Encoding.UTF8.GetString(body);
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
                        long retryCount = GetRetryCount(properties);
                        using (var channel = _persistentConnection.CreateModel())
                        {
                            if (retryCount > _retryCount)
                            {
                                // 重试次数大于3次，则自动加入到死信队列
                                IDictionary<String, Object> headers = new Dictionary<String, Object>();
                                headers.Add("x-orig-routing-key", GetOrigRoutingKey(properties, eventName));
                                channel.BasicPublish(_exchanges[QueueConsumerMode.Rollback], eventName, CreateOverrideProperties(properties, headers), body);
                            }
                            else
                            {
                                IDictionary<String, Object> headers = properties.Headers;
                                if (headers == null)
                                {
                                    headers = new Dictionary<String, Object>();
                                }
                                headers.Add("x-orig-routing-key", GetOrigRoutingKey(properties, eventName));
                                channel.BasicPublish(_exchanges[QueueConsumerMode.Retry], eventName, CreateOverrideProperties(properties, headers), body);
                            }
                        }
                    }
                }
            }
        }

        private IBasicProperties CreateOverrideProperties(IBasicProperties properties,
    IDictionary<String, Object> headers)
        {
            IBasicProperties newProperties = new BasicProperties();
            newProperties.ContentType = properties.ContentType;
            newProperties.ContentEncoding = properties.ContentEncoding;
            foreach (var key in headers.Keys)
            {
                newProperties.Headers.Add(key, headers[key]);
            }
            newProperties.DeliveryMode = properties.DeliveryMode;
            newProperties.Priority = properties.Priority;
            newProperties.CorrelationId = properties.CorrelationId;
            newProperties.ReplyTo = properties.ReplyTo;
            newProperties.Expiration = properties.Expiration;
            newProperties.MessageId = properties.MessageId;
            newProperties.Timestamp = properties.Timestamp;
            newProperties.Type = properties.Type;
            newProperties.ClusterId = properties.ClusterId;
            newProperties.UserId = properties.UserId;
            newProperties.AppId = properties.AppId;
            return newProperties;
        }

        private String GetOrigRoutingKey(IBasicProperties properties,
          String defaultValue)
        {
            String routingKey = defaultValue;
            if (properties != null)
            {
                IDictionary<String, Object> headers = properties.Headers;
                if (headers != null && headers.Count > 0)
                {
                    if (headers.ContainsKey("x-orig-routing-key"))
                    {
                        routingKey = headers["x-orig-routing-key"].ToString();
                    }
                }
            }
            return routingKey;
        }

        private long GetRetryCount(IBasicProperties properties)
        {
            long retryCount = 0L;
            try
            {
                IDictionary<String, Object> headers = properties.Headers;
                if (headers != null)
                {
                    if (headers.ContainsKey("x-death"))
                    {
                        List<Dictionary<String, Object>> deaths = (List<Dictionary<String, Object>>)headers["x-death"];
                        if (deaths.Count > 0)
                        {
                            IDictionary<String, Object> death = deaths[0];
                            retryCount = (long)death["count"];
                        }
                    }
                }
            }
            catch { }
            return retryCount;
        }
    }
}
