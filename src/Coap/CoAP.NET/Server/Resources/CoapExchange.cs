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

namespace CoAP.Server.Resources
{
    /// <summary>
    /// Represents an exchange of a CoAP request and response and
    /// provides a user-friendly API to subclasses of <see cref="Resource"/>
    /// for responding to requests.
    /// </summary>
    public class CoapExchange
    {
        readonly Exchange _exchange;
        readonly Resource _resource;

        private String _locationPath;
        private String _locationQuery;
        private Int32 _maxAge = 60;
        private Byte[] _eTag;

        /// <summary>
        /// Constructs a new CoAP Exchange object representing
        /// the specified exchange and resource.
        /// </summary>
        internal CoapExchange(Exchange exchange, Resource resource)
        {
            _exchange = exchange;
            _resource = resource;
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        public Request Request
        {
            get { return _exchange.Request; }
        }

        /// <summary>
        /// Gets or sets the Location-Path for the response.
        /// </summary>
        public String LocationPath
        {
            get { return _locationPath; }
            set { _locationPath = value; }
        }

        /// <summary>
        /// Gets or sets the Location-Query for the response.
        /// </summary>
        public String LocationQuery
        {
            get { return _locationQuery; }
            set { _locationQuery = value; }
        }

        /// <summary>
        /// Gets or sets the Max-Age for the response body.
        /// </summary>
        public Int32 MaxAge
        {
            get { return _maxAge; }
            set { _maxAge = value; }
        }

        /// <summary>
        /// Gets or sets the ETag for the response.
        /// </summary>
        public Byte[] ETag
        {
            get { return _eTag; }
            set { _eTag = value; }
        }

        /// <summary>
        /// Accepts the exchange.
        /// </summary>
        public void Accept()
        {
            _exchange.SendAccept();
        }

        /// <summary>
        /// Rejects the exchange.
        /// </summary>
        public void Reject()
        {
            _exchange.SendReject();
        }

        /// <summary>
        /// Responds the specified response code and no payload.
        /// </summary>
        public void Respond(StatusCode code)
        {
            Respond(new Response(code));
        }

        /// <summary>
        /// Responds with code 2.05 (Content) and the specified payload.
        /// </summary>
        public void Respond(String payload)
        {
            Respond(StatusCode.Content, payload);
        }

        /// <summary>
        /// Responds with the specified response code and payload.
        /// </summary>
        public void Respond(StatusCode code, String payload)
        {
            Response response = new Response(code);
            response.SetPayload(payload, MediaType.TextPlain);
            Respond(response);
        }

        /// <summary>
        /// Responds with the specified response code and payload.
        /// </summary>
        public void Respond(StatusCode code, Byte[] payload)
        {
            Response response = new Response(code);
            response.Payload = payload;
            Respond(response);
        }

        /// <summary>
        /// Responds with the specified response code, payload and content-type.
        /// </summary>
        public void Respond(StatusCode code, Byte[] payload, Int32 contentType)
        {
            Response response = new Response(code);
            response.Payload = payload;
            response.ContentType = contentType;
            Respond(response);
        }

        /// <summary>
        /// Responds with the specified response code, payload and content-type.
        /// </summary>
        public void Respond(StatusCode code, String payload, Int32 contentType)
        {
            Response response = new Response(code);
            response.SetPayload(payload, contentType);
            Respond(response);
        }

        /// <summary>
        /// Responds Respond with the specified response.
        /// </summary>
        public void Respond(Response response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            // set the response options configured through the CoapExchange API
            if (_locationPath != null)
                response.LocationPath = _locationPath;
            if (_locationQuery != null)
                response.LocationQuery = _locationQuery;
            if (_maxAge != 60)
                response.MaxAge = _maxAge;
            if (_eTag != null)
                response.SetOption(Option.Create(OptionType.ETag, _eTag));

            _resource.CheckObserveRelation(_exchange, response);

            _exchange.SendResponse(response);
        }
    }
}
