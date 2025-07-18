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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Handlers.Timeout
{
    using DotNetty.Common.Utilities;

    /// <summary>
    /// A user event triggered by <see cref="IdleStateHandler"/> when a <see cref="DotNetty.Transport.Channels.IChannel"/> is idle.
    /// </summary>
    public class IdleStateEvent
    {
        public static readonly IdleStateEvent FirstReaderIdleStateEvent = new DefaultIdleStateEvent(IdleState.ReaderIdle, true);
        public static readonly IdleStateEvent ReaderIdleStateEvent = new DefaultIdleStateEvent(IdleState.ReaderIdle, false);
        public static readonly IdleStateEvent FirstWriterIdleStateEvent = new DefaultIdleStateEvent(IdleState.WriterIdle, true);
        public static readonly IdleStateEvent WriterIdleStateEvent = new DefaultIdleStateEvent(IdleState.WriterIdle, false);
        public static readonly IdleStateEvent FirstAllIdleStateEvent = new DefaultIdleStateEvent(IdleState.AllIdle, true);
        public static readonly IdleStateEvent AllIdleStateEvent = new DefaultIdleStateEvent(IdleState.AllIdle, false);

        /// <summary>
        /// Constructor for sub-classes.
        /// </summary>
        /// <param name="state">the <see cref="IdleStateEvent"/> which triggered the event.</param>
        /// <param name="first"><c>true</c> if its the first idle event for the <see cref="IdleStateEvent"/>.</param>
        protected IdleStateEvent(IdleState state, bool first)
        {
            State = state;
            First = first;
        }

        /// <summary>
        /// Returns the idle state.
        /// </summary>
        /// <value>The state.</value>
        public IdleState State { get; }

        /// <summary>
        /// Returns <c>true</c> if this was the first event for the <see cref="IdleState"/>
        /// </summary>
        /// <returns><c>true</c> if first; otherwise, <c>false</c>.</returns>
        public bool First { get; }

        public override string ToString()
        {
            return $"{StringUtil.SimpleClassName(this)}({State}{(First ? ", first" : "")})";
        }

        private sealed class DefaultIdleStateEvent : IdleStateEvent
        {
            private readonly string _representation;

            public DefaultIdleStateEvent(IdleState state, bool first)
                : base(state, first)
            {
                _representation = $"IdleStateEvent({state}{(first ? ", first" : "")})";
            }

            public override string ToString() => _representation;
        }
    }
}

