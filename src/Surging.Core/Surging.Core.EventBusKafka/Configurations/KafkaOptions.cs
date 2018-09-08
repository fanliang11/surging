using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka.Configurations
{
    public class KafkaOptions
    {
        public string Servers { get; set; } = "localhost:9092";

        public int MaxQueueBuffering { get; set; } = 10;

        public int MaxSocketBlocking { get; set; } = 10;

        public bool EnableAutoCommit { get; set; } = false;

        public bool LogConnectionClose { get; set; } = false;

        public int Timeout { get; set; } = 100;

        public int CommitInterval { get; set; } = 1000;

        public OffsetResetMode OffsetReset { get; set; } = OffsetResetMode.Earliest;

        public int SessionTimeout { get; set; } = 36000;

        public string Acks { get; set; } = "all";

        public int Retries { get; set; } 

        public int Linger { get; set; } = 1;

        public string GroupID { get; set; } = "suringdemo";

        public KafkaOptions Value => this;

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

        public IEnumerable<KeyValuePair<string, object>> GetProducerConfig()
        {
            var configs = new List<KeyValuePair<string, object>>();
            configs.Add(new KeyValuePair<string, object>("bootstrap.servers", Servers));
            configs.Add(new KeyValuePair<string, object>("acks", Acks));
            configs.Add(new KeyValuePair<string, object>("retries", Retries)); 
            configs.Add(new KeyValuePair<string, object>("linger.ms", Linger)); 
            return configs;
        }
    }
}
