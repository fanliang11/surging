using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public static class AppConfig
    {
        #region 属性

        /// <summary>
        /// Gets or sets the BrokerName
        /// </summary>
        public static string BrokerName { get; internal set; }

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the FailCount
        /// </summary>
        public static int FailCount { get; internal set; } = 3;

        /// <summary>
        /// Gets or sets the MessageTTL
        /// </summary>
        public static int MessageTTL { get; internal set; } = 30 * 1000;

        /// <summary>
        /// Gets or sets the PrefetchCount
        /// </summary>
        public static ushort PrefetchCount { get; set; }

        /// <summary>
        /// Gets or sets the RetryCount
        /// </summary>
        public static int RetryCount { get; internal set; } = 3;

        #endregion 属性
    }
}