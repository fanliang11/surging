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
    public class SniCompletionEvent : TlsCompletionEvent
    {
        private readonly string _hostName;

        public SniCompletionEvent(string hostName)
        {
            _hostName = hostName;
        }

        public SniCompletionEvent(Exception cause)
            : this(null, cause)
        {
        }

        public SniCompletionEvent(string hostName, Exception cause)
            : base(cause)
        {
            _hostName = hostName;
        }

        /// <summary>
        /// Returns the SNI hostname send by the client if we were able to parse it, <code>null</code> otherwise.
        /// </summary>
        public string HostName => _hostName;

        public override string ToString()
        {
            return IsSuccess
                ? $"{nameof(SniCompletionEvent)}(SUCCESS'{_hostName}')"
                : $"{nameof(SniCompletionEvent)}({Cause.Message})";
        }
    }
}
