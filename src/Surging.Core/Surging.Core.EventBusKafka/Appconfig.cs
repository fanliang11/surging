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
                return _kafkaConfig;
            }
            internal set
            {
                _kafkaConfig = value;
            }
        }
    }
}
