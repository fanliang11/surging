/*
 * HttpStreamAsyncResult.cs
 *
 * This code is derived from HttpStreamAsyncResult.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2015 sta.blockhead
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
 * - Gonzalo Paniagua Javier <gonzalo@novell.com>
 */

using System;
using System.Threading;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="HttpStreamAsyncResult" />
    /// </summary>
    internal class HttpStreamAsyncResult : IAsyncResult
    {
        #region 字段

        /// <summary>
        /// Defines the _buffer
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Defines the _callback
        /// </summary>
        private AsyncCallback _callback;

        /// <summary>
        /// Defines the _completed
        /// </summary>
        private bool _completed;

        /// <summary>
        /// Defines the _count
        /// </summary>
        private int _count;

        /// <summary>
        /// Defines the _exception
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Defines the _offset
        /// </summary>
        private int _offset;

        /// <summary>
        /// Defines the _state
        /// </summary>
        private object _state;

        /// <summary>
        /// Defines the _sync
        /// </summary>
        private object _sync;

        /// <summary>
        /// Defines the _syncRead
        /// </summary>
        private int _syncRead;

        /// <summary>
        /// Defines the _waitHandle
        /// </summary>
        private ManualResetEvent _waitHandle;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStreamAsyncResult"/> class.
        /// </summary>
        /// <param name="callback">The callback<see cref="AsyncCallback"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        internal HttpStreamAsyncResult(AsyncCallback callback, object state)
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
                return _syncRead == _count;
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
        /// Gets or sets the Buffer
        /// </summary>
        internal byte[] Buffer
        {
            get
            {
                return _buffer;
            }

            set
            {
                _buffer = value;
            }
        }

        /// <summary>
        /// Gets or sets the Count
        /// </summary>
        internal int Count
        {
            get
            {
                return _count;
            }

            set
            {
                _count = value;
            }
        }

        /// <summary>
        /// Gets the Exception
        /// </summary>
        internal Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        /// <summary>
        /// Gets a value indicating whether HasException
        /// </summary>
        internal bool HasException
        {
            get
            {
                return _exception != null;
            }
        }

        /// <summary>
        /// Gets or sets the Offset
        /// </summary>
        internal int Offset
        {
            get
            {
                return _offset;
            }

            set
            {
                _offset = value;
            }
        }

        /// <summary>
        /// Gets or sets the SyncRead
        /// </summary>
        internal int SyncRead
        {
            get
            {
                return _syncRead;
            }

            set
            {
                _syncRead = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Complete
        /// </summary>
        internal void Complete()
        {
            lock (_sync)
            {
                if (_completed)
                    return;

                _completed = true;
                if (_waitHandle != null)
                    _waitHandle.Set();

                if (_callback != null)
                    _callback.BeginInvoke(this, ar => _callback.EndInvoke(ar), null);
            }
        }

        /// <summary>
        /// The Complete
        /// </summary>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        internal void Complete(Exception exception)
        {
            _exception = exception;
            Complete();
        }

        #endregion 方法
    }
}