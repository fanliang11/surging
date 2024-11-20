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
using CoAP.Observe;
using CoAP.Stack;
using CoAP.Util;

namespace CoAP.Net
{
    /// <summary>
    /// Represents the complete state of an exchange of one request
    /// and one or more responses. The lifecycle of an exchange ends
    /// when either the last response has arrived and is acknowledged,
    /// when a request or response has been rejected from the remote endpoint,
    /// when the request has been canceled, or when a request or response timed out,
    /// i.e., has reached the retransmission limit without being acknowledged.
    /// </summary>
    public class Exchange
    {
        private readonly ConcurrentDictionary<Object, Object> _attributes = new ConcurrentDictionary<Object, Object>();
        private readonly Origin _origin;
        private Boolean _timedOut;
        private Request _request;
        private Request _currentRequest;
        private BlockwiseStatus _requestBlockStatus;
        private Response _response;
        private Response _currentResponse;
        private BlockwiseStatus _responseBlockStatus;
        private ObserveRelation _relation;
        private BlockOption _block1ToAck;
        readonly DateTime _timestamp;
        private Boolean _complete;
        private IEndPoint _endpoint;
        private IOutbox _outbox;
        private IMessageDeliverer _deliverer;

        public event EventHandler Completed;

        public Exchange(Request request, Origin origin)
        {
            _origin = origin;
            _currentRequest = request;
            _timestamp = DateTime.Now;
        }

        public Origin Origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// Gets or sets the endpoint which has created and processed this exchange.
        /// </summary>
        public IEndPoint EndPoint
        {
            get { return _endpoint; }
            set { _endpoint = value; }
        }

        public Boolean TimedOut
        {
            get { return _timedOut; }
            set
            {
                _timedOut = value;
                if (value)
                    Complete = true;
            }
        }

        public Request Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public Request CurrentRequest
        {
            get { return _currentRequest; }
            set { _currentRequest = value; }
        }

        /// <summary>
        /// Gets or sets the status of the blockwise transfer of the request,
        /// or null in case of a normal transfer,
        /// </summary>
        public BlockwiseStatus RequestBlockStatus
        {
            get { return _requestBlockStatus; }
            set { _requestBlockStatus = value; }
        }

        public Response Response
        {
            get { return _response; }
            set { _response = value; }
        }

        public Response CurrentResponse
        {
            get { return _currentResponse; }
            set { _currentResponse = value; }
        }

        /// <summary>
        /// Gets or sets the status of the blockwise transfer of the response,
        /// or null in case of a normal transfer,
        /// </summary>
        public BlockwiseStatus ResponseBlockStatus
        {
            get { return _responseBlockStatus; }
            set { _responseBlockStatus = value; }
        }

        public ObserveRelation Relation
        {
            get { return _relation; }
            set { _relation = value; }
        }

        /// <summary>
        /// Gets or sets the block option of the last block of a blockwise sent request.
        /// When the server sends the response, this block option has to be acknowledged.
        /// </summary>
        public BlockOption Block1ToAck
        {
            get { return _block1ToAck; }
            set { _block1ToAck = value; }
        }

        /// <summary>
        /// Gets the time when this exchange was created.
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public IOutbox Outbox
        {
            get { return _outbox ?? (_endpoint == null ? null : _endpoint.Outbox); }
            set { _outbox = value; }
        }

        public IMessageDeliverer Deliverer
        {
            get { return _deliverer ?? (_endpoint == null ? null : _endpoint.MessageDeliverer); }
            set { _deliverer = value; }
        }

