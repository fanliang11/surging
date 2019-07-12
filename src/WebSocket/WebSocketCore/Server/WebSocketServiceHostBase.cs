/*
 * WebSocketServiceHost.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2017 sta.blockhead
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

/*
 * Contributors:
 * - Juan Manuel Lallana <juan.manuel.lallana@gmail.com>
 */

using System;
using WebSocketCore.Net.WebSockets;

namespace WebSocketCore.Server
{
    /// <summary>
    /// Exposes the methods and properties used to access the information in
    /// a WebSocket service provided by the <see cref="WebSocketServer"/> or
    /// <see cref="HttpServer"/>.
    /// </summary>
    public abstract class WebSocketServiceHostBase
    {
        #region 字段

        /// <summary>
        /// Defines the _log
        /// </summary>
        private Logger _log;

        /// <summary>
        /// Defines the _path
        /// </summary>
        private string _path;

        /// <summary>
        /// Defines the _sessions
        /// </summary>
        private WebSocketSessionManager _sessions;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServiceHostBase"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="log">The log<see cref="Logger"/></param>
        protected WebSocketServiceHostBase(string path, Logger log)
        {
            _path = path;
            _log = log;

            _sessions = new WebSocketSessionManager(log);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the <see cref="Type"/> of the behavior of the service.
        /// </summary>
        public abstract Type BehaviorType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the service cleans up
        /// the inactive sessions periodically.
        /// </summary>
        public bool KeepClean
        {
            get
            {
                return _sessions.KeepClean;
            }

            set
            {
                _sessions.KeepClean = value;
            }
        }

        /// <summary>
        /// Gets the path to the service.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Gets the management function for the sessions in the service.
        /// </summary>
        public WebSocketSessionManager Sessions
        {
            get
            {
                return _sessions;
            }
        }

        /// <summary>
        /// Gets or sets the time to wait for the response to the WebSocket Ping or
        /// Close.
        /// </summary>
        public TimeSpan WaitTime
        {
            get
            {
                return _sessions.WaitTime;
            }

            set
            {
                _sessions.WaitTime = value;
            }
        }

        /// <summary>
        /// Gets the State
        /// </summary>
        internal ServerState State
        {
            get
            {
                return _sessions.State;
            }
        }

        /// <summary>
        /// Gets the logging function for the service.
        /// </summary>
        protected Logger Log
        {
            get
            {
                return _log;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Start
        /// </summary>
        internal void Start()
        {
            _sessions.Start();
        }

        /// <summary>
        /// The StartSession
        /// </summary>
        /// <param name="context">The context<see cref="WebSocketContext"/></param>
        internal void StartSession(WebSocketContext context)
        {
            CreateSession().Start(context, _sessions);
        }

        /// <summary>
        /// The Stop
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        internal void Stop(ushort code, string reason)
        {
            _sessions.Stop(code, reason);
        }

        /// <summary>
        /// Creates a new session for the service.
        /// </summary>
        /// <returns>The <see cref="WebSocketBehavior"/></returns>
        protected abstract WebSocketBehavior CreateSession();

        #endregion 方法
    }
}