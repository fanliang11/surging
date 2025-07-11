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
    using System;

    /// <summary>
    /// An <see cref="Enum"/> that represents the idle state of a <see cref="DotNetty.Transport.Channels.IChannel"/>.
    /// </summary>
    public enum IdleState
    {
        /// <summary>
        /// No data was received for a while.
        /// </summary>
        ReaderIdle,

        /// <summary>
        /// No data was sent for a while.
        /// </summary>
        WriterIdle,

        /// <summary>
        /// No data was either received or sent for a while.
        /// </summary>
        AllIdle
    }
}