        public Boolean Complete
        {
            get { return _complete; }
            set
            {
                _complete = value;
                if (value)
                {
                    EventHandler h = Completed;
                    if (h != null)
                        h(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Reject this exchange and therefore the request.
        /// Sends an RST back to the client.
        /// </summary>
        public virtual void SendReject()
        {
            System.Diagnostics.Debug.Assert(_origin == Origin.Remote);
            _request.IsRejected = true;
            EmptyMessage rst = EmptyMessage.NewRST(_request);
            _endpoint.SendEmptyMessage(this, rst);
        }

        /// <summary>
        /// Accept this exchange and therefore the request. Only if the request's
        /// type was a <code>CON</code> and the request has not been acknowledged
        /// yet, it sends an ACK to the client.
        /// </summary>
        public virtual void SendAccept()
        {
            System.Diagnostics.Debug.Assert(_origin == Origin.Remote);
            if (_request.Type == MessageType.CON && !_request.IsAcknowledged)
            {
                _request.IsAcknowledged = true;
                EmptyMessage ack = EmptyMessage.NewACK(_request);
                _endpoint.SendEmptyMessage(this, ack);
            }
        }

        /// <summary>
        /// Sends the specified response over the same endpoint
        /// as the request has arrived.
        /// </summary>
        public virtual void SendResponse(Response response)
        {
            response.Destination = _request.Source;
            Response = response;
            _endpoint.SendResponse(this, response);
        }

        public T Get<T>(Object key)
        {
            return (T)Get(key);
        }

        public Object Get(Object key)
        {
            Object value;
            _attributes.TryGetValue(key, out value);
            return value;
        }

        public Object GetOrAdd(Object key, Object value)
        {
            return _attributes.GetOrAdd(key, value);
        }

        public T GetOrAdd<T>(Object key, Object value)
        {
            return (T)GetOrAdd(key, value);
        }

        public Object GetOrAdd(Object key, Func<Object, Object> valueFactory)
        {
            return _attributes.GetOrAdd(key, o => valueFactory(o));
        }

        public T GetOrAdd<T>(Object key, Func<Object, Object> valueFactory)
        {
            return (T)GetOrAdd(key, o => valueFactory(o));
        }

        public Object Set(Object key, Object value)
        {
            Object old = null;
            _attributes.AddOrUpdate(key, value, (k, v) =>
            {
                old = v;
                return value;
            });
            return old;
        }

        public Object Remove(Object key)
        {
            Object obj;
            _attributes.TryRemove(key, out obj);
            return obj;
        }

        public class KeyID
        {
            private readonly Int32 _id;
            private readonly System.Net.EndPoint _endpoint;
            private readonly Int32 _hash;

            public KeyID(Int32 id, System.Net.EndPoint ep)
            {
                _id = id;
                _endpoint = ep;
                _hash = id * 31 + (ep == null ? 0 : ep.GetHashCode());
            }

            /// <inheritdoc/>
            public override Int32 GetHashCode()
            {
                return _hash;
            }

            /// <inheritdoc/>
            public override Boolean Equals(Object obj)
            {
                KeyID other = obj as KeyID;
                if (other == null)
                    return false;
                return _id == other._id && Object.Equals(_endpoint, other._endpoint);
            }

            /// <inheritdoc/>
            public override String ToString()
            {
                return "KeyID[" + _id + " for " + _endpoint + "]";
            }
        }

        public class KeyToken
        {
            private readonly Byte[] _token;
            private readonly Int32 _hash;

            public KeyToken(Byte[] token)
            {
                if (token == null)
                    throw new ArgumentNullException("token");
                _token = token;
                _hash = ByteArrayUtils.ComputeHash(_token);
            }

            /// <inheritdoc/>
            public override Int32 GetHashCode()
            {
                return _hash;
            }

            /// <inheritdoc/>
            public override Boolean Equals(Object obj)
            {
                KeyToken other = obj as KeyToken;
                if (other == null)
                    return false;
                return _hash == other._hash;
            }

            /// <inheritdoc/>
            public override String ToString()
            {
                return "KeyToken[" + BitConverter.ToString(_token) + "]";
            }
        }

        public class KeyUri
        {
            private readonly Uri _uri;
            private readonly System.Net.EndPoint _endpoint;
            private readonly Int32 _hash;

            public KeyUri(Uri uri, System.Net.EndPoint ep)
            {
                _uri = uri;
                _endpoint = ep;
                _hash = _uri.GetHashCode() * 31 + ep.GetHashCode();
            }

            /// <inheritdoc/>
            public override Int32 GetHashCode()
            {
                return _hash;
            }

            /// <inheritdoc/>
            public override Boolean Equals(Object obj)
            {
                KeyUri other = obj as KeyUri;
                if (other == null)
                    return false;
                return Object.Equals(_uri, other._uri) && Object.Equals(_endpoint, other._endpoint);
            }

            /// <inheritdoc/>
            public override String ToString()
            {
                return "KeyUri[" + _uri + " for " + _endpoint + "]";
            }
        }
    }

    /// <summary>
    /// The origin of an exchange.
    /// </summary>
    public enum Origin
    {
        Local,
        Remote
    }
}
