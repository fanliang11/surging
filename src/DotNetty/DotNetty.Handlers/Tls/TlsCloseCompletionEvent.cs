/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;

namespace DotNetty.Handlers.Tls
{
    /// <summary>
    /// Event that is fired once the close_notify was received or if an failure happens before it was received.
    /// </summary>
    public sealed class TlsCloseCompletionEvent : TlsCompletionEvent
    {
        public static readonly TlsCloseCompletionEvent Success = new TlsCloseCompletionEvent();

        /// <summary>
        /// Creates a new event that indicates a successful receiving of close_notify.
        /// </summary>
        private TlsCloseCompletionEvent() { }

        /// <summary>
        /// Creates a new event that indicates an close_notify was not received because of an previous error.
        /// Use <see cref="Success"/> to indicate a success.
        /// </summary>
        /// <param name="cause"></param>
        public TlsCloseCompletionEvent(Exception cause) : base(cause) { }
    }
}
