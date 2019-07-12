/*
 * HttpListenerAsyncResult.cs
 *
 * This code is derived from ListenerAsyncResult.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Ximian, Inc. (http://www.ximian.com)
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

/*
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@ximian.com>
 */

/*
 * Contributors:
 * - Nicholas Devenish
 */

using System;
using System.Threading;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="HttpListenerAsyncResult" />
    /// </summary>
    internal class HttpListenerAsyncResult : IAsyncResult
    {
        #region 字段

        /// <summary>
        /// Defines the _callback
        /// </summary>
        private AsyncCallback _callback;

        /// <summary>
        /// Defines the _completed
        /// </summary>
        private bool _completed;

        /// <summary>
        /// Defines the _context
        /// </summary>
        private HttpListenerContext _context;

        /// <summary>
        /// Defines the _endCalled
        /// </summary>
        private bool _endCalled;

        /// <summary>
        /// Defines the _exception
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Defines the _inGet
        /// </summary>
        private bool _inGet;

        /// <summary>
        /// Defines the _state
        /// </summary>
        private object _state;

        /// <summary>
        /// Defines the _sync
        /// </summary>
        private object _sync;

        /// <summary>
        /// Defines the _syncCompleted
        /// </summary>
        private bool _syncCompleted;

        /// <summary>
        /// Defines the _waitHandle
        /// </summary>
        private ManualResetEvent _waitHandle;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerAsyncResult"/> class.
        /// </summary>
        /// <param name="callback">The callback<see cref="AsyncCallback"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        internal HttpListenerAsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;
            _sync = new object();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the AsyncState
        /// </summary>
        public object AsyncState
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Gets the AsyncWaitHandle
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (_sync)
                    return _waitHandle ?? (_waitHandle = new ManualResetEvent(_completed));
            }
        }

        /// <summary>
        /// Gets a value indicating whether CompletedSynchronously
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return _syncCompleted;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsCompleted
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                lock (_sync)
                    return _completed;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether EndCalled
        /// </summary>
        internal bool EndCalled
        {
            get
            {
                return _endCalled;
            }

            set
            {
                _endCalled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether InGet
        /// </summary>
        internal bool InGet
        {
            get
            {
                return _inGet;
            }

            set
            {
                _inGet = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Complete
        /// </summary>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        internal void Complete(Exception exception)
        {
            _exception = _inGet && (exception is ObjectDisposedException)
                         ? new HttpListenerException(995, "The listener is closed.")
                         : exception;

            complete(this);
        }

        /// <summary>
        /// The Complete
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        internal void Complete(HttpListenerContext context)
        {
            Complete(context, false);
        }

        /// <summary>
        /// The Complete
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        /// <param name="syncCompleted">The syncCompleted<see cref="bool"/></param>
        internal void Complete(HttpListenerContext context, bool syncCompleted)
        {
            _context = context;
            _syncCompleted = syncCompleted;

            complete(this);
        }

        /// <summary>
        /// The GetContext
        /// </summary>
        /// <returns>The <see cref="HttpListenerContext"/></returns>
        internal HttpListenerContext GetContext()
        {
            if (_exception != null)
                throw _exception;

            return _context;
        }

        /// <summary>
        /// The complete
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="HttpListenerAsyncResult"/></param>
        private static void complete(HttpListenerAsyncResult asyncResult)
        {
            lock (asyncResult._sync)
            {
                asyncResult._completed = true;

                var waitHandle = asyncResult._waitHandle;
                if (waitHandle != null)
                    waitHandle.Set();
            }

            var callback = asyncResult._callback;
            if (callback == null)
                return;

            ThreadPool.QueueUserWorkItem(
              state =>
              {
                  try
                  {
                      callback(asyncResult);
                  }
                  catch
                  {
                  }
              },
              null
            );
        }

        #endregion 方法
    }
}