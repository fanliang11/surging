using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace  Domain.DomainEvents.Inquiry
{
 public   class InquiryEditedEvent: IntegrationEvent 
    {
        public Guid CustomerKeyId { get; set; }
        public string CustomerName { get; set; }

    }
}
