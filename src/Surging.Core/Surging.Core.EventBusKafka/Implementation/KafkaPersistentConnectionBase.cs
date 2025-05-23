using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Surging.Core.EventBusKafka.Implementation
{
    public abstract class KafkaPersistentConnectionBase : IKafkaPersisterConnection
    {
        private readonly ILogger<KafkaPersistentConnectionBase> _logger;
        private readonly IEnumerable<KeyValuePair<string, object>> _config;
        object sync_root = new object();

        public KafkaPersistentConnectionBase(ILogger<KafkaPersistentConnectionBase> logger,
            IEnumerable<KeyValuePair<string, object>> config)
        {
            this._logger = logger;
            _config = config;
        }
        

         public abstract bool IsConnected { get; }

        public bool TryConnect()
        {
            _logger.LogInformation("Kafka Client is trying to connect");

            lock (sync_root)
            {
                var policy = RetryPolicy.Handle<KafkaException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    Connection(_config).Invoke();
                });

                if (IsConnected)
                {
                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: Kafka connections could not be created and opened");
                    return false;
                }
            }
        }

        public abstract Action Connection(IEnumerable<KeyValuePair<string, object>> options);
        
        public abstract object CreateConnect();
        public abstract void Dispose();
    }
}
