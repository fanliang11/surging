using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Configurations
{
    public class EventBusOption
    {
        public string EventBusConnection {get; set; }= "";

        public string EventBusUserName { get; set; } = "guest";

        public string EventBusPassword { get; set; } = "guest";

        public string VirtualHost { get; set; } = "/";

        public string Port { get; set; } = "5672";

        public string BrokerName { get; set; } = "surging";
    }
}
