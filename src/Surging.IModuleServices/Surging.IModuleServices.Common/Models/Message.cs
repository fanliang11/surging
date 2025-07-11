using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
   public class Message
    {
        public string MessageId { get; set; }
        public string RoutePath { get; set; }

        public string ServiceKey { get; set; }

        public string FailCallbackRoutePath { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public SchedulerConfig Config { get; set; }
    }
}
