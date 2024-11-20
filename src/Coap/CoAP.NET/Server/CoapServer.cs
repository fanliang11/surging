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
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Server
{
    /// <summary>
    /// Represents an execution environment for CoAP <see cref="IResource"/>s.
    /// </summary>
    public class CoapServer : IServer
    {
        private readonly ILogger log ;
        readonly IResource _root;
        readonly List<IEndPoint> _endpoints = new List<IEndPoint>();
        readonly ICoapConfig _config;
        private IMessageDeliverer _deliverer;


        public CoapServer(IPEndPoint endPoint)
    : this(null, endPoint)
        { }

        /// <summary>
        /// Constructs a server with default configuration.
        /// </summary>
        public CoapServer()
            : this((ICoapConfig)null,null)
        { }

        /// <summary>
        /// Constructs a server that listens to the specified port(s).
        /// </summary>
        /// <param name="ports">the ports to bind to</param>
        public CoapServer(params Int32[] ports)
            : this(null,null, ports)
        { }

        /// <summary>
        /// Constructs a server with the specified configuration that
        /// listens to the given ports.
        /// </summary>
        /// <param name="config">the configuration, or <code>null</code> for default</param>
        /// <param name="ports">the ports to bind to</param>
        public CoapServer(ICoapConfig config, IPEndPoint iPEndPoint, params Int32[] ports)
        {
            _config = config ?? CoapConfig.Default;
            _root = new RootResource(this);
            _deliverer = new ServerMessageDeliverer(_config, _root);
            if (iPEndPoint != null)
            { 
                AddEndPoint(iPEndPoint);
            }
            Resource wellKnown = new Resource(".well-known", false);
            wellKnown.Add(new DiscoveryResource(_root));
            _root.Add(wellKnown);
            log = ServiceLocator.GetService<ILogger<CoapServer>>();
            foreach (Int32 port in ports)
            {
                Bind(port);
            }
        }

        public ICoapConfig Config
        {
            get { return _config; }
        }

        private void Bind(Int32 port)
        {
            AddEndPoint(new CoAPEndPoint(port, _config));
        }

        /// <inheritdoc/>
        public IEnumerable<IEndPoint> EndPoints
        {
            get { return _endpoints; }
        }

        public IMessageDeliverer MessageDeliverer
        {
            get { return _deliverer; }
            set
            {
                _deliverer = value;
                foreach (IEndPoint endpoint in _endpoints)
                {
                   endpoint.MessageDeliverer = value;
                }
            }
        }

        /// <inheritdoc/>
        public void AddEndPoint(IEndPoint endpoint)
        {
            endpoint.MessageDeliverer = _deliverer;
            _endpoints.Add(endpoint);
        }

        /// <inheritdoc/>
        public void AddEndPoint(IPEndPoint ep)
        {
            AddEndPoint(new CoAPEndPoint(ep, _config));
        }

        /// <inheritdoc/>
        public void AddEndPoint(IPAddress address, Int32 port)
        {
            AddEndPoint(new CoAPEndPoint(new IPEndPoint(address, port), _config));
        }

        /// <inheritdoc/>
        public IEndPoint FindEndPoint(System.Net.EndPoint ep)
        {
            foreach (IEndPoint endpoint in _endpoints)
            {
                if (endpoint.LocalEndPoint.Equals(ep))
                    return endpoint;
            }
            return null;
        }

        /// <inheritdoc/>
        public IEndPoint FindEndPoint(Int32 port)
        {
            foreach (IEndPoint endpoint in _endpoints)
            {
                if (((System.Net.IPEndPoint)endpoint.LocalEndPoint).Port == port)
                    return endpoint;
            }
            return null;
        }

        /// <inheritdoc/>
        public IServer Add(IResource resource)
        {
            _root.Add(resource);
            return this;
        }

        /// <inheritdoc/>
        public IServer Add(params IResource[] resources)
        {
            foreach (IResource resource in resources)
            {
                _root.Add(resource);
            }
            return this;
        }

        /// <inheritdoc/>
        public Boolean Remove(IResource resource)
        {
            return _root.Remove(resource);
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (log.IsEnabled(LogLevel.Debug))
                log.LogDebug("Starting CoAP server");
            
            if (_endpoints.Count == 0)
            {
                Bind(_config.DefaultPort);
            }

            Int32 started = 0;
            foreach (IEndPoint endpoint in _endpoints)
            {
                try
                {
                    endpoint.Start();
                    started++;
                }
                catch (Exception e)
                {
                    if (log.IsEnabled(LogLevel.Warning))
                        log.LogWarning("Could not start endpoint " + endpoint.LocalEndPoint, e);
                }
            }

            if (started == 0)
                throw new InvalidOperationException("None of the server's endpoints could be started");
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (log.IsEnabled(LogLevel.Warning))
                log.LogDebug("Starting CoAP server");
            _endpoints.ForEach(ep => ep.Stop());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _endpoints.ForEach(ep => ep.Dispose());
        }

        class RootResource : Resource
        {
            readonly CoapServer _server;

            public RootResource(CoapServer server)
                : base(String.Empty)
            {
                _server = server;
            }

            protected override void DoGet(CoapExchange exchange)
            {
#if COAPALL || COAP03 || COAP08 || COAP12 || COAP13 || COAP18 || RFC7252
                exchange.Respond("Ni Hao from CoAP.NET " + _server._config.Spec.Name);
#else
                exchange.Respond("Ni Hao from CoAP.NET " + Spec.Name);
#endif
            }
        }
    }
}
