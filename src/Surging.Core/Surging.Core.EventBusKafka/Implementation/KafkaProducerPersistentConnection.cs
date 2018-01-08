using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Surging.Core.EventBusKafka.Implementation
{
    public class KafkaProducerPersistentConnection : KafkaPersistentConnectionBase
    {
        private  Producer _connection;
        private readonly ILogger<KafkaProducerPersistentConnection> _logger;
        bool _disposed;

        public KafkaProducerPersistentConnection(ILogger<KafkaProducerPersistentConnection> logger)
            :base(logger)
        {
            _logger = logger;
        }

        public override bool IsConnected =>   _connection != null &&  !_disposed;
             

        public override Action Connection(IEnumerable<KeyValuePair<string, object>> options)
        {
            return () =>
            {
                _connection = new Producer(options);
                _connection.OnError += OnConnectionException;
 
            };
        }

        public override object CreateConnect()
        {
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
