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
using System.Collections.Generic;
using CoAP.Net;
using CoAP.Observe;
using CoAP.Threading;

namespace CoAP.Server.Resources
{
    public interface IResource
    {
        /// <summary>
        /// Gets or sets the name of the resource.
        /// Note that changing the name of a resource changes
        /// the path and URI of all children.
        /// Note that the parent of this resource must be notified
        /// that the name has changed so that it finds the
        /// resource under the correct new URI when another request arrives.
        /// </summary>
        String Name { get; set; }
        /// <summary>
        /// Gets or sets the path to the resource which is equal to
        /// the URI of its parent plus a slash.
        /// Note that changing the path of a resource also changes
        /// the path of all its children.
        /// </summary>
        String Path { get; set; }
        /// <summary>
        /// Gets the uri of the resource.
        /// </summary>
        String Uri { get; }
        /// <summary>
        /// Checks if the resource is visible to remote CoAP clients.
        /// </summary>
        Boolean Visible { get; }
        /// <summary>
        /// Checks if is the URI of the resource can be cached.
        /// If another request with the same destination URI arrives,
        /// it can be forwarded to this resource right away instead of
        /// traveling through the resource tree looking for it.
        /// </summary>
        Boolean Cachable { get; }
        /// <summary>
        /// Checks if this resource is observable by remote CoAP clients.
        /// </summary>
        Boolean Observable { get; }
        /// <summary>
        /// Gets the attributes of this resource.
        /// </summary>
        ResourceAttributes Attributes { get; }
        /// <summary>
        /// Gets the executor of this resource.
        /// </summary>
        IExecutor Executor { get; }
        /// <summary>
        /// Gets the endpoints this resource is bound to.
        /// </summary>
        IEnumerable<IEndPoint> EndPoints { get; }
        /// <summary>
        /// Gets or sets the parent of this resource.
        /// </summary>
        IResource Parent { get; set; }
        /// <summary>
        /// Gets all child resources.
        /// </summary>
        IEnumerable<IResource> Children { get; }
        /// <summary>
        /// Adds the specified resource as child.
        /// </summary>
        void Add(IResource child);
        /// <summary>
        /// Removes the the specified child.
        /// </summary>
        /// <param name="child"></param>
        /// <returns>true if the child was found, otherwise false</returns>
        Boolean Remove(IResource child);
        /// <summary>
        /// Gets the child with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IResource GetChild(String name);
        /// <summary>
        /// Adds the specified CoAP observe relation.
        /// </summary>
        void AddObserveRelation(ObserveRelation relation);
        /// <summary>
        /// Removes the specified CoAP observe relation.
        /// </summary>
        void RemoveObserveRelation(ObserveRelation relation);
        /// <summary>
        /// Handles the request from the specified exchange.
        /// </summary>
        /// <param name="exchange">the exchange with the request</param>
        void HandleRequest(Exchange exchange);
    }
}
