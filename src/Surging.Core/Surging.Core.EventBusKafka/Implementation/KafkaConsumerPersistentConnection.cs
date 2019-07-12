using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Surging.Core.EventBusKafka.Implementation
{
    /// <summary>
    /// Defines the <see cref="KafkaConsumerPersistentConnection" />
    /// </summary>
    public class KafkaConsumerPersistentConnection : KafkaPersistentConnectionBase
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<KafkaConsumerPersistentConnection> _logger;

        /// <summary>
        /// Defines the _stringDeserializer
        /// </summary>
        private readonly IDeserializer<string> _stringDeserializer;

        /// <summary>
        /// Defines the _disposed
        /// </summary>
        internal bool _disposed;

        /// <summary>
        /// Defines the _consumerClient
        /// </summary>
        private Consumer<Null, string> _consumerClient;

        /// <summary>
        /// Defines the _consumerClients
        /// </summary>
        private ConcurrentBag<Consumer<Null, string>> _consumerClients;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumerPersistentConnection"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{KafkaConsumerPersistentConnection}"/></param>
        public KafkaConsumerPersistentConnection(ILogger<KafkaConsumerPersistentConnection> logger)
            : base(logger, AppConfig.KafkaConsumerConfig)
        {
            _logger = logger;
            _stringDeserializer = new StringDeserializer(Encoding.UTF8);
            _consumerClients = new ConcurrentBag<Consumer<Null, string>>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        public override bool IsConnected => _consumerClient != null && !_disposed;

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Connection
        /// </summary>
        /// <param name="options">The options<see cref="IEnumerable{KeyValuePair{string, object}}"/></param>
        /// <returns>The <see cref="Action"/></returns>
        public override Action Connection(IEnumerable<KeyValuePair<string, object>> options)
        {
            return () =>
            {
                _consumerClient = new Consumer<Null, string>(options, null, _stringDeserializer);
                _consumerClient.OnConsumeError += OnConsumeError;
                _consumerClient.OnError += OnConnectionException;
                _consumerClients.Add(_consumerClient);
            };
        }

        /// <summary>
        /// The CreateConnect
        /// </summary>
        /// <returns>The <see cref="object"/></returns>
        public override object CreateConnect()
        {
            TryConnect();
            return _consumerClient;
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public override void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _consumerClient.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        /// <summary>
        /// The Listening
        /// </summary>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/></param>
        public void Listening(TimeSpan timeout)
        {
            if (!IsConnected)
            {
                TryConnect();
            }
            while (true)
            {
                foreach (var client in _consumerClients)
                {
                    client.Poll(timeout);

                    if (!client.Consume(out Message<Null, string> msg, (int)timeout.TotalMilliseconds))
                    {
                        continue;
                    }
                    if (msg.Offset % 5 == 0)
                    {
                        var committedOffsets = client.CommitAsync(msg).Result;
                    }
                }
            }
        }

        /// <summary>
        /// The OnConnectionException
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="error">The error<see cref="Error"/></param>
        private void OnConnectionException(object sender, Error error)
        {
            if (_disposed) return;

            _logger.LogWarning($"A Kafka connection throw exception.info:{error} ,Trying to re-connect...");

            TryConnect();
        }

        /// <summary>
        /// The OnConsumeError
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="Message"/></param>
        private void OnConsumeError(object sender, Message e)
        {
            var message = e.Deserialize<Null, string>(null, _stringDeserializer);
            if (_disposed) return;

            _logger.LogWarning($"An error occurred during consume the message; Topic:'{e.Topic}'," +
                $"Message:'{message.Value}', Reason:'{e.Error}'.");

            TryConnect();
        }

        #endregion 方法
    }
}