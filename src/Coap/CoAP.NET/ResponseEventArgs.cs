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

namespace CoAP
{
    /// <summary>
    /// Represents an event of a message.
    /// </summary>
    /// <typeparam name="T">the type of the message</typeparam>
    public class MessageEventArgs<T> : EventArgs where T : Message
    {
        readonly T _message;

        /// <summary>
        /// 
        /// </summary>
        public MessageEventArgs(T message)
        {
            _message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public T Message
        {
            get { return _message; }
        }
    }

    /// <summary>
    /// Represents an event when a response arrives for a request.
    /// </summary>
    public class ResponseEventArgs : MessageEventArgs<Response>
    {
        /// <summary>
        /// 
        /// </summary>
        public ResponseEventArgs(Response response)
            : base(response)
        { }

        /// <summary>
        /// Gets the incoming response.
        /// </summary>
        public Response Response
        {
            get { return Message; }
        }
    }
}
