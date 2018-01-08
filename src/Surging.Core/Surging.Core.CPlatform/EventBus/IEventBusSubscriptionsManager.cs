using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus
{
   public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;
        void AddSubscription<T, TH>(Func<TH> handler)
           where TH : IIntegrationEventHandler<T>;

        void RemoveSubscription<T, TH>()
             where TH : IIntegrationEventHandler<T>;
        bool HasSubscriptionsForEvent<T>();
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<Delegate> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<Delegate> GetHandlersForEvent(string eventName);
    }
}
