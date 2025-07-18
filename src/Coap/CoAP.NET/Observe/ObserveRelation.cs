/*
 * Copyright (c) 2011-2015, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System; 
using CoAP.Net;
using CoAP.Server.Resources;
using CoAP.Util;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Observe
{
    /// <summary>
    /// Represents a relation between a client endpoint and a resource on this server.
    /// </summary>
    public class ObserveRelation
    {
        private readonly ILogger log;
        readonly ICoapConfig _config;
        readonly ObservingEndpoint _endpoint;
        readonly IResource _resource;
        readonly Exchange _exchange;
        private Response _recentControlNotification;
        private Response _nextControlNotification;
        private String _key;
        private Boolean _established;
        private DateTime _interestCheckTime = DateTime.Now;
        private Int32 _interestCheckCounter = 1;

        /// <summary>
        /// The notifications that have been sent, so they can be removed from the Matcher
        /// </summary>
        private ConcurrentQueue<Response> _notifications = new ConcurrentQueue<Response>();

        /// <summary>
        /// Constructs a new observe relation.
        /// </summary>
        /// <param name="config">the config</param>
        /// <param name="endpoint">the observing endpoint</param>
        /// <param name="resource">the observed resource</param>
        /// <param name="exchange">the exchange that tries to establish the observe relation</param>
        public ObserveRelation(ICoapConfig config, ObservingEndpoint endpoint, IResource resource, Exchange exchange)
        {
            log=ServiceLocator.GetService<ILogger<ObserveRelation>>();
            if (config == null)
                throw ThrowHelper.ArgumentNull("config");
            if (endpoint == null)
                throw ThrowHelper.ArgumentNull("endpoint");
            if (resource == null)
                throw ThrowHelper.ArgumentNull("resource");
            if (exchange == null)
                throw ThrowHelper.ArgumentNull("exchange");
            _config = config;
            _endpoint = endpoint;
            _resource = resource;
            _exchange = exchange;
            _key = String.Format("{0}#{1}", Source, exchange.Request.TokenString);
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public IResource Resource
        {
            get { return _resource; }
        }

        /// <summary>
        /// Gets the exchange.
        /// </summary>
        public Exchange Exchange
        {
            get { return _exchange; }
        }

        public String Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the source endpoint of the observing endpoint.
        /// </summary>
        public System.Net.EndPoint Source
        {
            get { return _endpoint.EndPoint; }
        }

        public Response CurrentControlNotification
        {
            get { return _recentControlNotification; }
            set { _recentControlNotification = value; }
        }

        public Response NextControlNotification
        {
            get { return _nextControlNotification; }
            set { _nextControlNotification = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this relation has been established.
        /// </summary>
        public Boolean Established
        {
            get { return _established; }
            set { _established = value; }
        }

        /// <summary>
        /// Cancel this observe relation.
        /// </summary>
        public void Cancel()
        {
           if (log.IsEnabled(LogLevel.Debug))
                 log.LogDebug("Cancel observe relation from " + _key + " with " + _resource.Path);
            // stop ongoing retransmissions
            if (_exchange.Response != null)
                _exchange.Response.Cancel();
            _established = false;
            _resource.RemoveObserveRelation(this);
            _endpoint.RemoveObserveRelation(this);
            _exchange.Complete = true;
        }

        /// <summary>
        /// Cancel all observer relations that this server has
        /// established with this's realtion's endpoint.
        /// </summary>
        public void CancelAll()
        {
            _endpoint.CancelAll();
        }

        /// <summary>
        /// Notifies the observing endpoint that the resource has been changed.
        /// </summary>
        public void NotifyObservers()
        {
            // makes the resource process the same request again
            _resource.HandleRequest(_exchange);
        }

        public Boolean Check()
        {
            Boolean check = false;
            DateTime now = DateTime.Now;
            check |= _interestCheckTime.AddMilliseconds(_config.NotificationCheckIntervalTime) < now;
            check |= (++_interestCheckCounter >= _config.NotificationCheckIntervalCount);
            if (check)
            {
                _interestCheckTime = now;
                _interestCheckCounter = 0;
            }
            return check;
        }

        public void AddNotification(Response notification)
        {
            _notifications.Enqueue(notification);
        }

        public IEnumerable<Response> ClearNotifications()
        {
            Response resp;
            while (_notifications.TryDequeue(out resp))
            {
                yield return resp;
            }
        }
    }
}
