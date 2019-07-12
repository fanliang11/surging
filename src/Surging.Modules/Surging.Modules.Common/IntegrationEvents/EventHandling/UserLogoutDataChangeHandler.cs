using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.EventBusRabbitMQ.Attributes;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Common.Models.Events;
using System;
using System.Threading.Tasks;

namespace Surging.Modules.Common.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Defines the <see cref="UserLogoutDataChangeHandler" />
    /// </summary>
    [QueueConsumer("UserLogoutDateChangeHandler")]
    public class UserLogoutDataChangeHandler : IIntegrationEventHandler<LogoutEvent>
    {
        #region 字段

        /// <summary>
        /// Defines the _userService
        /// </summary>
        private readonly IUserService _userService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLogoutDataChangeHandler"/> class.
        /// </summary>
        public UserLogoutDataChangeHandler()
        {
            _userService = ServiceLocator.GetService<IUserService>("User");
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Handle
        /// </summary>
        /// <param name="@event">The event<see cref="LogoutEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Handle(LogoutEvent @event)
        {
            Console.WriteLine($"消费1。");
            await _userService.Update(int.Parse(@event.UserId), new UserModel()
            {
            });
            Console.WriteLine($"消费1失败。");
            throw new Exception();
        }

        #endregion 方法
    }
}