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
using System.Collections.Generic;

namespace CoAP.Util
{
    /// <summary>
    /// Provides methods to wait for a response of a request.
    /// </summary>
    /// <typeparam name="TRequest">the type of the request</typeparam>
    /// <typeparam name="TResponse">the type of the response</typeparam>
    public class WaitFuture<TRequest, TResponse> : IDisposable
    {
        private readonly TRequest _request;
        private TResponse _response;
        private System.Threading.ManualResetEvent _mre = new System.Threading.ManualResetEvent(false);

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="request">the request to wait</param>
        public WaitFuture(TRequest request)
        {
            _request = request;
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        public TRequest Request
        {
            get { return _request; }
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public TResponse Response
        {
            get { return _response; }
            set
            {
                _response = value;
                try { _mre.Set(); }
                catch (ObjectDisposedException) { /* do nothing */ }
            }
        }

        /// <summary>
        /// Waits for response.
        /// </summary>
        public void Wait()
        {
            _mre.WaitOne();
        }

        /// <summary>
        /// Waits for response for the given time.
        /// </summary>
        public void Wait(Int32 millisecondsTimeout)
        {
            _mre.WaitOne(millisecondsTimeout);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ((IDisposable)_mre).Dispose();
        }

        /// <summary>
        /// Waits for all response.
        /// </summary>
        public static void WaitAll(IEnumerable<WaitFuture<TRequest, TResponse>> futures)
        {
            foreach (var f in futures)
            {
                f.Wait();
            }
        }

        /// <summary>
        /// Waits for all response for the given time.
        /// </summary>
        public static void WaitAll(IEnumerable<WaitFuture<TRequest, TResponse>> futures, Int32 millisecondsTimeout)
        {
            foreach (var f in futures)
            {
                f.Wait(millisecondsTimeout);
            }
        }
    }
}
