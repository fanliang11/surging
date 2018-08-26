using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus.Events
{
    public class EventContext: IntegrationEvent
    {
        public EventContext(IntegrationEvent integrationEvent) : base(integrationEvent)
        {

        }
        public long Count { get; set; }

        public string Type { get; set; }
    }
}
