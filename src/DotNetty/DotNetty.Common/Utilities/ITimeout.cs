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

namespace DotNetty.Common.Utilities
{
    /// <summary>
    /// A handle associated with a <see cref="ITimerTask"/> that is returned by a
    /// <see cref="ITimer"/>.
    /// </summary>
    public interface ITimeout
    {
        /// <summary>
        /// Returns the <see cref="ITimer"/> that created this handle.
        /// </summary>
        ITimer Timer { get; }

        /// <summary>
        /// Returns the <see cref="ITimerTask"/> which is associated with this handle.
        /// </summary>
        ITimerTask Task { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if the <see cref="ITimerTask"/> associated
        /// with this handle has been expired.
        /// </summary>
        bool Expired { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if the <see cref="ITimerTask"/> associated
        /// with this handle has been canceled.
        /// </summary>
        bool Canceled { get; }

        /// <summary>
        /// Attempts to cancel the <see cref="ITimerTask"/> associated with this handle.
        /// If the task has been executed or canceled already, it will return with
        /// no side effect.
        /// </summary>
        /// <returns><c>true</c> if the cancellation completed successfully, otherwise <c>false</c>.</returns>
        bool Cancel();
    }
}