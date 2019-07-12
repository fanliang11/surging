using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka.Configurations
{
    /// <summary>
    /// Defines the <see cref="KafkaOptions" />
    /// </summary>
    public class KafkaOptions
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Acks
        /// </summary>
        public string Acks { get; set; } = "all";

        /// <summary>
        /// Gets or sets the CommitInterval
        /// </summary>
        public int CommitInterval { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether EnableAutoCommit
        /// </summary>
        public bool EnableAutoCommit { get; set; } = false;

        /// <summary>
        /// Gets or sets the GroupID
        /// </summary>
        public string GroupID { get; set; } = "suringdemo";

        /// <summary>
        /// Gets or sets the Linger
        /// </summary>
        public int Linger { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether LogConnectionClose
        /// </summary>
        public bool LogConnectionClose { get; set; } = false;

        /// <summary>
        /// Gets or sets the MaxQueueBuffering
        /// </summary>
        public int MaxQueueBuffering { get; set; } = 10;

        /// <summary>
        /// Gets or sets the MaxSocketBlocking
        /// </summary>
        public int MaxSocketBlocking { get; set; } = 10;

        /// <summary>
        /// Gets or sets the OffsetReset
        /// </summary>
        public OffsetResetMode OffsetReset { get; set; } = OffsetResetMode.Earliest;

        /// <summary>
        /// Gets or sets the Retries
        /// </summary>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or sets the Servers
        /// </summary>
        public string Servers { get; set; } = "localhost:9092";

        /// <summary>
        /// Gets or sets the SessionTimeout
        /// </summary>
        public int SessionTimeout { get; set; } = 36000;

        /// <summary>
        /// Gets or sets the Timeout
        /// </summary>
        public int Timeout { get; set; } = 100;

        /// <summary>
        /// Gets the Value
        /// </summary>
        public KafkaOptions Value => this;

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetConsumerConfig
        /// </summary>
        /// <returns>The <see cref="IEnumerable{KeyValuePair{string, object}}"/></returns>
        public IEnumerable<KeyValuePair<string, object>> GetConsumerConfig()
        {
            var configs = new List<KeyValuePair<string, object>>();
            configs.Add(new KeyValuePair<string, object>("bootstrap.servers", Servers));
            configs.Add(new KeyValuePair<string, object>("queue.buffering.max.ms", MaxQueueBuffering.ToString()));
            configs.Add(new KeyValuePair<string, object>("socket.blocking.max.ms", MaxSocketBlocking.ToString()));
            configs.Add(new KeyValuePair<string, object>("enable.auto.commit", EnableAutoCommit.ToString()));
            configs.Add(new KeyValuePair<string, object>("log.connection.close", LogConnectionClose.ToString()));
            configs.Add(new KeyValuePair<string, object>("auto.commit.interval.ms", CommitInterval));
            configs.Add(new KeyValuePair<string, object>("auto.offset.reset", OffsetReset.ToString().ToLower()));
            configs.Add(new KeyValuePair<string, object>("session.timeout.ms", SessionTimeout));
            configs.Add(new KeyValuePair<string, object>("group.id", GroupID));
            return configs;
        }

        /// <summary>
        /// The GetProducerConfig
        /// </summary>
        /// <returns>The <see cref="IEnumerable{KeyValuePair{string, object}}"/></returns>
        public IEnumerable<KeyValuePair<string, object>> GetProducerConfig()
        {
            var configs = new List<KeyValuePair<string, object>>();
            configs.Add(new KeyValuePair<string, object>("bootstrap.servers", Servers));
            configs.Add(new KeyValuePair<string, object>("acks", Acks));
            configs.Add(new KeyValuePair<string, object>("retries", Retries));
            configs.Add(new KeyValuePair<string, object>("linger.ms", Linger));
            return configs;
        }

        #endregion 方法
    }
}