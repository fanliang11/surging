﻿using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Mqtt.Implementation
{
    /// <summary>
    /// Defines the <see cref="MqttServiceRouteChangedEventArgs" />
    /// </summary>
    public class MqttServiceRouteChangedEventArgs : MqttServiceRouteEventArgs
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttServiceRouteChangedEventArgs"/> class.
        /// </summary>
        /// <param name="route">The route<see cref="MqttServiceRoute"/></param>
        /// <param name="oldRoute">The oldRoute<see cref="MqttServiceRoute"/></param>
        public MqttServiceRouteChangedEventArgs(MqttServiceRoute route, MqttServiceRoute oldRoute) : base(route)
        {
            OldRoute = oldRoute;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the OldRoute
        /// </summary>
        public MqttServiceRoute OldRoute { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="MqttServiceRouteEventArgs" />
    /// </summary>
    public class MqttServiceRouteEventArgs
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttServiceRouteEventArgs"/> class.
        /// </summary>
        /// <param name="route">The route<see cref="MqttServiceRoute"/></param>
        public MqttServiceRouteEventArgs(MqttServiceRoute route)
        {
            Route = route;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Route
        /// </summary>
        public MqttServiceRoute Route { get; private set; }

        #endregion 属性
    }

    public abstract class MqttServiceRouteManagerBase : IMqttServiceRouteManager
    {
        private readonly ISerializer<string> _serializer;
        private EventHandler<MqttServiceRouteEventArgs> _created;
        private EventHandler<MqttServiceRouteEventArgs> _removed;
        private EventHandler<MqttServiceRouteChangedEventArgs> _changed;

        protected MqttServiceRouteManagerBase(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        public event EventHandler<MqttServiceRouteEventArgs> Created
        {
            add { _created += value; }
            remove { _created -= value; }
        }

        public event EventHandler<MqttServiceRouteEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }

        public event EventHandler<MqttServiceRouteChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        public abstract Task ClearAsync();

        public abstract Task<IEnumerable<MqttServiceRoute>> GetRoutesAsync();

        public abstract Task RemveAddressAsync(IEnumerable<AddressModel> addresses);

        public abstract Task RemoveByTopicAsync(string topic, IEnumerable<AddressModel> endpoint);

        public virtual Task SetRoutesAsync(IEnumerable<MqttServiceRoute> routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            var descriptors = routes.Where(route => route != null).Select(route => new MqttServiceDescriptor
            {
                AddressDescriptors = route.MqttEndpoint?.Select(address => new MqttEndpointDescriptor
                {
                    Type = address.GetType().FullName,
                    Value = _serializer.Serialize(address)
                }) ?? Enumerable.Empty<MqttEndpointDescriptor>(),
                MqttDescriptor = route.MqttDescriptor
            });
            return SetRoutesAsync(descriptors);
        }

        protected abstract Task SetRoutesAsync(IEnumerable<MqttServiceDescriptor> descriptors);

        protected void OnCreated(params MqttServiceRouteEventArgs[] args)
        {
            if (_created == null)
                return;

            foreach (var arg in args)
                _created(this, arg);
        }

        protected void OnChanged(params MqttServiceRouteChangedEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
        }

        protected void OnRemoved(params MqttServiceRouteEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }
    }
}