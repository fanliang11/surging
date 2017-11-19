using Surging.Core.CPlatform.EventBus;
using Surging.Core.ServiceHosting.Internal;
using Autofac;

namespace Surging.Core.EventBusRabbitMQ
{
    public static class ServiceHostBuilderExtensions
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
