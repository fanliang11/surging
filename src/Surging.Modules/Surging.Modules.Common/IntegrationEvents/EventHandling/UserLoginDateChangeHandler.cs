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
    /// <summary>
    /// Defines the <see cref="UserLoginDateChangeHandler" />
    /// </summary>
    [QueueConsumer("UserLoginDateChangeHandler", QueueConsumerMode.Normal, QueueConsumerMode.Fail)]
    public class UserLoginDateChangeHandler : BaseIntegrationEventHandler<UserEvent>
    {
        #region 字段

        /// <summary>
        /// Defines the _userService
        /// </summary>
        private readonly IUserService _userService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginDateChangeHandler"/> class.
        /// </summary>
        public UserLoginDateChangeHandler()
        {
            _userService = ServiceLocator.GetService<IUserService>("User");
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Handle
        /// </summary>
        /// <param name="@event">The event<see cref="UserEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The Handled
        /// </summary>
        /// <param name="context">The context<see cref="EventContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override Task Handled(EventContext context)
        {
            Console.WriteLine($"调用{context.Count}次。类型:{context.Type}");
            var model = context.Content as UserEvent;
            return Task.CompletedTask;
        }

        #endregion 方法
    }
}