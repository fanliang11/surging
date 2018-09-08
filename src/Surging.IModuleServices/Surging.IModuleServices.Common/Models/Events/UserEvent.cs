using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models.Events
{
    public class UserEvent : IntegrationEvent
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }
    }
}
