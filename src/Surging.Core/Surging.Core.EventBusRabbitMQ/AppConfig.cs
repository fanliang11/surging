using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
    public static class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }


        public static string BrokerName { get; internal set; }

        public static ushort PrefetchCount { get; set; } 

        public static int RetryCount { get; internal set; } = 3;

        public static int FailCount { get; internal set; } = 3;

        public static int MessageTTL { get; internal set; } = 30 * 1000;
    }
}
