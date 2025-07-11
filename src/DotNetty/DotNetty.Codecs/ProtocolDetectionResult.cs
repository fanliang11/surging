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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using DotNetty.Buffers;

    /// <summary>
    /// Result of detecting a protocol.
    /// </summary>
    /// <typeparam name="T">the type of the protocol</typeparam>
    public sealed class ProtocolDetectionResult<T> where T : class
    {
        /// <summary>
        /// Returns a <see cref="ProtocolDetectionResult{T}"/> that signals that more data is needed to detect the protocol.
        /// </summary>
        public static readonly ProtocolDetectionResult<T> NeedsMoreData;

        /// <summary>
        /// Returns a <see cref="ProtocolDetectionResult{T}"/> that signals the data was invalid for the protocol.
        /// </summary>
        public static readonly ProtocolDetectionResult<T> Invalid;

        static ProtocolDetectionResult()
        {
            NeedsMoreData = new ProtocolDetectionResult<T>(ProtocolDetectionState.NeedsMoreData, default);
            Invalid = new ProtocolDetectionResult<T>(ProtocolDetectionState.Invalid, default);
        }

        private ProtocolDetectionResult(ProtocolDetectionState state, T result)
        {
            State = state;
            DetectedProtocol = result;
        }

        /// <summary>
        /// Returns a <see cref="ProtocolDetectionResult{T}"/> which holds the detected protocol.
        /// </summary>
        public static ProtocolDetectionResult<T> Detected(T protocol)
        {
            if (protocol is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.protocol); }

            return new ProtocolDetectionResult<T>(ProtocolDetectionState.Detected, protocol);
        }

        /// <summary>
        /// Return the <see cref="ProtocolDetectionState"/>. If the state is <see cref="ProtocolDetectionState.Detected"/> you
        /// can retrieve the protocol via <see cref="DetectedProtocol"/>.
        /// </summary>
        public ProtocolDetectionState State { get; }

        /// <summary>
        /// Returns the protocol if <see cref="State"/> returns <see cref="ProtocolDetectionState.Detected"/>, otherwise <c>null</c>.
        /// </summary>
        public T DetectedProtocol { get; }
    }
}
