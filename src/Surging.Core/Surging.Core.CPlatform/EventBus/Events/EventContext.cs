using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus.Events
{
    public class EventContext: IntegrationEvent
    {
     
        public object Content { get; set; }

        public long Count { get; set; }

        public string Type { get; set; }
    }
}
