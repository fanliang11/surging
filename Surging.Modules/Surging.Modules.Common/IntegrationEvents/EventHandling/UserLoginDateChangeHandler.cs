using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Surging.IModuleServices.Common.Models.Events;
using System.Threading.Tasks;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;

namespace Surging.Modules.Common.IntegrationEvents.EventHandling
{
  public  class UserLoginDateChangeHandler : IIntegrationEventHandler<UserEvent>
    {
        private readonly IUserService _userService;
        public UserLoginDateChangeHandler(IUserService userService)
        {
            _userService = userService;
        }
        public async Task Handle(UserEvent @event)
        {
            await _userService.Update( int.Parse(@event.UserId),new UserModel()
            {

            });
        }
    }
}
