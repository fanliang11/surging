using Autofac;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka
{
   public static  class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder SubscribeAt(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ISubscriptionAdapt>().SubscribeAt();
            });
        }
    }
}
