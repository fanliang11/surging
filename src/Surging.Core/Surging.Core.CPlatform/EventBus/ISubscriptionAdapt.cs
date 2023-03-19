using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus
{
   public interface ISubscriptionAdapt
    {
        void SubscribeAt();

        void Unsubscribe();
    }
}
