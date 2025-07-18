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
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Internal.Logging;
    using Thread = DotNetty.Common.Concurrency.XThread;

    public static class ReferenceCountUtil
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance(typeof(ReferenceCountUtil));

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Retain()"/> if the specified message implements
        /// <see cref="IReferenceCounted"/>. If the specified message doesn't implement
        /// <see cref="IReferenceCounted"/>, this method does nothing.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static T Retain<T>(T msg)
        {
            return msg is IReferenceCounted counted ? (T)counted.Retain() : msg;
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Retain(int)"/> if the specified message implements
        /// <see cref="IReferenceCounted"/>. If the specified message doesn't implement
        /// <see cref="IReferenceCounted"/>, this method does nothing.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static T Retain<T>(T msg, int increment)
        {
            return msg is IReferenceCounted counted ? (T)counted.Retain(increment) : msg;
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Touch()" /> if the specified message implements
        /// <see cref="IReferenceCounted" />.
        /// If the specified message doesn't implement <see cref="IReferenceCounted" />, this method does nothing.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static T Touch<T>(T msg)
        {
            return msg is IReferenceCounted refCnt ? (T)refCnt.Touch() : msg;
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Touch(object)" /> if the specified message implements
        /// <see cref="IReferenceCounted" />. If the specified message doesn't implement
        /// <see cref="IReferenceCounted" />, this method does nothing.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static T Touch<T>(T msg, object hint)
        {
            return msg is IReferenceCounted refCnt ? (T)refCnt.Touch(hint) : msg;
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Release()" /> if the specified message implements
        /// <see cref="IReferenceCounted"/>. If the specified message doesn't implement
        /// <see cref="IReferenceCounted"/>, this method does nothing.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool Release(object msg)
        {
            return msg is IReferenceCounted counted && counted.Release();
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Release(int)" /> if the specified message implements
        /// <see cref="IReferenceCounted"/>. If the specified message doesn't implement
        /// <see cref="IReferenceCounted"/>, this method does nothing.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool Release(object msg, int decrement)
        {
            return msg is IReferenceCounted counted && counted.Release(decrement);
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Release()" /> if the specified message implements
        /// <see cref="IReferenceCounted"/>. If the specified message doesn't implement
        /// <see cref="IReferenceCounted"/>, this method does nothing. Unlike <see cref="Release(object)"/>, this
        /// method catches an exception raised by <see cref="IReferenceCounted.Release()" /> and logs it, rather than
        /// rethrowing it to the caller. It is usually recommended to use <see cref="Release(object)"/> instead, unless
        /// you absolutely need to swallow an exception.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static void SafeRelease(object msg)
        {
            try
            {
                _ = Release(msg);
            }
            catch (Exception ex)
            {
                Logger.FailedToReleaseAMessage(msg, ex);
            }
        }

        /// <summary>
        /// Tries to call <see cref="IReferenceCounted.Release(int)" /> if the specified message implements
        /// <see cref="IReferenceCounted"/>. If the specified message doesn't implement
        /// <see cref="IReferenceCounted"/>, this method does nothing. Unlike <see cref="Release(object)"/>, this
        /// method catches an exception raised by <see cref="IReferenceCounted.Release(int)" /> and logs it, rather
        /// than rethrowing it to the caller. It is usually recommended to use <see cref="Release(object, int)"/>
        /// instead, unless you absolutely need to swallow an exception.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static void SafeRelease(object msg, int decrement)
        {
            try
            {
                _ = Release(msg, decrement);
            }
            catch (Exception ex)
            {
                if (Logger.WarnEnabled)
                {
                    Logger.FailedToReleaseAMessage(msg, decrement, ex);
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static void SafeRelease(this IReferenceCounted msg)
        {
            try
            {
                _ = msg?.Release();
            }
            catch (Exception ex)
            {
                Logger.FailedToReleaseAMessage(msg, ex);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static void SafeRelease(this IReferenceCounted msg, int decrement)
        {
            try
            {
                _ = msg?.Release(decrement);
            }
            catch (Exception ex)
            {
                Logger.FailedToReleaseAMessage(msg, decrement, ex);
            }
        }

        /// <summary>
        /// Schedules the specified object to be released when the caller thread terminates. Note that this operation
        /// is intended to simplify reference counting of ephemeral objects during unit tests. Do not use it beyond the
        /// intended use case.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static T ReleaseLater<T>(T msg) => ReleaseLater(msg, 1);

        /// <summary>
        /// Schedules the specified object to be released when the caller thread terminates. Note that this operation
        /// is intended to simplify reference counting of ephemeral objects during unit tests. Do not use it beyond the
        /// intended use case.
        /// </summary>
        public static T ReleaseLater<T>(T msg, int decrement)
        {
            if (msg is IReferenceCounted referenceCounted)
            {
                ThreadDeathWatcher.Watch(Thread.CurrentThread, () =>
                {
                    try
                    {
                        if (!referenceCounted.Release(decrement))
                        {
                            Logger.NonZeroRefCnt(referenceCounted, decrement);
                        }
#if DEBUG
                        else
                        {
                            if (Logger.DebugEnabled) Logger.ReleasedObject(referenceCounted, decrement);
                        }
#endif
                    }
                    catch (Exception ex)
                    {
                        Logger.FailedToReleaseAObject(referenceCounted, ex);
                    }
                });
            }
            return msg;
        }

        internal static string FormatReleaseString(IReferenceCounted referenceCounted, int decrement)
            => $"{referenceCounted.GetType().Name}.Release({decrement.ToString(CultureInfo.InvariantCulture)}) refCnt: {referenceCounted.ReferenceCount.ToString(CultureInfo.InvariantCulture)}";
    }
}