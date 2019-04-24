using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
   public  interface IConsumeConfigurator
    {
        void Configure(List<Type> consumers);

        void Unconfigure(List<Type> consumers);
    }
}
