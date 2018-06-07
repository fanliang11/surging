using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul
{
   public class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }
    }
}
