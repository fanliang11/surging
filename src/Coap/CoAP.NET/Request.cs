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
using System.Net;
using System.Text.RegularExpressions; 
using CoAP.Net;
using CoAP.Observe;

namespace CoAP
{
    /// <summary>
    /// This class describes the functionality of a CoAP Request as
    /// a subclass of a CoAP Message. It provides:
    /// 1. operations to answer a request by a response using respond()
    /// 2. different ways to handle incoming responses: receiveResponse() or Responsed event
    /// </summary>
    public class Request : Message
    {
        private readonly Method _method;
        private Boolean _multicast;
        private Uri _uri;
        private Response _currentResponse;
        private IEndPoint _endPoint;
        private Object _sync;

        /// <summary>
        /// Fired when a response arrives.
        /// </summary>
        public event EventHandler<ResponseEventArgs> Respond;

        /// <summary>
        /// Occurs when a block of response arrives in a blockwise transfer.
        /// </summary>
        public event EventHandler<ResponseEventArgs> Responding;

        /// <summary>
        /// Occurs when a observing request is reregistering.
        /// </summary>
        public event EventHandler<ReregisterEventArgs> Reregistering;

        /// <summary>
        /// Initializes a request message.
        /// </summary>
        public Request(Method method)
            : this(method, true)
        { }

        /// <summary>
        /// Initializes a request message.
        /// </summary>
        /// <param name="method">The method code of the message</param>
        /// <param name="confirmable">True if the request is Confirmable</param>
        public Request(Method method, Boolean confirmable)
            : base(confirmable ? MessageType.CON : MessageType.NON, (Int32)method)
        {
            _method = method;
        }

