using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models.Events
{
   public class LogoutEvent : IntegrationEvent
    {
        public string UserId { get; set; }

        public string Name { get; set; }

        public string Age { get; set; }
    }
}
 