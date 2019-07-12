using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus.Implementation
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IEventBus" />
    /// </summary>
    public interface IEventBus
    {
        #region 事件

        /// <summary>
        /// Defines the OnShutdown
        /// </summary>
        event EventHandler OnShutdown;

        #endregion 事件

        #region 方法

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="@event">The event<see cref="IntegrationEvent"/></param>
        void Publish(IntegrationEvent @event);

        /// <summary>
        /// The Subscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler">The handler<see cref="Func{TH}"/></param>
        void Subscribe<T, TH>(Func<TH> handler)
            where TH : IIntegrationEventHandler<T>;

        /// <summary>
        /// The Unsubscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>;

        #endregion 方法
    }

    #endregion 接口
}