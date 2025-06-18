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

namespace DotNetty.Handlers.Tls
{
    using System;

    public sealed class TlsHandshakeCompletionEvent
    {
        public static readonly TlsHandshakeCompletionEvent Success = new TlsHandshakeCompletionEvent();

        readonly Exception _exception;

        /// <summary>
        ///     Creates a new event that indicates a successful handshake.
        /// </summary>
        TlsHandshakeCompletionEvent()
        {
            _exception = null;
        }

        /// <summary>
        ///     Creates a new event that indicates an unsuccessful handshake.
        ///     Use {@link #SUCCESS} to indicate a successful handshake.
        /// </summary>
        public TlsHandshakeCompletionEvent(Exception exception)
        {
            if (exception is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.exception); }
            _exception = exception;
        }

        /// <summary>
        ///     Return <c>true</c> if the handshake was successful
        /// </summary>
        public bool IsSuccessful => _exception is null;

        /// <summary>
        ///     Return the {@link Throwable} if {@link #isSuccess()} returns <c>false</c>
        ///     and so the handshake failed.
        /// </summary>
        public Exception Exception => _exception;

        public override string ToString()
        {
            Exception ex = Exception;
            return ex is null ? "TlsHandshakeCompletionEvent(SUCCESS)" : $"TlsHandshakeCompletionEvent({ex})";
        }
    }
}