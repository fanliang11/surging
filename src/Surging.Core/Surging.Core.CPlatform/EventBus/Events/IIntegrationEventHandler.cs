using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.EventBus.Events
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IIntegrationEventHandler{in TIntegrationEvent}" />
    /// </summary>
    /// <typeparam name="TIntegrationEvent"></typeparam>
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    {
        #region 方法

        /// <summary>
        /// The Handle
        /// </summary>
        /// <param name="@event">The event<see cref="TIntegrationEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Handle(TIntegrationEvent @event);

        #endregion 方法
    }

    /// <summary>
    /// Defines the <see cref="IIntegrationEventHandler" />
    /// </summary>
    public interface IIntegrationEventHandler
    {
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="BaseIntegrationEventHandler{TIntegrationEvent}" />
    /// </summary>
    /// <typeparam name="TIntegrationEvent"></typeparam>
    public abstract class BaseIntegrationEventHandler<TIntegrationEvent> : IIntegrationEventHandler<TIntegrationEvent>
    {
        #region 方法

        /// <summary>
        /// The Handle
        /// </summary>
        /// <param name="@event">The event<see cref="TIntegrationEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Handle(TIntegrationEvent @event);

        /// <summary>
        /// The Handled
        /// </summary>
        /// <param name="context">The context<see cref="EventContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public virtual async Task Handled(EventContext context)
        {
            await Task.CompletedTask;
        }

        #endregion 方法
    }
}