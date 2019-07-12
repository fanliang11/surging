using Microsoft.Extensions.Configuration;
using Surging.Core.EventBusKafka.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.EventBusKafka
{
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public class AppConfig
    {
        #region 字段

        /// <summary>
        /// Defines the _kafkaConsumerConfig
        /// </summary>
        private static IEnumerable<KeyValuePair<string, object>> _kafkaConsumerConfig;

        /// <summary>
        /// Defines the _kafkaProducerConfig
        /// </summary>
        private static IEnumerable<KeyValuePair<string, object>> _kafkaProducerConfig;

        #endregion 字段

        #region 属性

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the KafkaConsumerConfig
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> KafkaConsumerConfig
        {
            get
            {
                return _kafkaConsumerConfig;
            }
            internal set
            {
                _kafkaConsumerConfig = value;
            }
        }

        /// <summary>
        /// Gets or sets the KafkaProducerConfig
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> KafkaProducerConfig
        {
            get
            {
                return _kafkaProducerConfig;
            }
            internal set
            {
                _kafkaProducerConfig = value;
            }
        }

        /// <summary>
        /// Gets or sets the Options
        /// </summary>
        public static KafkaOptions Options { get; internal set; }

        #endregion 属性
    }
}