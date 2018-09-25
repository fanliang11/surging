using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;

namespace Surging.Core.EventBusKafka.Implementation
{
    public class KafkaProducerPersistentConnection : KafkaPersistentConnectionBase
    {
        private Producer<Null, string> _connection;
        private readonly ILogger<KafkaProducerPersistentConnection> _logger;
        private readonly ISerializer<string> _stringSerializer;
        bool _disposed;

        public KafkaProducerPersistentConnection(ILogger<KafkaProducerPersistentConnection> logger)
            :base(logger,AppConfig.KafkaProducerConfig)
        { 
            _logger = logger;
            _stringSerializer = new StringSerializer(Encoding.UTF8);
        }

        public override bool IsConnected =>   _connection != null &&  !_disposed;
             

        public override Action Connection(IEnumerable<KeyValuePair<string, object>> options)
        {
            return () =>
            { 
                _connection = new Producer<Null, string>(options,null, _stringSerializer);
                _connection.OnError += OnConnectionException; 
            };
        }

        public override object CreateConnect()
        {
            TryConnect();
            return _connection;
        }

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

        private void OnConnectionException(object sender, Error error)
        {
            if (_disposed) return;

            _logger.LogWarning($"A Kafka connection throw exception.info:{error} ,Trying to re-connect...");

            TryConnect();
        }


    }
}
