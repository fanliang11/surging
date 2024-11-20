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
 
using CoAP.Net;
using CoAP.Observe;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP
{
    /// <summary>
    /// Provides convenient methods for accessing CoAP resources.
    /// </summary>
    public class CoapClient
    {
        private readonly ILogger log;
        private static readonly IEnumerable<WebLink> EmptyLinks = new WebLink[0];
        private Uri _uri;
        private ICoapConfig _config;
        private IEndPoint _endpoint;
        private MessageType _type = MessageType.CON;
        private Int32 _blockwise;
        private Int32 _timeout = System.Threading.Timeout.Infinite;

        /// <summary>
        /// Occurs when a response has arrived.
        /// </summary>
        public event EventHandler<ResponseEventArgs> Respond;
        /// <summary>
        /// Occurs if an exception is thrown while executing a request.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Instantiates with default config.
        /// </summary>
        public CoapClient()
            : this(null, null)
        { }

        /// <summary>
        /// Instantiates with default config.
        /// </summary>
        /// <param name="uri">the Uri of remote resource</param>
        public CoapClient(Uri uri)
            : this(uri, null)
        { }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="config">the config</param>
        public CoapClient(ICoapConfig config)
            : this(null, config)
        { }

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="uri">the Uri of remote resource</param>
        /// <param name="config">the config</param>
        public CoapClient(Uri uri, ICoapConfig config)
        {
            log = ServiceLocator.GetService<ILogger<CoapClient>>();
            _uri = uri;
            _config = config ?? CoapConfig.Default;
        }

        /// <summary>
        /// Gets or sets the destination URI of this client.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        /// <summary>
        /// Gets or sets the endpoint this client is supposed to use.
        /// </summary>
        public IEndPoint EndPoint
        {
            get { return _endpoint; }
            set { _endpoint = value; }
        }

        /// <summary>
        /// Gets or sets the timeout how long synchronous method calls will wait
        /// until they give up and return anyways. The default value is <see cref="System.Threading.Timeout.Infinite"/>.
        /// </summary>
        public Int32 Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// Let the client use Confirmable requests.
        /// </summary>
        public CoapClient UseCONs()
        {
            _type = MessageType.CON;
            return this;
        }

        /// <summary>
        /// Let the client use Non-Confirmable requests.
        /// </summary>
        public CoapClient UseNONs()
        {
            _type = MessageType.NON;
            return this;
        }

        /// <summary>
        /// Let the client use early negotiation for the blocksize
        /// (16, 32, 64, 128, 256, 512, or 1024). Other values will
        /// be matched to the closest logarithm dualis.
        /// </summary>
        public CoapClient UseEarlyNegotiation(Int32 size)
        {
            _blockwise = size;
            return this;
        }

        /// <summary>
        /// Let the client use late negotiation for the block size (default).
        /// </summary>
        public CoapClient UseLateNegotiation()
        {
            _blockwise = 0;
            return this;
        }

        /// <summary>
        /// Performs a CoAP ping.
        /// </summary>
        /// <returns>success of the ping</returns>
        public Boolean Ping()
        {
            return Ping(_timeout);
        }

        /// <summary>
        /// Performs a CoAP ping and gives up after the given number of milliseconds.
        /// </summary>
        /// <param name="timeout">the time to wait for a pong in milliseconds</param>
        /// <returns>success of the ping</returns>
        public Boolean Ping(Int32 timeout)
        {
            try
            {
                Request request = new Request(Code.Empty, true);
                request.Token = CoapConstants.EmptyToken;
                request.URI = Uri;
                request.Send().WaitForResponse(timeout);
                return request.IsRejected;
            }
            catch (System.Threading.ThreadInterruptedException)
            {
                /* ignore */
            }
            return false;
        }

        /// <summary>
        /// Discovers remote resources.
        /// </summary>
        /// <returns>the descoverd <see cref="WebLink"/> representing remote resources, or null if no response</returns>
        public IEnumerable<WebLink> Discover()
        {
            return Discover(null);
        }

        /// <summary>
        /// Discovers remote resources.
        /// </summary>
        /// <param name="query">the query to filter resources</param>
        /// <returns>the descoverd <see cref="WebLink"/> representing remote resources, or null if no response</returns>
        public IEnumerable<WebLink> Discover(String query)
        {
            Request discover = Prepare(Request.NewGet());
            discover.ClearUriPath().ClearUriQuery().UriPath = CoapConstants.DefaultWellKnownURI;
            if (!String.IsNullOrEmpty(query))
                discover.UriQuery = query;
            Response links = discover.Send().WaitForResponse(_timeout);
            if (links == null)
                // if no response, return null (e.g., timeout)
                return null;
            else if (links.ContentFormat != MediaType.ApplicationLinkFormat)
                return EmptyLinks;
            else
                return LinkFormat.Parse(links.PayloadString);
        }

        /// <summary>
        /// Sends a GET request and blocks until the response is available.
        /// </summary>
        /// <returns>the CoAP response</returns>
        public Response Get()
        {
            return Send(Request.NewGet());
        }

        /// <summary>
        /// Sends a GET request with the specified Accept option and blocks
        /// until the response is available.
        /// </summary>
        /// <param name="accept">the Accept option</param>
        /// <returns>the CoAP response</returns>
        public Response Get(Int32 accept)
        {
            return Send(Accept(Request.NewGet(), accept));
        }

        /// <summary>
        /// Sends a GET request asynchronizely.
        /// </summary>
        /// <param name="done">the callback when a response arrives</param>
        /// <param name="fail">the callback when an error occurs</param>
        public void GetAsync(Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync(Request.NewGet(), done, fail);
        }

        /// <summary>
        /// Sends a GET request with the specified Accept option asynchronizely.
        /// </summary>
        /// <param name="accept">the Accept option</param>
        /// <param name="done">the callback when a response arrives</param>
        /// <param name="fail">the callback when an error occurs</param>
        public void GetAsync(Int32 accept, Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync(Accept(Request.NewGet(), accept), done, fail);
        }

        public Response Post(String payload, Int32 format = MediaType.TextPlain)
        {
            return Send((Request)Request.NewPost().SetPayload(payload, format));
        }

        public Response Post(String payload, Int32 format, Int32 accept)
        {
            return Send(Accept((Request)Request.NewPost().SetPayload(payload, format), accept));
        }

        public Response Post(Byte[] payload, Int32 format)
        {
            return Send((Request)Request.NewPost().SetPayload(payload, format));
        }

        public Response Post(Byte[] payload, Int32 format, Int32 accept)
        {
            return Send(Accept((Request)Request.NewPost().SetPayload(payload, format), accept));
        }

        public void PostAsync(String payload,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            PostAsync(payload, MediaType.TextPlain, done, fail);
        }

        public void PostAsync(String payload, Int32 format,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync((Request)Request.NewPost().SetPayload(payload, format), done, fail);
        }

        public void PostAsync(String payload, Int32 format, Int32 accept,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync(Accept((Request)Request.NewPost().SetPayload(payload, format), accept), done, fail);
        }

        public void PostAsync(Byte[] payload, Int32 format,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync((Request)Request.NewPost().SetPayload(payload, format), done, fail);
        }

        public void PostAsync(Byte[] payload, Int32 format, Int32 accept,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync(Accept((Request)Request.NewPost().SetPayload(payload, format), accept), done, fail);
        }

        public Response Put(String payload, Int32 format = MediaType.TextPlain)
        {
            return Send((Request)Request.NewPut().SetPayload(payload, format));
        }

        public Response Put(Byte[] payload, Int32 format, Int32 accept)
        {
            return Send(Accept((Request)Request.NewPut().SetPayload(payload, format), accept));
        }

        public Response PutIfMatch(String payload, Int32 format, params Byte[][] etags)
        {
            return Send(IfMatch((Request)Request.NewPut().SetPayload(payload, format), etags));
        }

        public Response PutIfMatch(Byte[] payload, Int32 format, params Byte[][] etags)
        {
            return Send(IfMatch((Request)Request.NewPut().SetPayload(payload, format), etags));
        }

        public Response PutIfNoneMatch(String payload, Int32 format)
        {
            return Send(IfNoneMatch((Request)Request.NewPut().SetPayload(payload, format)));
        }

        public Response PutIfNoneMatch(Byte[] payload, Int32 format)
        {
            return Send(IfNoneMatch((Request)Request.NewPut().SetPayload(payload, format)));
        }

        public void PutAsync(String payload,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            PutAsync(payload, MediaType.TextPlain, done, fail);
        }

        public void PutAsync(String payload, Int32 format,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync((Request)Request.NewPut().SetPayload(payload, format), done, fail);
        }

        public void PutAsync(Byte[] payload, Int32 format, Int32 accept,
            Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync(Accept((Request)Request.NewPut().SetPayload(payload, format), accept), done, fail);
        }

        /// <summary>
        /// Sends a DELETE request and waits for the response.
        /// </summary>
        /// <returns>the CoAP response</returns>
        public Response Delete()
        {
            return Send(Request.NewDelete());
        }

        /// <summary>
        /// Sends a DELETE request asynchronizely.
        /// </summary>
        /// <param name="done">the callback when a response arrives</param>
        /// <param name="fail">the callback when an error occurs</param>
        public void DeleteAsync(Action<Response> done = null, Action<FailReason> fail = null)
        {
            SendAsync(Request.NewDelete(), done, fail);
        }

        public Response Validate(params Byte[][] etags)
        {
            return Send(ETags(Request.NewGet(), etags));
        }

        public CoapObserveRelation Observe(Action<Response> notify = null, Action<FailReason> error = null)
        {
            return Observe(Request.NewGet().MarkObserve(), notify, error);
        }

        public CoapObserveRelation Observe(Int32 accept, Action<Response> notify = null, Action<FailReason> error = null)
        {
            return Observe(Accept(Request.NewGet().MarkObserve(), accept), notify, error);
        }

        public CoapObserveRelation ObserveAsync(Action<Response> notify = null, Action<FailReason> error = null)
        {
            return ObserveAsync(Request.NewGet().MarkObserve(), notify, error);
        }

        public CoapObserveRelation ObserveAsync(Int32 accept, Action<Response> notify = null, Action<FailReason> error = null)
        {
            return ObserveAsync(Accept(Request.NewGet().MarkObserve(), accept), notify, error);
        }

        public Response Send(Request request)
        {
            return Prepare(request).Send().WaitForResponse(_timeout);
        }

        public void SendAsync(Request request, Action<Response> done = null, Action<FailReason> fail = null)
        {
            request.Respond += (o, e) => Deliver(done, e);
            request.Rejected += (o, e) => Fail(fail, FailReason.Rejected);
            request.TimedOut += (o, e) => Fail(fail, FailReason.TimedOut);
            
            Prepare(request).Send();
        }

        protected Request Prepare(Request request)
        {
            return Prepare(request, GetEffectiveEndpoint(request));
        }

        protected Request Prepare(Request request, IEndPoint endpoint)
        {
            request.Type = _type;
            request.URI = _uri;
            
            if (_blockwise != 0)
                request.SetBlock2(BlockOption.EncodeSZX(_blockwise), false, 0);

            if (endpoint != null)
                request.EndPoint = endpoint;

            return request;
        }

        /// <summary>
        /// Gets the effective endpoint that the specified request
        /// is supposed to be sent over.
        /// </summary>
        protected IEndPoint GetEffectiveEndpoint(Request request)
        {
            if (_endpoint != null)
                return _endpoint;
            else
                return EndPointManager.Default;
            // TODO secure coap
        }

        private CoapObserveRelation Observe(Request request, Action<Response> notify, Action<FailReason> error)
        {
            CoapObserveRelation relation = ObserveAsync(request, notify, error);
            Response response = relation.Request.WaitForResponse(_timeout);
            if (response == null || !response.HasOption(OptionType.Observe))
                relation.Canceled = true;
            relation.Current = response;
            return relation;
        }

        private CoapObserveRelation ObserveAsync(Request request, Action<Response> notify, Action<FailReason> error)
        {
            IEndPoint endpoint = GetEffectiveEndpoint(request);
            CoapObserveRelation relation = new CoapObserveRelation(request, endpoint, _config);

            request.Respond += (o, e) =>
            {
                Response resp = e.Response;
                lock (relation)
                {
                    if (relation.Orderer.IsNew(resp))
                    {
                        relation.Current = resp;
                        Deliver(notify, e);
                    }
                    else
                    {
                         log.LogDebug("Dropping old notification: " + resp);
                    }
                }
            };
            Action<FailReason> fail = r =>
            {
                relation.Canceled = true;
                Fail(error, r);
            };
            request.Rejected += (o, e) => fail(FailReason.Rejected);
            request.TimedOut += (o, e) => fail(FailReason.TimedOut);

            Prepare(request, endpoint).Send();
            return relation;
        }

        private void Deliver(Action<Response> act, ResponseEventArgs e)
        {
            if (act != null)
                act(e.Response);
            EventHandler<ResponseEventArgs> h = Respond;
            if (h != null)
                h(this, e);
        }

        private void Fail(Action<FailReason> fail, FailReason reason)
        {
            if (fail != null)
                fail(reason);
            EventHandler<ErrorEventArgs> h = Error;
            if (h != null)
                h(this, new ErrorEventArgs(reason));
        }

        static Request Accept(Request request, Int32 accept)
        {
            request.Accept = accept;
            return request;
        }

        static Request IfMatch(Request request, params Byte[][] etags)
        {
            foreach (Byte[] etag in etags)
            {
                request.AddIfMatch(etag);
            }
            return request;
        }

        static Request IfNoneMatch(Request request)
        {
            request.IfNoneMatch = true;
            return request;
        }

        static Request ETags(Request request, params Byte[][] etags)
        {
            foreach (Byte[] etag in etags)
            {
                request.AddETag(etag);
            }
            return request;
        }

        /// <summary>
        /// Provides details about errors.
        /// </summary>
        public enum FailReason
        {
            /// <summary>
            /// The request has been rejected.
            /// </summary>
            Rejected,
            /// <summary>
            /// The request has been timed out.
            /// </summary>
            TimedOut
        }

        /// <summary>
        /// Provides event args for errors.
        /// </summary>
        public class ErrorEventArgs : EventArgs
        {
            internal ErrorEventArgs(FailReason reason)
            {
                this.Reason = reason;
            }

            /// <summary>
            /// Gets the reason why failed.
            /// </summary>
            public FailReason Reason { get; private set; }
        }
    }
}
