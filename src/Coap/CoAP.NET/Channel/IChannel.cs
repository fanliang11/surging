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
    /// Represents a channel where bytes data can flow through.
    /// </summary>
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Gets the local endpoint of this channel.
        /// </summary>
        System.Net.EndPoint LocalEndPoint { get; }
        /// <summary>
        /// Occurs when some bytes are received in this channel.
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;
        /// <summary>
        /// Starts this channel.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops this channel.
        /// </summary>
        void Stop();
        /// <summary>
        /// Sends data through this channel. This method should be non-blocking.
        /// </summary>
        /// <param name="data">the bytes to send</param>
        /// <param name="ep">the target endpoint</param>
        void Send(Byte[] data, System.Net.EndPoint ep);
    }
}
