/*
 * IWebSocketSession.cs
 *
 * The MIT License
 *
 * Copyright (c) 2013-2018 sta.blockhead
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
using WebSocketCore.Net.WebSockets;

namespace WebSocketCore.Server
{
    #region 接口

    /// <summary>
    /// Exposes the access to the information in a WebSocket session.
    /// </summary>
    public interface IWebSocketSession
    {
        #region 属性

        /// <summary>
        /// Gets the current state of the WebSocket connection for the session.
        /// </summary>
        WebSocketState ConnectionState { get; }

        /// <summary>
        /// Gets the information in the WebSocket handshake request.
        /// </summary>
        WebSocketContext Context { get; }

        /// <summary>
        /// Gets the unique ID of the session.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Gets the name of the WebSocket subprotocol for the session.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Gets the time that the session has started.
        /// </summary>
        DateTime StartTime { get; }

        #endregion 属性
    }

    #endregion 接口
}