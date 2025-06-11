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

namespace CoAP.Channel
{
    /// <summary>
    /// Provides data for <see cref="IChannel.DataReceived"/> event.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        readonly Byte[] _data;
        readonly System.Net.EndPoint _endPoint;

        /// <summary>
        /// </summary>
        public DataReceivedEventArgs(Byte[] data, System.Net.EndPoint endPoint)
        {
            _data = data;
            _endPoint = endPoint;
        }

        /// <summary>
        /// Gets the received bytes.
        /// </summary>
        public Byte[] Data { get { return _data; } }

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> where the data is received from.
        /// </summary>
        public System.Net.EndPoint EndPoint { get { return _endPoint; } }
    }
}
