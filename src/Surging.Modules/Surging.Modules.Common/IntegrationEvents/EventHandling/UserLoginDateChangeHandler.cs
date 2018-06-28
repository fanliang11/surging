using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Surging.IModuleServices.Common.Models.Events;
using System.Threading.Tasks;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.EventBusRabbitMQ.Attributes;

namespace Surging.Modules.Common.IntegrationEvents.EventHandling
{

    [QueueConsumer("UserLoginDateChangeHandler")]
    public  class UserLoginDateChangeHandler : IIntegrationEventHandler<UserEvent>
    {
        private readonly IUserService _userService;
        public UserLoginDateChangeHandler()
        {
            _userService = ServiceLocator.GetService<IUserService>("User");
        }
        public async Task Handle(UserEvent @event)
        {
            Console.WriteLine($"消费1。");
            await _userService.Update(int.Parse(@event.UserId), new UserModel()
            {

            });
        }
    }
}
