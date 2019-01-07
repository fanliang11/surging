using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.EventBusRabbitMQ.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Surging.Core.CPlatform.Utilities.FastInvoke;

namespace Surging.Core.EventBusRabbitMQ.Implementation
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly string BROKER_NAME;
        private readonly int _messageTTL;
        private readonly int _retryCount;
        private readonly int _rollbackCount;
        private readonly ushort _prefetchCount;
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IDictionary<QueueConsumerMode, string> _exchanges;

        private IDictionary<Tuple<string,QueueConsumerMode>, IModel> _consumerChannels;

        public event EventHandler OnShutdown;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger, IEventBusSubscriptionsManager subsManager)
        {
            BROKER_NAME = AppConfig.BrokerName;
            _messageTTL = AppConfig.MessageTTL;
            _retryCount = AppConfig.RetryCount;
            _prefetchCount = AppConfig.PrefetchCount;
            _rollbackCount = AppConfig.FailCount;
            _consumerChannels = new Dictionary<Tuple<string,QueueConsumerMode>, IModel>();
            _exchanges = new Dictionary<QueueConsumerMode, string>();
            _exchanges.Add(QueueConsumerMode.Normal, BROKER_NAME);
            _exchanges.Add(QueueConsumerMode.Retry, $"{BROKER_NAME}@{QueueConsumerMode.Retry.ToString()}");
            _exchanges.Add(QueueConsumerMode.Fail, $"{BROKER_NAME}@{QueueConsumerMode.Fail.ToString()}");
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _persistentConnection.OnRabbitConnectionShutdown += PersistentConnection_OnEventShutDown;
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
                var _modeNames = Enum.GetNames(typeof(QueueConsumerMode));

                using (var channel = _persistentConnection.CreateModel())
                {
                    foreach (var modeName in _modeNames)
                    {
                        var mode = Enum.Parse<QueueConsumerMode>(modeName);

                        string queueName = "";

                        if (mode != QueueConsumerMode.Normal)
                            queueName = $"{queueConsumerAttr.QueueName}@{mode.ToString()}";
                        else
                            queueName = queueConsumerAttr.QueueName;
                        var key = new Tuple<string, QueueConsumerMode>(queueName, mode);
                        if (_consumerChannels.ContainsKey(key))
                        {
                            _consumerChannels[key].Close();
                            _consumerChannels.Remove(key);
                        }
                        _consumerChannels.Add(key,
                            CreateConsumerChannel(queueConsumerAttr, eventName, mode));
                        channel.QueueBind(queue: queueName,
                                          exchange: _exchanges[mode],
                                          routingKey: eventName); 
                    }
                }
            }
            if (!_subsManager.HasSubscriptionsForEvent<T>())
                _subsManager.AddSubscription<T, TH>(handler, queueConsumerAttr.QueueName);
        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
        {
            if (_subsManager.HasSubscriptionsForEvent<T>())
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

        private IModel CreateConsumerChannel(QueueConsumerAttribute queueConsumer, string routeKey, QueueConsumerMode mode)
        {
            IModel result = null;
            switch (mode)
            {
                case QueueConsumerMode.Normal:
                    {
                        var bindConsumer = queueConsumer.Modes.Any(p => p == QueueConsumerMode.Normal);
                        result = CreateConsumerChannel(queueConsumer.QueueName,
                           bindConsumer);
                    }
                    break;
                case QueueConsumerMode.Retry:
                    {
                        var bindConsumer = queueConsumer.Modes.Any(p => p == QueueConsumerMode.Retry);
                        result = CreateRetryConsumerChannel(queueConsumer.QueueName, routeKey,
                            bindConsumer);
                    }
                    break;
                case QueueConsumerMode.Fail:
                    {
                        var bindConsumer = queueConsumer.Modes.Any(p => p == QueueConsumerMode.Fail);
                        result = CreateFailConsumerChannel(queueConsumer.QueueName,
                              bindConsumer);
                    }
                    break;
            }
            return result;
        }

        private IModel CreateConsumerChannel(string queueName, bool bindConsumer)
        {
            var mode = QueueConsumerMode.Normal;
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
                await ProcessEvent(eventName, ea.Body, mode, ea.BasicProperties);
                channel.BasicAck(ea.DeliveryTag, false);
            };
            if (bindConsumer)
            {
                channel.BasicQos(0, _prefetchCount, false);
                channel.BasicConsume(queue: queueName,
                                  autoAck: false,
                                 consumer: consumer);
                channel.CallbackException += (sender, ea) =>
                {
                    var key = new Tuple<string, QueueConsumerMode>(queueName, mode);
                    _consumerChannels[key].Dispose();
                    _consumerChannels[key] = CreateConsumerChannel(queueName, bindConsumer);
                };
            }
            else
                channel.Close();
            return channel;
        }

        private IModel CreateRetryConsumerChannel(string queueName, string routeKey, bool bindConsumer)
        {
            var mode = QueueConsumerMode.Retry;
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
            IDictionary<String, Object> arguments = new Dictionary<String, Object>();
            arguments.Add("x-dead-letter-exchange", _exchanges[QueueConsumerMode.Fail].ToString());
            arguments.Add("x-message-ttl", _messageTTL);
            arguments.Add("x-dead-letter-routing-key", routeKey);
            var channel = _persistentConnection.CreateModel();
            var retryQueueName = $"{queueName}@{mode.ToString()}";
            channel.ExchangeDeclare(exchange: _exchanges[mode],
                                 type: "direct");
            channel.QueueDeclare(retryQueueName, true, false, false, arguments);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                await ProcessEvent(eventName, ea.Body, mode,ea.BasicProperties);
                channel.BasicAck(ea.DeliveryTag, false);
            };
            if (bindConsumer)
            {
                channel.BasicQos(0, _prefetchCount, false);
                channel.BasicConsume(queue: retryQueueName,
                                      autoAck: false,
                                     consumer: consumer);
                channel.CallbackException += (sender, ea) =>
              {
                  var key = new Tuple<string, QueueConsumerMode>(queueName, mode);
                  _consumerChannels[key].Dispose();
                  _consumerChannels[key] = CreateRetryConsumerChannel(queueName, routeKey, bindConsumer);
              };
            }
            else
                channel.Close();


            return channel;
        }

        private IModel CreateFailConsumerChannel(string queueName, bool bindConsumer)
        {
            var mode = QueueConsumerMode.Fail;
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
            var channel = _persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange: _exchanges[mode],
                                type: "direct");
            var failQueueName = $"{queueName}@{mode.ToString()}";
            channel.QueueDeclare(failQueueName, true, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                await ProcessEvent(eventName, ea.Body, mode, ea.BasicProperties);
                channel.BasicAck(ea.DeliveryTag, false);
            };
            if (bindConsumer)
            {
                channel.BasicQos(0, _prefetchCount, false);
                channel.BasicConsume(queue: failQueueName,
                                      autoAck: false,
                                     consumer: consumer);
                channel.CallbackException += (sender, ea) =>
                {
                    var key = new Tuple<string, QueueConsumerMode>(queueName, mode);
                    _consumerChannels[key].Dispose();
                    _consumerChannels[key] = CreateFailConsumerChannel(queueName, bindConsumer);
                };
            }
            else
                channel.Close();

            return channel;
        }

        private async Task ProcessEvent(string eventName, byte[] body, QueueConsumerMode mode, IBasicProperties properties)
        {
            var message = Encoding.UTF8.GetString(body);
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var eventType = _subsManager.GetEventTypeByName(eventName);
                var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                var handlers = _subsManager.GetHandlersForEvent(eventName);
                 var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                foreach (var handlerfactory in handlers)
                {
                    var handler = handlerfactory.DynamicInvoke();
                    long retryCount = 1; 
                    try
                    {
                        var fastInvoker = GetHandler($"{concreteType.FullName}.Handle", concreteType.GetMethod("Handle"));
                        await (Task)fastInvoker(handler, new object[] { integrationEvent });
                    }
                    catch
                    {
                        if (!_persistentConnection.IsConnected)
                        {
                            _persistentConnection.TryConnect();
                        }
                        retryCount = GetRetryCount(properties);
                        using (var channel = _persistentConnection.CreateModel())
                        {
                            if (retryCount > _retryCount)
                            {
                                // 重试次数大于3次，则自动加入到死信队列
                                var rollbackCount = retryCount - _retryCount;
                                if (rollbackCount < _rollbackCount)
                                {
                                    IDictionary<String, Object> headers = new Dictionary<String, Object>();
                                    if (!headers.ContainsKey("x-orig-routing-key"))
                                        headers.Add("x-orig-routing-key", GetOrigRoutingKey(properties, eventName));
                                    retryCount = rollbackCount;
                                    channel.BasicPublish(_exchanges[QueueConsumerMode.Fail], eventName, CreateOverrideProperties(properties, headers), body);

                                }
                            }
                            else
                            {
                                IDictionary<String, Object> headers = properties.Headers;
                                if (headers == null)
                                {
                                    headers = new Dictionary<String, Object>();
                                }
                                if (!headers.ContainsKey("x-orig-routing-key"))
                                    headers.Add("x-orig-routing-key", GetOrigRoutingKey(properties, eventName)); 
                                channel.BasicPublish(_exchanges[QueueConsumerMode.Retry], eventName, CreateOverrideProperties(properties, headers), body);
                            }
                        }
                    }
                    finally
                    {
                        var baseConcreteType = typeof(BaseIntegrationEventHandler<>).MakeGenericType(eventType); 
                       if (handler.GetType().BaseType== baseConcreteType)
                        { 
                            var context = new EventContext()
                            {
                                Content= integrationEvent,
                                Count = retryCount,
                                Type = mode.ToString()
                            };
                            var fastInvoker = GetHandler($"{baseConcreteType.FullName}.Handled", baseConcreteType.GetMethod("Handled"));
                            await (Task)fastInvoker(handler, new object[] { context });
                        }
                    }
                }
            }
        }

        private IBasicProperties CreateOverrideProperties(IBasicProperties properties,
    IDictionary<String, Object> headers)
        {
            IBasicProperties newProperties = new BasicProperties();
            newProperties.ContentType = properties.ContentType ?? "";
            newProperties.ContentEncoding = properties.ContentEncoding ?? "";
            newProperties.Headers = properties.Headers;
            if (newProperties.Headers == null)
                newProperties.Headers = new Dictionary<string, object>();
            foreach (var key in headers.Keys)
            {
                if (!newProperties.Headers.ContainsKey(key))
                    newProperties.Headers.Add(key, headers[key]);
            }
            newProperties.DeliveryMode = properties.DeliveryMode;
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
            long retryCount = 1L;
            try
            {
                if (properties != null)
                {

                    IDictionary<String, Object> headers = properties.Headers;
                    if (headers != null)
                    {
                        if (headers.ContainsKey("x-death"))
                        {
                            retryCount= GetRetryCount(properties, headers);
                        }
                        else
                        {
                            var death = new Dictionary<string, object>();
                            death.Add("count", retryCount);
                            headers.Add("x-death", death);
                        }
                    }
                }
            }
            catch { }
            return retryCount;
        }

       private  long GetRetryCount(IBasicProperties properties, IDictionary<String, Object> headers)
        {
            var retryCount = 1L;
            if (headers["x-death"] is List<object>)
            {
                var deaths = (List<object>)headers["x-death"];
                if (deaths.Count > 0)
                {
                    IDictionary<String, Object> death = deaths[0] as Dictionary<String, Object>;
                    retryCount = (long)death["count"];
                    death["count"] = ++retryCount;
                }
            }
            else
            {
                Dictionary<String, Object> death = (Dictionary<String, Object>)headers["x-death"];
                if (death != null)
                {
                    retryCount = (long)death["count"];
                    death["count"] = ++retryCount;
                    properties.Headers = headers;
                }
            }
            return retryCount;
        }

        private FastInvokeHandler GetHandler(string key, MethodInfo method)
        {
            var objInstance = ServiceResolver.Current.GetService(null, key);
            if (objInstance == null)
            {
                objInstance = FastInvoke.GetMethodInvoker(method);
                ServiceResolver.Current.Register(key, objInstance, null);
            }
            return objInstance as FastInvokeHandler;
        }

        private void PersistentConnection_OnEventShutDown(object sender, ShutdownEventArgs reason)
        {
            OnShutdown(this,new EventArgs());
        }
    }
}
