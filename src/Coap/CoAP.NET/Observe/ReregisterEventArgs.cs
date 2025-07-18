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

namespace CoAP.Observe
{
    /// <summary>
    /// Represents an event when a observing request is reregistering.
    /// </summary>
    public class ReregisterEventArgs : EventArgs
    {
        readonly Request _refreshRequest;

        /// <summary>
        /// Instantiates.
        /// </summary>
        public ReregisterEventArgs(Request refresh)
        {
            _refreshRequest = refresh;
        }

        /// <summary>
        /// Gets the request sent to refresh an observation.
        /// </summary>
        public Request RefreshRequest
        {
            get { return _refreshRequest; }
        }
    }
}
