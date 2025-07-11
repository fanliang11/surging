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
using CoAP.Observe;

namespace CoAP
{
    /// <summary>
    /// Represents a CoAP observe relation between a CoAP client and a resource on a server.
    /// Provides a simple API to check whether a relation has successfully established and
    /// to cancel or refresh the relation.
    /// </summary>
    public class CoapObserveRelation
    {
        readonly ICoapConfig _config;
        readonly Request _request;
        readonly IEndPoint _endpoint;
        private Boolean _canceled;
        private Response _current;
        private ObserveNotificationOrderer _orderer;

        public CoapObserveRelation(Request request, IEndPoint endpoint, ICoapConfig config)
        {
            _config = config;
            _request = request;
            _endpoint = endpoint;
            _orderer = new ObserveNotificationOrderer(config);

            request.Reregistering += OnReregister;
        }

        public Request Request
        {
            get { return _request; }
        }

        public Response Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public ObserveNotificationOrderer Orderer
        {
            get { return _orderer; }
        }

        public Boolean Canceled
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public void ReactiveCancel()
        {
            _request.IsCancelled = true;
            _canceled = true;
        }

        public void ProactiveCancel()
        {
            Request cancel = Request.NewGet();
            // copy options, but set Observe to cancel
            cancel.SetOptions(_request.GetOptions());
            cancel.MarkObserveCancel();
            // use same Token
            cancel.Token = _request.Token;
            cancel.Destination = _request.Destination;

            // dispatch final response to the same message observers
            cancel.CopyEventHandler(_request);

            cancel.Send(_endpoint);
            // cancel old ongoing request
            _request.IsCancelled = true;
            _canceled = true;
        }

        private void OnReregister(Object sender, ReregisterEventArgs e)
        {
            // TODO: update request in observe handle for correct cancellation?
            //_request = e.RefreshRequest;

            // reset orderer to accept any sequence number since server might have rebooted
            _orderer = new ObserveNotificationOrderer(_config);
        }
    }
}
