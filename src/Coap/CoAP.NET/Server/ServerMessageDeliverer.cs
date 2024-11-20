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
using CoAP.Net;
using CoAP.Observe;
using CoAP.Server.Resources;
using CoAP.Util;
using CoAP.Threading;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Server
{
    /// <summary>
    /// Delivers requests to corresponding resources and
    /// responses to corresponding requests.
    /// </summary>
    public class ServerMessageDeliverer : IMessageDeliverer
    {
        private readonly ILogger log;
        readonly ICoapConfig _config;
        readonly IResource _root;
        readonly ObserveManager _observeManager = new ObserveManager();

        /// <summary>
        /// Constructs a default message deliverer that delivers requests
        /// to the resources rooted at the specified root.
        /// </summary>
        public ServerMessageDeliverer(ICoapConfig config, IResource root)
        {
            log=ServiceLocator.GetService<ILogger<CoapServer>>();
            _config = config;
            _root = root;
        }

        /// <inheritdoc/>
        public void DeliverRequest(Exchange exchange)
        {
            Request request = exchange.Request;
            IResource resource = FindResource(request.UriPaths);
            if (resource != null)
            {
                CheckForObserveOption(exchange, resource);

                // Get the executor and let it process the request
                IExecutor executor = resource.Executor;
                if (executor != null)
                    executor.Start(() => resource.HandleRequest(exchange));
                else
                    resource.HandleRequest(exchange);
            }
            else
            {
                exchange.SendResponse(new Response(StatusCode.NotFound));
            }
        }

        /// <inheritdoc/>
        public void DeliverResponse(Exchange exchange, Response response)
        {
            if (exchange == null)
                throw ThrowHelper.ArgumentNull("exchange");
            if (response == null)
                throw ThrowHelper.ArgumentNull("response");
            if (exchange.Request == null)
                throw ThrowHelper.Argument("exchange", "Request should not be empty.");
            exchange.Request.Response = response;
        }

        private IResource FindResource(IEnumerable<String> paths)
        {
            IResource current = _root;
            using (IEnumerator<String> ie = paths.GetEnumerator())
            {
                while (ie.MoveNext() && current != null)
                {
                    current = current.GetChild(ie.Current);
                }
            }
            return current;
        }

        private void CheckForObserveOption(Exchange exchange, IResource resource)
        {
            Request request = exchange.Request;
            if (request.Method != Method.GET)
                return;

            System.Net.EndPoint source = request.Source;
            Int32? obs = request.Observe;
            if (obs.HasValue && resource.Observable)
            {
                if (obs == 0)
                {
                    // Requests wants to observe and resource allows it :-)
                   if (log.IsEnabled(LogLevel.Debug))
                         log.LogDebug("Initiate an observe relation between " + source + " and resource " + resource.Uri);
                    ObservingEndpoint remote = _observeManager.FindObservingEndpoint(source);
                    ObserveRelation relation = new ObserveRelation(_config, remote, resource, exchange);
                    remote.AddObserveRelation(relation);
                    exchange.Relation = relation;
                    // all that's left is to add the relation to the resource which
                    // the resource must do itself if the response is successful
                }
                else if (obs == 1)
                {
                    ObserveRelation relation = _observeManager.GetRelation(source, request.Token);
                    if (relation != null)
                        relation.Cancel();
                }
            }
        }
    }
}
