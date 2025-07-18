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
 
using CoAP.NET.Util;
using CoAP.Util;

namespace CoAP.Observe
{
    /// <summary>
    /// Represents an observing endpoint. It holds all observe relations
    /// that the endpoint has to this server. If a confirmable notification timeouts
    /// for the maximum times allowed the server assumes the client is no longer
    /// reachable and cancels all relations that it has established to resources.
    /// </summary>
    public class ObservingEndpoint
    {
        readonly System.Net.EndPoint _endpoint;
        readonly ICollection<ObserveRelation> _relations = new SynchronizedCollection<ObserveRelation>();

        /// <summary>
        /// Constructs a new observing endpoint.
        /// </summary>
        public ObservingEndpoint(System.Net.EndPoint ep)
        {
            _endpoint = ep;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> of this endpoint.
        /// </summary>
        public System.Net.EndPoint EndPoint
        {
            get { return _endpoint; }
        }

        /// <summary>
        /// Adds the specified observe relation.
        /// </summary>
        public void AddObserveRelation(ObserveRelation relation)
        {
            _relations.Add(relation);
        }

        /// <summary>
        /// Removes the specified observe relation.
        /// </summary>
        public void RemoveObserveRelation(ObserveRelation relation)
        {
            _relations.Remove(relation);
        }

        /// <summary>
        /// Finds the observe relation by token.
        /// </summary>
        public ObserveRelation GetObserveRelation(Byte[] token)
        {
            foreach (ObserveRelation relation in _relations)
            {
                if (ByteArrayUtils.Equals(token, relation.Exchange.Request.Token))
                    return relation;
            }
            return null;
        }

        /// <summary>
        /// Cancels all observe relations that this endpoint has established with
        /// resources from this server.
        /// </summary>
        public void CancelAll()
        {
            foreach (ObserveRelation relation in _relations)
            {
                relation.Cancel();
            }
        }
    }
}
