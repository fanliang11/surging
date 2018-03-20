using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace   Domain.DomainEvents.Inquiry
{
    public class InquiryEditedHandler : IIntegrationEventHandler<InquiryEditedEvent>
    {
        public async Task Handle(InquiryEditedEvent @event)
        {
           
              await Task.FromResult("InquiryEditedEvent更改名字》"+@event.CustomerName+"请进行下一步操作");
        }
    }
}
