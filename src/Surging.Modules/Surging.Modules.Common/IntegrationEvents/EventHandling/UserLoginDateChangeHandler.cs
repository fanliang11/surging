using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Attributes;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Common.Models.Events;
using System;
using System.Threading.Tasks;

namespace Surging.Modules.Common.IntegrationEvents.EventHandling
{
    [QueueConsumer("UserLoginDateChangeHandler",QueueConsumerMode.Normal,QueueConsumerMode.Fail)]
    public  class UserLoginDateChangeHandler : BaseIntegrationEventHandler<UserEvent>
    {
        private readonly IUserService _userService;
        public UserLoginDateChangeHandler()
        {
            _userService = ServiceLocator.GetService<IUserService>("User");
         }
        public override async Task Handle(UserEvent @event)
        {
            Console.WriteLine($"消费1。");
            await _userService.Update(@event.UserId, new UserModel()
            {
                Age = @event.Age,
                Name = @event.Name,
                UserId = @event.UserId
            });
            Console.WriteLine($"消费1失败。");
            throw new Exception();
        }

        public override Task Handled(EventContext context)
        {
            Console.WriteLine($"调用{context.Count}次。类型:{context.Type}");
            var model = context.Content as UserEvent;
            return Task.CompletedTask;
        }
    }
}
