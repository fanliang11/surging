using Application.Interface.Org.EVENT;
using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface.Auth.EVENT
{
    /// <summary>
    /// 事件处理器
    /// </summary>
  public interface IAuthEventHandler : IIntegrationEventHandler<CorporationActivatedEvent>
    {
    }
}