        /// <summary>
        /// Gets the request method.
        /// </summary>
        public Method Method
        {
            get { return _method; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this request is a multicast request or not.
        /// </summary>
        public Boolean Multicast
        {
            get { return _multicast; }
            set { _multicast = value; }
        }

        static readonly Regex regIP = new Regex("(\\[[0-9a-f:]+\\]|[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3})", RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets or sets the URI of this CoAP message.
        /// </summary>
        public Uri URI
        {
            get
            {
                if (_uri == null)
                {
                    UriBuilder ub = new UriBuilder();
                    ub.Scheme = CoapConstants.UriScheme;
                    ub.Host = UriHost ?? "localhost";
                    ub.Port = UriPort<0?0:UriPort;
                    ub.Path = UriPath;
                    ub.Query = UriQuery;
                    _uri = ub.Uri;
                }
                return _uri;
            }
            set
            {
                if (null != value)
                {
                    String host = value.Host;
                    Int32 port = value.Port;

                    // set Uri-Host option if not IP literal
                    if (!String.IsNullOrEmpty(host) && !regIP.IsMatch(host)
                        && !host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                        UriHost = host;

                    if (port < 0)
                    {
                        if (String.IsNullOrEmpty(value.Scheme) ||
                            String.Equals(value.Scheme, CoapConstants.UriScheme))
                            port = CoapConstants.DefaultPort;
                        else if (String.Equals(value.Scheme, CoapConstants.SecureUriScheme))
                            port = CoapConstants.DefaultSecurePort;
                    }

                    if (UriPort != port)
                    {
                        if (port != CoapConstants.DefaultPort)
                            UriPort = port;
                        else
                            UriPort = 0;
                    }

                    Destination = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);

                    UriPath = value.AbsolutePath;
                    UriQuery = value.Query;
                }
                _uri = value;
            }
        }

        public IEndPoint EndPoint
        {
            get
            {
                if (_endPoint == null)
                    _endPoint = EndPointManager.Default;
                return _endPoint;
            }
            set { _endPoint = value; }
        }

        /// <summary>
        /// Gets or sets the response to this request.
        /// </summary>
        public Response Response
        {
            get { return _currentResponse; }
            set
            {
                _currentResponse = value;
                if (_sync != null)
                    NotifyResponse();
                FireRespond(value);
            }
        }

        public Request SetUri(String uri)
        {
            if (!uri.StartsWith("coap://") && !uri.StartsWith("coaps://"))
                uri = "coap://" + uri;
            URI = new Uri(uri);
            return this;
        }

        /// <summary>
        /// Sets CoAP's observe option. If the target resource of this request
	    /// responds with a success code and also sets the observe option, it will
        /// send more responses in the future whenever the resource's state changes.
        /// </summary>
        public Request MarkObserve()
        {
            Observe = 0;
            return this;
        }

        /// <summary>
        /// Sets CoAP's observe option to the value of 1 to proactively cancel.
        /// </summary>
        public Request MarkObserveCancel()
        {
            Observe = 1;
            return this;
        }

        /// <summary>
        /// Gets the value of a query parameter as a <code>String</code>,
        /// or <code>null</code> if the parameter does not exist.
        /// </summary>
        /// <param name="name">a <code>String</code> specifying the name of the parameter</param>
        /// <returns>a <code>String</code> representing the single value of the parameter</returns>
        public String GetParameter(String name)
        {
            foreach (Option query in GetOptions(OptionType.UriQuery))
            {
                String val = query.StringValue;
                if (String.IsNullOrEmpty(val))
                    continue;
                if (val.StartsWith(name + "="))
                    return val.Substring(name.Length + 1);
            }
            return null;
        }

        [Obsolete("Call Send() instead")]
        public void Execute()
        {
            Send();
        }

        /// <summary>
        /// Sends this message.
        /// </summary>
        public Request Send()
        {
            ValidateBeforeSending();
            EndPoint.SendRequest(this);
            return this;
        }

        /// <summary>
        /// Sends the request over the specified endpoint.
        /// </summary>
        public Request Send(IEndPoint endpoint)
        {
            ValidateBeforeSending();
            _endPoint = endpoint;
            endpoint.SendRequest(this);
            return this;
        }

        /// <summary>
        /// Wait for a response.
        /// </summary>
        /// <exception cref="System.Threading.ThreadInterruptedException"></exception>
        public Response WaitForResponse()
        {
            return WaitForResponse(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Wait for a response.
        /// </summary>
        /// <param name="millisecondsTimeout">the maximum time to wait in milliseconds</param>
        /// <returns>the response, or null if timeout occured</returns>
        /// <exception cref="System.Threading.ThreadInterruptedException"></exception>
        public Response WaitForResponse(Int32 millisecondsTimeout)
        {
            // lazy initialization of a lock
            if (_sync == null)
            {
                lock (this)
                {
                    if (_sync == null)
                    {
                        _sync = new Byte[0];
                    }
                }
            }

            lock (_sync)
            {
                if (_currentResponse == null &&
                    !IsCancelled && !IsTimedOut && !IsRejected)
                {
                    System.Threading.Monitor.Wait(_sync, millisecondsTimeout);
                }
                Response resp = _currentResponse;
                _currentResponse = null;
                return resp;
            }
        }

        /// <inheritdoc/>
        protected override void OnRejected()
        {
            if (_sync != null)
                NotifyResponse();
            base.OnRejected();
        }

        /// <inheritdoc/>
        protected override void OnTimedOut()
        {
            if (_sync != null)
                NotifyResponse();
            base.OnTimedOut();
        }

        /// <inheritdoc/>
        protected override void OnCanceled()
        {
            if (_sync != null)
                NotifyResponse();
            base.OnCanceled();
        }

        private void NotifyResponse()
        {
            lock (_sync)
            {
                System.Threading.Monitor.PulseAll(_sync);
            }
        }

        private void FireRespond(Response response)
        {
            EventHandler<ResponseEventArgs> h = Respond;
            if (h != null)
                h(this, new ResponseEventArgs(response));
        }

        internal void FireResponding(Response response)
        {
            EventHandler<ResponseEventArgs> h = Responding;
            if (h != null)
                h(this, new ResponseEventArgs(response));
        }

        internal void FireReregister(Request refresh)
        {
            EventHandler<ReregisterEventArgs> h = Reregistering;
            if (h != null)
                h(this, new ReregisterEventArgs(refresh));
        }

        private void ValidateBeforeSending()
        {
            if (Destination == null)
                throw new InvalidOperationException("Missing Destination");
        }

        internal override void CopyEventHandler(Message src)
        {
            base.CopyEventHandler(src);

            Request srcReq = src as Request;
            if (srcReq != null)
            {
                ForEach(srcReq.Respond, h => this.Respond += h);
                ForEach(srcReq.Responding, h => this.Responding += h);
            }
        }

        /// <summary>
        /// Construct a GET request.
        /// </summary>
        public static Request NewGet()
        {
            return new Request(CoAP.Method.GET);
        }

        /// <summary>
        /// Construct a POST request.
        /// </summary>
        public static Request NewPost()
        {
            return new Request(CoAP.Method.POST);
        }

        /// <summary>
        /// Construct a PUT request.
        /// </summary>
        public static Request NewPut()
        {
            return new Request(CoAP.Method.PUT);
        }

        /// <summary>
        /// Construct a DELETE request.
        /// </summary>
        public static Request NewDelete()
        {
            return new Request(CoAP.Method.DELETE);
        }
    }
}
