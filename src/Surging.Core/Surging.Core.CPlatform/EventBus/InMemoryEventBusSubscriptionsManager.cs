using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.CPlatform.EventBus
{
    /// <summary>
    /// Defines the <see cref="InMemoryEventBusSubscriptionsManager" />
    /// </summary>
    public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        #region 字段

        /// <summary>
        /// Defines the _consumers
        /// </summary>
        private readonly Dictionary<Delegate, string> _consumers;

        /// <summary>
        /// Defines the _eventTypes
        /// </summary>
        private readonly List<Type> _eventTypes;

        /// <summary>
        /// Defines the _handlers
        /// </summary>
        private readonly Dictionary<string, List<Delegate>> _handlers;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventBusSubscriptionsManager"/> class.
        /// </summary>
        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<Delegate>>();
            _consumers = new Dictionary<Delegate, string>();
            _eventTypes = new List<Type>();
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Defines the OnEventRemoved
        /// </summary>
        public event EventHandler<ValueTuple<string, string>> OnEventRemoved;

        #endregion 事件

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsEmpty
        /// </summary>
        public bool IsEmpty => !_handlers.Keys.Any();

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddSubscription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler">The handler<see cref="Func{TH}"/></param>
        /// <param name="consumerName">The consumerName<see cref="string"/></param>
        public void AddSubscription<T, TH>(Func<TH> handler, string consumerName)
            where TH : IIntegrationEventHandler<T>
        {
            var key = GetEventKey<T>();
            if (!HasSubscriptionsForEvent<T>())
            {
                _handlers.Add(key, new List<Delegate>());
            }
            _handlers[key].Add(handler);
            _consumers.Add(handler, consumerName);
            _eventTypes.Add(typeof(T));
        }

        /// <summary>
        /// The Clear
        /// </summary>
        public void Clear() => _handlers.Clear();

        /// <summary>
        /// The GetEventTypeByName
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <returns>The <see cref="Type"/></returns>
        public Type GetEventTypeByName(string eventName) => _eventTypes.Single(t => t.Name == eventName);

        /// <summary>
        /// The GetHandlersForEvent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="IEnumerable{Delegate}"/></returns>
        public IEnumerable<Delegate> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        /// <summary>
        /// The GetHandlersForEvent
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <returns>The <see cref="IEnumerable{Delegate}"/></returns>
        public IEnumerable<Delegate> GetHandlersForEvent(string eventName) => _handlers[eventName];

        /// <summary>
        /// The HasSubscriptionsForEvent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="bool"/></returns>
        public bool HasSubscriptionsForEvent<T>()
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        /// <summary>
        /// The HasSubscriptionsForEvent
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        /// <summary>
        /// The RemoveSubscription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindHandlerToRemove<T, TH>();
            if (handlerToRemove != null)
            {
                var key = GetEventKey<T>();
                var consumerName = _consumers[handlerToRemove];
                _handlers[key].Remove(handlerToRemove);
                if (!_handlers[key].Any())
                {
                    _handlers.Remove(key);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == key);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                        _consumers.Remove(handlerToRemove);
                        RaiseOnEventRemoved(eventType.Name, consumerName);
                    }
                }
            }
        }

        /// <summary>
        /// The FindHandlerToRemove
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <returns>The <see cref="Delegate"/></returns>
        private Delegate FindHandlerToRemove<T, TH>()
            where TH : IIntegrationEventHandler<T>
        {
            if (!HasSubscriptionsForEvent<T>())
            {
                return null;
            }

            var key = GetEventKey<T>();
            foreach (var func in _handlers[key])
            {
                var genericArgs = func.GetType().GetGenericArguments();
                if (genericArgs.SingleOrDefault() == typeof(TH))
                {
                    return func;
                }
            }

            return null;
        }

        /// <summary>
        /// The GetEventKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="string"/></returns>
        private string GetEventKey<T>()
        {
            return typeof(T).Name;
        }

        /// <summary>
        /// The RaiseOnEventRemoved
        /// </summary>
        /// <param name="eventName">The eventName<see cref="string"/></param>
        /// <param name="consumerName">The consumerName<see cref="string"/></param>
        private void RaiseOnEventRemoved(string eventName, string consumerName)
        {
            var handler = OnEventRemoved;
            if (handler != null)
            {
                handler(this, new ValueTuple<string, string>(consumerName, eventName));
            }
        }

        #endregion 方法
    }
}