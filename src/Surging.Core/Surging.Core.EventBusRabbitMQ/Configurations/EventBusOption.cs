using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Configurations
{
    /// <summary>
    /// Defines the <see cref="EventBusOption" />
    /// </summary>
    public class EventBusOption
    {
        #region 属性

        /// <summary>
        /// Gets or sets the BrokerName
        /// </summary>
        public string BrokerName { get; set; } = "surging";

        /// <summary>
        /// Gets or sets the EventBusConnection
        /// </summary>
        public string EventBusConnection { get; set; } = "";

        /// <summary>
        /// Gets or sets the EventBusPassword
        /// </summary>
        public string EventBusPassword { get; set; } = "guest";

        /// <summary>
        /// Gets or sets the EventBusUserName
        /// </summary>
        public string EventBusUserName { get; set; } = "guest";

        /// <summary>
        /// Gets or sets the FailCount
        /// </summary>
        public int FailCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the MessageTTL
        /// </summary>
        public int MessageTTL { get; set; } = 30 * 1000;

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public string Port { get; set; } = "5672";

        /// <summary>
        /// Gets or sets the PrefetchCount
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// Gets or sets the RetryCount
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the VirtualHost
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        #endregion 属性
    }
}