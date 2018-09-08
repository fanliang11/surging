using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Configurations
{
    public class EventBusOption
    {
        public string EventBusConnection { get; set; } = "";

        public string EventBusUserName { get; set; } = "guest";

        public string EventBusPassword { get; set; } = "guest";

        public string VirtualHost { get; set; } = "/";

        public string Port { get; set; } = "5672";

        public string BrokerName { get; set; } = "surging";

        public int RetryCount { get; set; } = 3;

        public int FailCount { get; set; } = 3;

        public ushort PrefetchCount { get; set; }

        public int MessageTTL { get; set; } = 30 * 1000;
    }
}
