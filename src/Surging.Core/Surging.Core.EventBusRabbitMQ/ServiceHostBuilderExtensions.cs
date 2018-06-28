using Surging.Core.CPlatform.EventBus;
using Surging.Core.ServiceHosting.Internal;
using Autofac;
using Surging.Core.CPlatform.Engines;

namespace Surging.Core.EventBusRabbitMQ
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder SubscribeAt(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
                {
                      mapper.Resolve<ISubscriptionAdapt>().SubscribeAt();
                });
            });
        }
    }
}
