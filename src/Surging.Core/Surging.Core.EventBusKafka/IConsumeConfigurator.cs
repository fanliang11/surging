using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka
{
   public interface IConsumeConfigurator
    {
        void Configure(List<Type> consumers);
    }
}
