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

namespace DotNetty.Common.Concurrency
{
    using System;
    using DotNetty.Common.Internal.Logging;

    public static class IPromiseExtensions
    {
        public static void TrySetCanceled(this IPromise promise, IInternalLogger logger)
        {
            if (!promise.TrySetCanceled() && logger is object && logger.WarnEnabled)
            {
                var err = promise.Task.Exception;
                if (err is null)
                {
                    logger.Warn($"Failed to cancel promise because it has succeeded already: {promise}");
                }
                else
                {
                    logger.Warn($"Failed to cancel promise because it has failed already: {promise}, unnotified cause:", err);
                }
            }
        }

        public static void TryComplete(this IPromise promise, IInternalLogger logger)
        {
            if (!promise.TryComplete() && logger is object && logger.WarnEnabled)
            {
                var err = promise.Task.Exception;
                if (err is null)
                {
                    logger.Warn($"Failed to mark a promise as success because it has succeeded already: {promise}");
                }
                else
                {
                    logger.Warn($"Failed to mark a promise as success because it has failed already: {promise}, unnotified cause:", err);
                }
            }
        }

        public static void TrySetException(this IPromise promise, Exception cause, IInternalLogger logger)
        {
            if (!promise.TrySetException(cause) && logger is object && logger.WarnEnabled)
            {
                var err = promise.Task.Exception;
                if (err is null)
                {
                    logger.Warn($"Failed to mark a promise as failure because it has succeeded already: {promise}", cause);
                }
                else
                {
                    logger.Warn($"Failed to mark a promise as failure because it has failed already: {promise}, unnotified cause:{err.ToString()}", cause);
                }
            }
        }
    }
}