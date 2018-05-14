using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
   public static class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static  string HostName => Configuration != null ? Configuration["EventBusConnection"]:"";

        public static string RabbitUserName => Configuration != null ? Configuration["EventBusUserName"] ?? "guest" : "guest";

        public static string RabbitPassword =>Configuration != null ? Configuration["EventBusPassword"] ?? "guest" : "guest";

        public static string VirtualHost => Configuration !=null? Configuration["VirtualHost"] ?? "/": "/";

        public static string Port => Configuration != null ? Configuration["Port"] ?? "5672" : "5672";

        public static string BrokerName => Configuration != null ? Configuration["BrokerName"] ?? "surging": "surging";

    }
}
