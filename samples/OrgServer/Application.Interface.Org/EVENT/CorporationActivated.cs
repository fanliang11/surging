using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface.Org.EVENT
{
    /// <summary>
    /// 新注册公司被激活的事件
    /// </summary>
   public class CorporationActivatedEvent: IntegrationEvent
    {
        public Guid CorpId { get; set; }
        public string  Email { get; set; }
        public string  EmpId { get; set; }

    }
}
