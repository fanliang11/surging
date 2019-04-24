using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
   public  enum QueueConsumerMode
    {
        Normal = 0,
        Retry,
        Fail,
    }
}
