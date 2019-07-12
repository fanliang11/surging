using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IEventBusSubscriptionsManager" />
    /// </summary>
    public interface IEventBusSubscriptionsManager
    {
        #region 事件

        /// <summary>
        /// Defines the OnEventRemoved
        /// </summary>
        event EventHandler<ValueTuple<string, string>> OnEventRemoved;

        #endregion 事件

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsEmpty
        /// </summary>
        bool IsEmpty { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddSubscription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler">The handler<see cref="Func{TH}"/></param>
        /// <param name="consumerName">The consumerName<see cref="string"/></param>
        void AddSubscription<T, TH>(Func<TH> handler, string consumerName)
           where TH : IIntegrationEventHandler<T>;

        /// <summary>
        /// The Clear
        /// </summary>
        void Clear();

        /// <summary>
        /// The GetEventTypeByName
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <returns>The <see cref="Type"/></returns>
        Type GetEventTypeByName(string eventName);

        /// <summary>
        /// The GetHandlersForEvent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="IEnumerable{Delegate}"/></returns>
        IEnumerable<Delegate> GetHandlersForEvent<T>() where T : IntegrationEvent;

        /// <summary>
        /// The GetHandlersForEvent
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <returns>The <see cref="IEnumerable{Delegate}"/></returns>
        IEnumerable<Delegate> GetHandlersForEvent(string eventName);

        /// <summary>
        /// The HasSubscriptionsForEvent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="bool"/></returns>
        bool HasSubscriptionsForEvent<T>();

        /// <summary>
        /// The HasSubscriptionsForEvent
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool HasSubscriptionsForEvent(string eventName);

        /// <summary>
        /// The RemoveSubscription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void RemoveSubscription<T, TH>()
             where TH : IIntegrationEventHandler<T>;

        #endregion 方法
    }

    #endregion 接口
}