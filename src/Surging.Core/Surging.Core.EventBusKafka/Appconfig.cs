using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.EventBusKafka
{
    public  class AppConfig
    {

        public static IConfigurationRoot Configuration { get; set; }

        private static IEnumerable<KeyValuePair<string, object>> _kafkaConfig;

        public static IEnumerable<KeyValuePair<string, object>> KafkaConfig
        {

            get
            {
                if (_kafkaConfig == null)
                {
                    _kafkaConfig = new Dictionary<string, object>()
                    {
                        {"bootstrap.servers","127.0.0.1"},
                        { "queue.buffering.max.ms","10" },
                         {"socket.blocking.max.ms","10"},
                        { "enable.auto.commit","false"},
                        {"log.connection.close","false"}
                    }.AsEnumerable();
                }
                return _kafkaConfig;
            }
        }
    }
}
