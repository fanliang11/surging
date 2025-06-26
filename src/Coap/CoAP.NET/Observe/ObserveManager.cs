/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Collections.Concurrent;

namespace CoAP.Observe
{
    /// <summary>
    /// The observe manager holds a mapping of endpoint addresses to
    /// <see cref="ObservingEndpoint"/>s. It makes sure that there be only one
    /// ObservingEndpoint that represents the observe relations from one endpoint to
    /// this server. This important in case we want to cancel all relations to a
    /// specific endpoint, e.g., when a confirmable notification timeouts.
    /// <remarks>
    /// Notice that each server has its own ObserveManager. If a server binds to
    /// multiple endpoints, the ObserveManager keeps the observe relations for all of
    /// them.
    /// </remarks>
    /// </summary>
    public class ObserveManager
    {
        readonly ConcurrentDictionary<System.Net.EndPoint, ObservingEndpoint> _endpoints
            = new ConcurrentDictionary<System.Net.EndPoint, ObservingEndpoint>();

        /// <summary>
        /// Constructs a new observe manager.
        /// </summary>
        public ObserveManager()
        { }

        /// <summary>
        /// Finds the ObservingEndpoint for the specified endpoint address
        /// or create a new one if none exists yet.
        /// </summary>
        public ObservingEndpoint FindObservingEndpoint(System.Net.EndPoint endpoint)
        {
            return _endpoints.GetOrAdd(endpoint, ep => new ObservingEndpoint(ep));
        }

        /// <summary>
        /// Returns the ObservingEndpoint for the specified endpoint address
        /// or null if none exists.
        /// </summary>
        public ObservingEndpoint GetObservingEndpoint(System.Net.EndPoint endpoint)
        {
            ObservingEndpoint ep;
            _endpoints.TryGetValue(endpoint, out ep);
            return ep;
        }

        public ObserveRelation GetRelation(System.Net.EndPoint source, Byte[] token)
        {
            ObservingEndpoint remote = GetObservingEndpoint(source);
            return remote == null ? null : remote.GetObserveRelation(token);
        }
    }
}
