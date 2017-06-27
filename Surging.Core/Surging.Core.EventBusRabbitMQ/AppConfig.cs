using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
   public static class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }
    }
}
