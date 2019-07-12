using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.EventBusKafka.Implementation
{
    /// <summary>
    /// Defines the <see cref="KafkaProducerPersistentConnection" />
    /// </summary>
    public class KafkaProducerPersistentConnection : KafkaPersistentConnectionBase
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<KafkaProducerPersistentConnection> _logger;

        /// <summary>
        /// Defines the _stringSerializer
        /// </summary>
        private readonly ISerializer<string> _stringSerializer;

        /// <summary>
        /// Defines the _disposed
        /// </summary>
        internal bool _disposed;

        /// <summary>
        /// Defines the _connection
        /// </summary>
        private Producer<Null, string> _connection;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaProducerPersistentConnection"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{KafkaProducerPersistentConnection}"/></param>
        public KafkaProducerPersistentConnection(ILogger<KafkaProducerPersistentConnection> logger)
            : base(logger, AppConfig.KafkaProducerConfig)
        {
            _logger = logger;
            _stringSerializer = new StringSerializer(Encoding.UTF8);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        public override bool IsConnected => _connection != null && !_disposed;

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
                _connection = new Producer<Null, string>(options, null, _stringSerializer);
                _connection.OnError += OnConnectionException;
            };
        }

        /// <summary>
        /// The CreateConnect
        /// </summary>
        /// <returns>The <see cref="object"/></returns>
        public override object CreateConnect()
        {
            TryConnect();
            return _connection;
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
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
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

        #endregion 方法
    }
}