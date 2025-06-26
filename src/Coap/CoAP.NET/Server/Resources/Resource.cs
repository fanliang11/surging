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
using System.Collections.Concurrent; 
using CoAP.Net;
using CoAP.Observe;
using CoAP.Threading;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Server.Resources
{
    /// <summary>
    /// Basic implementation of a resource.
    /// Extend this class to write your own resources.
    /// </summary>
    public class Resource : IResource
    {
        static readonly IEnumerable<IEndPoint> EmptyEndPoints = new IEndPoint[0];
        private readonly ILogger log ;
        readonly ResourceAttributes _attributes = new ResourceAttributes();
        private String _name;
        private String _path = String.Empty;
        private Boolean _visible;
        private Boolean _observable;
        private MessageType _observeType = MessageType.Unknown;
        private IResource _parent;
        private IDictionary<String, IResource> _children
            = new ConcurrentDictionary<String, IResource>();
        private ConcurrentDictionary<String, ObserveRelation> _observeRelations
            = new ConcurrentDictionary<String, ObserveRelation>();
        private ObserveNotificationOrderer _notificationOrderer
            = new ObserveNotificationOrderer();

        /// <summary>
        /// Constructs a new resource with the specified name.
        /// </summary>
        /// <param name="name">the name</param>
        public Resource(String name)
            : this(name, true)
        { }

        /// <summary>
        /// Constructs a new resource with the specified name
        /// and makes it visible to clients if the flag is true.
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="visible">if the resource is visible or not</param>
        public Resource(String name, Boolean visible)
        {
            log=ServiceLocator.GetService<ILogger<CoapServer>>();
            _name = name;
            _visible = visible;
        }

        /// <inheritdoc/>
        public String Name
        {
            get { return _name; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                lock (this)
                {
                    IResource parent = _parent;
					if (parent == null)
					{
						_name = value;
					}
					else
					{
                        lock (parent)
                        {
                            parent.Remove(this);
                            _name = value;
                            parent.Add(this);
                        }
                    }
                    AdjustChildrenPath();
                }
            }
        }

        [Obsolete("Use Attributes.Title instead.")]
        public String Title
        {
            get { return Attributes.Title; }
            set { Attributes.Title = value; }
        }

        /// <inheritdoc/>
        public String Path
        {
            get { return _path; }
            set
            {
                lock (this)
                {
                    _path = value;
                    AdjustChildrenPath();
                }
            }
        }

        /// <inheritdoc/>
        public String Uri
        {
            get { return Path + Name; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the resource is visible to remote CoAP clients.
        /// </summary>
        public Boolean Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        /// <inheritdoc/>
        public virtual Boolean Cachable
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this resource is observable by remote CoAP clients.
        /// </summary>
        public Boolean Observable
        {
            get { return _observable; }
            set
            {
                _observable = value;
                Attributes.Observable = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the notifications that will be sent.
        /// </summary>
        public MessageType ObserveType
        {
            get { return _observeType; }
            set
            {
                if (value == MessageType.ACK || value == MessageType.RST)
                    throw new ArgumentException(
                        "Only CON and NON notifications are allowed or Unknown for no changes by the framework", "value");
                _observeType = value;
            }
        }

        /// <inheritdoc/>
        public ResourceAttributes Attributes
        {
            get { return _attributes; }
        }

        /// <inheritdoc/>
        public virtual IExecutor Executor
        {
            get { return _parent != null ? _parent.Executor : null; }
        }

        /// <inheritdoc/>
        public IEnumerable<IEndPoint> EndPoints
        {
            get { return _parent == null ? EmptyEndPoints : _parent.EndPoints; }
        }

        /// <inheritdoc/>
        public IResource Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != value)
                {
                    lock (this)
                    {
                        if (_parent != null)
                            _parent.Remove(this);
                        _parent = value;
                        if (value != null)
                            _path = value.Path + value.Name + "/";
                        AdjustChildrenPath();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IResource> Children
        {
            get { return _children.Values; }
        }

        /// <inheritdoc/>
        public void Add(IResource child)
        {
            if (child.Name == null)
                throw new ArgumentException("Child must have a name", "child");
            lock (this)
            {
                _children[child.Name] = child;
                child.Parent = this;
            }
        }

        public Resource Add(Resource child)
        {
            lock (this)
            {
                Add((IResource)child);
            }
            return this;
        }

        /// <inheritdoc/>
        public Boolean Remove(IResource child)
        {
            IResource toRemove;
            if (_children.TryGetValue(child.Name, out toRemove)
                && toRemove == child)
            {
                _children.Remove(child.Name);
                child.Parent = null;
                child.Path = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public IResource GetChild(String name)
        {
            IResource child;
            _children.TryGetValue(name, out child);
            return child;
        }

        public void Delete()
        {
            lock (this)
            {
                IResource parent = Parent;
                if (parent != null)
                    parent.Remove(this);
                if (Observable)
                    ClearAndNotifyObserveRelations(StatusCode.NotFound);
            }
        }

        /// <inheritdoc/>
        public void AddObserveRelation(ObserveRelation relation)
        {
            ObserveRelation old = null;
            _observeRelations.AddOrUpdate(relation.Key, relation, (k, v) =>
            {
                old = v;
                return relation;
            });

            if (old != null)
            {
                old.Cancel();
               if (log.IsEnabled(LogLevel.Debug))
                     log.LogDebug("Replacing observe relation between " + relation.Key + " and resource " + Uri);
            }
            else
            {
               if (log.IsEnabled(LogLevel.Debug))
                     log.LogDebug("Successfully established observe relation between " + relation.Key + " and resource " + Uri);
            }
        }

        /// <inheritdoc/>
        public void RemoveObserveRelation(ObserveRelation relation)
        {
            ((ICollection<KeyValuePair<String, ObserveRelation>>)_observeRelations).Remove(new KeyValuePair<String, ObserveRelation>(relation.Key, relation));
        }

        /// <summary>
        /// Cancel all observe relations to CoAP clients.
        /// </summary>
        public void ClearObserveRelations()
        {
            foreach (ObserveRelation relation in _observeRelations.Values)
            {
                relation.Cancel();
            }
        }

        /// <summary>
        /// Remove all observe relations to CoAP clients and notify them that the
        /// observe relation has been canceled.
        /// </summary>
        public void ClearAndNotifyObserveRelations(StatusCode code)
        {
            /*
             * draft-ietf-core-observe-08, chapter 3.2 Notification states:
             * In the event that the resource changes in a way that would cause
             * a normal GET request at that time to return a non-2.xx response
             * (for example, when the resource is deleted), the server sends a
             * notification with a matching response code and removes the client
             * from the list of observers.
             * This method is called, when the resource is deleted.
             */
            foreach (ObserveRelation relation in _observeRelations.Values)
            {
                relation.Cancel();
                relation.Exchange.SendResponse(new Response(code));
            }
        }

        /// <inheritdoc/>
        public virtual void HandleRequest(Exchange exchange)
        {
            CoapExchange ce = new CoapExchange(exchange, this);
            switch (exchange.Request.Method)
            {
                case Method.GET:
                    DoGet(ce);
                    break;
                case Method.POST:
                    DoPost(ce);
                    break;
                case Method.PUT:
                    DoPut(ce);
                    break;
                case Method.DELETE:
                    DoDelete(ce);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the GET request in the given CoAPExchange.
        /// By default it responds with a 4.05 (Method Not Allowed).
        /// Override this method to respond differently.
        /// Possible response codes for GET requests are 2.05 (Content) and 2.03 (Valid).
        /// </summary>
        protected virtual void DoGet(CoapExchange exchange)
        {
            exchange.Respond(StatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Handles the POST request in the given CoAPExchange.
        /// By default it responds with a 4.05 (Method Not Allowed).
        /// Override this method to respond differently.
        /// Possible response codes for POST requests are 2.01 (Created), 2.04 (Changed), and 2.02 (Deleted).
        /// </summary>
        protected virtual void DoPost(CoapExchange exchange)
        {
            exchange.Respond(StatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Handles the PUT request in the given CoAPExchange.
        /// By default it responds with a 4.05 (Method Not Allowed).
        /// Override this method to respond differently.
        /// Possible response codes for PUT requests are 2.01 (Created) and 2.04 (Changed).
        /// </summary>
        protected virtual void DoPut(CoapExchange exchange)
        {
            exchange.Respond(StatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Handles the DELETE request in the given CoAPExchange.
        /// By default it responds with a 4.05 (Method Not Allowed).
        /// Override this method to respond differently.
        /// The response code to a DELETE request should be a 2.02 (Deleted).
        /// </summary>
        protected virtual void DoDelete(CoapExchange exchange)
        {
            exchange.Respond(StatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Notifies all CoAP clients that have established an observe relation with
        /// this resource that the state has changed by reprocessing their original
        /// request that has established the relation. The notification is done by
        /// the executor of this resource or on the executor of its parent or
        /// transitively ancestor. If no ancestor defines its own executor, the
        /// thread that has called this method performs the notification.
        /// </summary>
        /// <seealso cref="Changed(Func<ObserveRelation, Boolean>)"/>
        public void Changed()
        {
            Changed(null);
        }

        /// <summary>
        /// Notifies a filtered set of CoAP clients that have established an observe
	    /// relation with this resource that the state has changed by reprocessing
	    /// their original request that has established the relation. The notification
	    /// is done by the executor of this resource or on the executor of its parent or
	    /// transitively ancestor. If no ancestor defines its own executor, the
        /// thread that has called this method performs the notification.
        /// </summary>
        /// <param name="filter">the filter to select set of relations,
        /// or <code>null</code> if all clients should be notified</param>
        public void Changed(Func<ObserveRelation, Boolean> filter)
        {
            IExecutor executor = this.Executor;
            if (executor != null)
                executor.Start(() => NotifyObserverRelations(filter));
            else
                NotifyObserverRelations(filter);
        }

        /// <summary>
        /// Notifies all CoAP clients that have established an observe relation with
	    /// this resource that the state has changed by reprocessing their original
        /// request that has established the relation.
        /// </summary>
        /// <param name="filter">the filter to select set of relations,
        /// or <code>null</code> if all clients should be notified</param>
        protected void NotifyObserverRelations(Func<ObserveRelation, Boolean> filter)
        {
            _notificationOrderer.GetNextObserveNumber();
            foreach (ObserveRelation relation in _observeRelations.Values)
            {
                if (filter == null || filter(relation))
                    relation.NotifyObservers();
            }
        }

        internal void CheckObserveRelation(Exchange exchange, Response response)
        {
            /*
             * If the request for the specified exchange tries to establish an observer
             * relation, then the ServerMessageDeliverer must have created such a relation
             * and added to the exchange. Otherwise, there is no such relation.
             * Remember that different paths might lead to this resource.
             */

            ObserveRelation relation = exchange.Relation;
            if (relation == null)
                return; // because request did not try to establish a relation

            if (Code.IsSuccess(response.Code))
            {
                response.SetOption(Option.Create(OptionType.Observe, _notificationOrderer.Current));

                if (!relation.Established)
                {
                    relation.Established = true;
                    AddObserveRelation(relation);
                }
                else if (_observeType != MessageType.Unknown)
                {
                    // The resource can control the message type of the notification
                    response.Type = _observeType;
                }
            } // ObserveLayer takes care of the else case
        }

        private void AdjustChildrenPath()
        {
            String childpath = _path + _name + "/";
            foreach (IResource child in _children.Values)
            {
                child.Path = childpath;
            }
        }
    }
}
