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
    /// <summary>
    /// Defines the <see cref="KafkaPersistentConnectionBase" />
    /// </summary>
    public abstract class KafkaPersistentConnectionBase : IKafkaPersisterConnection
    {
        #region 字段

        /// <summary>
        /// Defines the _config
        /// </summary>
        private readonly IEnumerable<KeyValuePair<string, object>> _config;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<KafkaPersistentConnectionBase> _logger;

        /// <summary>
        /// Defines the sync_root
        /// </summary>
        internal object sync_root = new object();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaPersistentConnectionBase"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{KafkaPersistentConnectionBase}"/></param>
        /// <param name="config">The config<see cref="IEnumerable{KeyValuePair{string, object}}"/></param>
        public KafkaPersistentConnectionBase(ILogger<KafkaPersistentConnectionBase> logger,
            IEnumerable<KeyValuePair<string, object>> config)
        {
            this._logger = logger;
            _config = config;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        public abstract bool IsConnected { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Connection
        /// </summary>
        /// <param name="options">The options<see cref="IEnumerable{KeyValuePair{string, object}}"/></param>
        /// <returns>The <see cref="Action"/></returns>
        public abstract Action Connection(IEnumerable<KeyValuePair<string, object>> options);

        /// <summary>
        /// The CreateConnect
        /// </summary>
        /// <returns>The <see cref="object"/></returns>
        public abstract object CreateConnect();

        /// <summary>
        /// The Dispose
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// The TryConnect
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
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

        #endregion 方法
    }
}