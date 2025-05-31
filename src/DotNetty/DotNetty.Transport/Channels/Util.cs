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

namespace DotNetty.Transport.Channels
{
    using System;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;

    public static class Util
    {
        static readonly IInternalLogger Log = InternalLoggerFactory.GetInstance<IChannel>();

        /// <summary>
        /// Marks the specified <see cref="IPromise"/> as success. If the
        /// <see cref="IPromise"/> is done already, logs a message.
        /// </summary>
        /// <param name="promise">The <see cref="IPromise"/> to complete.</param>
        /// <param name="logger">The <see cref="IInternalLogger"/> to use to log a failure message.</param>
        public static void SafeSetSuccess(IPromise promise, IInternalLogger logger)
        {
            if (!promise.IsVoid && !promise.TryComplete() && logger is object)
            {
                logger.FailedToMarkAPromiseAsSuccess(promise);
            }
        }

        /// <summary>
        /// Marks the specified <see cref="IPromise"/> as failure. If the
        /// <see cref="IPromise"/> is done already, log a message.
        /// </summary>
        /// <param name="promise">The <see cref="IPromise"/> to complete.</param>
        /// <param name="cause">The <see cref="Exception"/> to fail the <see cref="IPromise"/> with.</param>
        /// <param name="logger">The <see cref="IInternalLogger"/> to use to log a failure message.</param>
        public static void SafeSetFailure(IPromise promise, Exception cause, IInternalLogger logger)
        {
            if (!promise.IsVoid && !promise.TrySetException(cause) && logger is object)
            {
                logger.FailedToMarkAPromiseAsFailure(promise, cause);
            }
        }

        public static void CloseSafe(this IChannel channel)
        {
            CompleteChannelCloseTaskSafely(channel, channel.CloseAsync());
        }

        //public static void CloseSafe(this IChannelUnsafe u)
        //{
        //    CompleteChannelCloseTaskSafely(u, u.CloseAsync());
        //}

        internal static async void CompleteChannelCloseTaskSafely(object channelObject, Task closeTask)
        {
            try
            {
                await closeTask;
            }
            catch (TaskCanceledException) { }
#if DEBUG
            catch (Exception ex)
            {
                if (Log.DebugEnabled)
                {
                    Log.FailedToCloseChannelCleanly(channelObject, ex);
                }
            }
#else
            catch (Exception) { }
#endif
        }
    }
}