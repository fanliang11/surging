using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.Configurations
{
   public class ConsulOption
    {
        public string SessionTimeout { get; set; }

        public string ConnectionString { get; set; }

        public string RoutePath { get; set; }

        public string SubscriberPath { get; set; }

        public string CommandPath { get; set; }

        public string CachePath { get; set; }

        public string MqttRoutePath { get; set; }

        public string ReloadOnChange { get; set; }

        public string EnableChildrenMonitor { get; set; }

        public int? LockDelay { get; set; }
    }
}
