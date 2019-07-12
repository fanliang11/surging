/*
 * WebSocketBehavior.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2016 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;
using WebSocketCore.Net;
using WebSocketCore.Net.WebSockets;

namespace WebSocketCore.Server
{
    /// <summary>
    /// Exposes a set of methods and properties used to define the behavior of
    /// a WebSocket service provided by the <see cref="WebSocketServer"/> or
    /// <see cref="HttpServer"/>.
    /// </summary>
    public abstract class WebSocketBehavior : IWebSocketSession
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private WebSocketContext _context;

        /// <summary>
        /// Defines the _cookiesValidator
        /// </summary>
        private Func<CookieCollection, CookieCollection, bool> _cookiesValidator;

        /// <summary>
        /// Defines the _emitOnPing
        /// </summary>
        private bool _emitOnPing;

        /// <summary>
        /// Defines the _id
        /// </summary>
        private string _id;

        /// <summary>
        /// Defines the _ignoreExtensions
        /// </summary>
        private bool _ignoreExtensions;

        /// <summary>
        /// Defines the _originValidator
        /// </summary>
        private Func<string, bool> _originValidator;

        /// <summary>
        /// Defines the _protocol
        /// </summary>
        private string _protocol;

        /// <summary>
        /// Defines the _sessions
        /// </summary>
        private WebSocketSessionManager _sessions;

        /// <summary>
        /// Defines the _startTime
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Defines the _websocket
        /// </summary>
        private WebSocket _websocket;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketBehavior"/> class.
        /// </summary>
        protected WebSocketBehavior()
        {
            _startTime = DateTime.MaxValue;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the current state of the WebSocket connection for a session.
        /// </summary>
        public WebSocketState ConnectionState
        {
            get
            {
                return _websocket != null
                       ? _websocket.ReadyState
                       : WebSocketState.Connecting;
            }
        }

        /// <summary>
        /// Gets the information in a WebSocket handshake request to the service.
        /// </summary>
        public WebSocketContext Context
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// Gets or sets the delegate used to validate the HTTP cookies included in
        /// a WebSocket handshake request to the service.
        /// </summary>
        public Func<CookieCollection, CookieCollection, bool> CookiesValidator
        {
            get
            {
                return _cookiesValidator;
            }

            set
            {
                _cookiesValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the WebSocket instance for
        /// a session emits the message event when receives a ping.
        /// </summary>
        public bool EmitOnPing
        {
            get
            {
                return _websocket != null ? _websocket.EmitOnPing : _emitOnPing;
            }

            set
            {
                if (_websocket != null)
                {
                    _websocket.EmitOnPing = value;
                    return;
                }

                _emitOnPing = value;
            }
        }

        /// <summary>
        /// Gets the unique ID of a session.
        /// </summary>
        public string ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the service ignores
        /// the Sec-WebSocket-Extensions header included in a WebSocket
        /// handshake request.
        /// </summary>
        public bool IgnoreExtensions
        {
            get
            {
                return _ignoreExtensions;
            }

            set
            {
                _ignoreExtensions = value;
            }
        }

        /// <summary>
        /// Gets or sets the delegate used to validate the Origin header included in
        /// a WebSocket handshake request to the service.
        /// </summary>
        public Func<string, bool> OriginValidator
        {
            get
            {
                return _originValidator;
            }

            set
            {
                _originValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the WebSocket subprotocol for the service.
        /// </summary>
        public string Protocol
        {
            get
            {
                return _websocket != null
                       ? _websocket.Protocol
                       : (_protocol ?? String.Empty);
            }

            set
            {
                if (ConnectionState != WebSocketState.Connecting)
                {
                    var msg = "The session has already started.";
                    throw new InvalidOperationException(msg);
                }

                if (value == null || value.Length == 0)
                {
                    _protocol = null;
                    return;
                }

                if (!value.IsToken())
                    throw new ArgumentException("Not a token.", "value");

                _protocol = value;
            }
        }

        /// <summary>
        /// Gets the time that a session has started.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

        /// <summary>
        /// Gets the logging function.
        /// </summary>
        [Obsolete("This property will be removed.")]
        protected Logger Log
        {
            get
            {
                return _websocket != null ? _websocket.Log : null;
            }
        }

        /// <summary>
        /// Gets the management function for the sessions in the service.
        /// </summary>
        protected WebSocketSessionManager Sessions
        {
            get
            {
                return _sessions;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Start
        /// </summary>
        /// <param name="context">The context<see cref="WebSocketContext"/></param>
        /// <param name="sessions">The sessions<see cref="WebSocketSessionManager"/></param>
        internal void Start(WebSocketContext context, WebSocketSessionManager sessions)
        {
            if (_websocket != null)
            {
                _websocket.Log.Error("A session instance cannot be reused.");
                context.WebSocket.Close(HttpStatusCode.ServiceUnavailable);

                return;
            }

            _context = context;
            _sessions = sessions;

            _websocket = context.WebSocket;
            _websocket.CustomHandshakeRequestChecker = checkHandshakeRequest;
            _websocket.EmitOnPing = _emitOnPing;
            _websocket.IgnoreExtensions = _ignoreExtensions;
            _websocket.Protocol = _protocol;

            var waitTime = sessions.WaitTime;
            if (waitTime != _websocket.WaitTime)
                _websocket.WaitTime = waitTime;

            _websocket.OnOpen += onOpen;
            _websocket.OnMessage += onMessage;
            _websocket.OnError += onError;
            _websocket.OnClose += onClose;
            _websocket.InternalAccept();
        }

        /// <summary>
        /// Closes the WebSocket connection for a session.
        /// </summary>
        protected void Close()
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Close();
        }

        /// <summary>
        /// Calls the <see cref="OnError"/> method with the specified message.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        [Obsolete("This method will be removed.")]
        protected void Error(string message, Exception exception)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (message.Length == 0)
                throw new ArgumentException("An empty string.", "message");

            OnError(new ErrorEventArgs(message, exception));
        }

        /// <summary>
        /// Called when the WebSocket connection for a session has been closed.
        /// </summary>
        /// <param name="e">The e<see cref="CloseEventArgs"/></param>
        protected virtual void OnClose(CloseEventArgs e)
        {
        }

        /// <summary>
        /// Called when the WebSocket instance for a session gets an error.
        /// </summary>
        /// <param name="e">The e<see cref="ErrorEventArgs"/></param>
        protected virtual void OnError(ErrorEventArgs e)
        {
        }

        /// <summary>
        /// Called when the WebSocket instance for a session receives a message.
        /// </summary>
        /// <param name="e">The e<see cref="MessageEventArgs"/></param>
        protected virtual void OnMessage(MessageEventArgs e)
        {
        }

        /// <summary>
        /// Called when the WebSocket connection for a session has been established.
        /// </summary>
        protected virtual void OnOpen()
        {
        }

        /// <summary>
        /// Sends the specified data to a client using the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        protected void Send(byte[] data)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(data);
        }

        /// <summary>
        /// Sends the specified file to a client using the WebSocket connection.
        /// </summary>
        /// <param name="fileInfo">The fileInfo<see cref="FileInfo"/></param>
        protected void Send(FileInfo fileInfo)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(fileInfo);
        }

        /// <summary>
        /// Sends the data from the specified stream to a client using
        /// the WebSocket connection.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        protected void Send(Stream stream, int length)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(stream, length);
        }

        /// <summary>
        /// Sends the specified data to a client using the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="string"/></param>
        protected void Send(string data)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(data);
        }

        /// <summary>
        /// Sends the specified data to a client asynchronously using
        /// the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        protected void SendAsync(byte[] data, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(data, completed);
        }

        /// <summary>
        /// Sends the specified file to a client asynchronously using
        /// the WebSocket connection.
        /// </summary>
        /// <param name="fileInfo">The fileInfo<see cref="FileInfo"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        protected void SendAsync(FileInfo fileInfo, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(fileInfo, completed);
        }

        /// <summary>
        /// Sends the data from the specified stream to a client asynchronously
        /// using the WebSocket connection.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        protected void SendAsync(Stream stream, int length, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(stream, length, completed);
        }

        /// <summary>
        /// Sends the specified data to a client asynchronously using
        /// the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="string"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        protected void SendAsync(string data, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(data, completed);
        }

        /// <summary>
        /// The checkHandshakeRequest
        /// </summary>
        /// <param name="context">The context<see cref="WebSocketContext"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string checkHandshakeRequest(WebSocketContext context)
        {
            if (_originValidator != null)
            {
                if (!_originValidator(context.Origin))
                    return "It includes no Origin header or an invalid one.";
            }

            if (_cookiesValidator != null)
            {
                var req = context.CookieCollection;
                var res = context.WebSocket.CookieCollection;
                if (!_cookiesValidator(req, res))
                    return "It includes no cookie or an invalid one.";
            }

            return null;
        }

        /// <summary>
        /// The onClose
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="CloseEventArgs"/></param>
        private void onClose(object sender, CloseEventArgs e)
        {
            if (_id == null)
                return;

            _sessions.Remove(_id);
            OnClose(e);
        }

        /// <summary>
        /// The onError
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ErrorEventArgs"/></param>
        private void onError(object sender, ErrorEventArgs e)
        {
            OnError(e);
        }

        /// <summary>
        /// The onMessage
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="MessageEventArgs"/></param>
        private void onMessage(object sender, MessageEventArgs e)
        {
            OnMessage(e);
        }

        /// <summary>
        /// The onOpen
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void onOpen(object sender, EventArgs e)
        {
            _id = _sessions.Add(this);
            if (_id == null)
            {
                _websocket.Close(CloseStatusCode.Away);
                return;
            }

            _startTime = DateTime.Now;
            OnOpen();
        }

        #endregion 方法
    }
}