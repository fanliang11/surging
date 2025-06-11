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
using System.Net;
using CoAP.Net;
using CoAP.Server.Resources;

namespace CoAP.Server
{
    /// <summary>
    /// Represents an execution environment for CoAP <see cref="IResource"/>s.
    /// 
    /// A server hosts a tree of <see cref="IResource"/>s which are exposed to clients by
    /// means of one or more <see cref="IEndPoint"/>s which are bound to a network interface.
    /// Resources can be added and removed from the server dynamically during runtime.
    /// </summary>
    public interface IServer : IDisposable
    {
        IEnumerable<IEndPoint> EndPoints { get; }
        /// <summary>
        /// Adds an endpoint for receive and sending CoAP messages on.
        /// </summary>
        void AddEndPoint(IEndPoint endpoint);
        /// <summary>
        /// Adds an <see cref="IPEndPoint"/> for receive and sending CoAP messages on.
        /// </summary>
        void AddEndPoint(IPEndPoint ep);
        /// <summary>
        /// Adds an <see cref="IPAddress"/> and a port for receive and sending CoAP messages on.
        /// </summary>
        void AddEndPoint(IPAddress address, Int32 port);
        /// <summary>
        /// Finds the endpoint bound to a particular <see cref="System.Net.EndPoint"/>.
        /// </summary>
        /// <returns>the endpoint or <code>null</code> if none of the server's
        /// endpoints is bound to the given <see cref="System.Net.EndPoint"/></returns>
        IEndPoint FindEndPoint(System.Net.EndPoint ep);
        /// <summary>
        /// Finds the endpoint bound to a particular port.
        /// </summary>
        /// <returns>the endpoint or <code>null</code> if none of the
        /// server's endpoints is bound to the given port</returns>
        IEndPoint FindEndPoint(Int32 port);
        /// <summary>
        /// Add one resource to the server.
        /// </summary>
        IServer Add(IResource resource);
        /// <summary>
        /// Adds one or more resources to the server.
        /// </summary>
        IServer Add(params IResource[] resources);
        /// <summary>
        /// Removes a resource from the server.
        /// </summary>
        /// <returns><code>true</code> if the resource has been removed successfully</returns>
        Boolean Remove(IResource resource);
        /// <summary>
        /// Starts the server by starting all endpoints this server is assigned to.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops the server.
        /// </summary>
        void Stop();
    }
}
