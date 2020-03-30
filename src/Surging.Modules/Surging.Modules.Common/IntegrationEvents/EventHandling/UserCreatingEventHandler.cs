using System;
using System.Threading.Tasks;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Attributes;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Common.Models.Events;

namespace Surging.Modules.Common.IntegrationEvents.EventHandling
{
    [QueueConsumer("UserCreatingEventHandler", QueueConsumerMode.Normal, QueueConsumerMode.Fail)]
    public class UserCreatingEventHandler : BaseIntegrationEventHandler<EntityCreatingEvent<UserModel>>
    {
        public override async Task Handle(EntityCreatingEvent<UserModel> @event)
        {
            Console.WriteLine($"User: {@event.Entity.Name} is creating.");
        }
    }
}
