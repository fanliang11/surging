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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace DotNetty.Transport
{
    internal static class TransportLoggingExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FreedThreadLocalBufferFromThreadFull(this IInternalLogger logger, Exception error, Exception cause)
        {
            logger.Debug("An exception {}"
                + "was thrown by a user handler's exceptionCaught() "
                + "method while handling the following exception:", error, cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FreedThreadLocalBufferFromThread(this IInternalLogger logger, Exception error, Exception cause)
        {
            logger.Warn("An exception '{}' [enable DEBUG level for full stacktrace] "
                + "was thrown by a user handler's exceptionCaught() "
                + "method while handling the following exception:", error, cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseChannelCleanly(this IInternalLogger logger, object channelObject, Exception ex)
        {
            logger.Debug("Failed to close channel " + channelObject + " cleanly.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseChannel(this IInternalLogger logger, IChannel channel, Exception ex)
        {
            logger.Warn("Failed to close channel: " + channel, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DiscardedInboundMessage(this IInternalLogger logger, object msg)
        {
            logger.Debug("Discarded inbound message {} that reached at the tail of the pipeline. " +
                "Please check your pipeline configuration.", msg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DiscardedInboundMessage(this IInternalLogger logger, IChannelHandlerContext ctx)
        {
            logger.Debug("Discarded message pipeline : {}. Channel : {}.",
                         ctx.Pipeline, ctx.Channel);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ExceptionOnAccept(this IInternalLogger logger, SocketException ex)
        {
            logger.Info("Exception on accept.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ChildGroupIsNotSetUsingParentGroupInstead(this IInternalLogger logger)
        {
            logger.Warn("childGroup is not set. Using parentGroup instead.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ChildGroupIsNotSetUsingParentGroupInstead(this IInternalLogger logger, IChannel child, Exception ex)
        {
            logger.Warn("Failed to register an accepted channel: " + child, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseAPartiallyInitializedSocket(this IInternalLogger logger, SocketException ex2)
        {
            logger.Warn("Failed to close a partially initialized socket.", ex2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCreateANewChannelFromAcceptedSocket(this IInternalLogger logger, Exception ex)
        {
            logger.Warn("Failed to create a new channel from accepted socket.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseASocketCleanly(this IInternalLogger logger, Exception ex2)
        {
            logger.Warn("Failed to close a socket cleanly.", ex2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseAChannel(this IInternalLogger logger, Exception e)
        {
            logger.Warn("Failed to close a channel.", e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ForceClosingAChannel(this IInternalLogger logger, IChannel channel, Exception ex)
        {
            logger.Warn("Force-closing a channel whose registration task was not accepted by an event loop: {}", channel, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnexpectedExceptionOccurredWhileDeregisteringChannel(this IInternalLogger logger, Exception t)
        {
            logger.Warn("Unexpected exception occurred while deregistering a channel.", t);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CannotInvokeTaskLaterAsEventLoopRejectedIt(this IInternalLogger logger, RejectedExecutionException e)
        {
            logger.Warn("Can't invoke task later as EventLoop rejected it", e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToSubmitAnExceptionCaughtEvent(this IInternalLogger logger, Exception t)
        {
            logger.Warn("Failed to submit an ExceptionCaught() event.", t);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TheExceptionCaughtEventThatWasFailedToSubmit(this IInternalLogger logger, Exception cause)
        {
            logger.Warn("The ExceptionCaught() event that was failed to submit was:", cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ExceptionCaughtMethodWhileHandlingTheFollowingException(this IInternalLogger logger, Exception cause)
        {
            logger.Warn("An exception was thrown by a user handler's ExceptionCaught() method while handling the following exception:", cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToInitializeAChannel(this IInternalLogger logger, IChannelHandlerContext ctx, Exception cause)
        {
            logger.Warn("Failed to initialize a channel. Closing: " + ctx.Channel, cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToRemoveAHandler(this IInternalLogger logger, IChannelHandlerContext ctx, Exception ex2)
        {
            logger.Warn($"Failed to remove a handler: {ctx.Name}", ex2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AnExceptionCaughtEventWasFired(this IInternalLogger logger, Exception cause)
        {
            logger.Warn("An ExceptionCaught() event was fired, and it reached at the tail of the pipeline. " +
                "It usually means the last handler in the pipeline did not handle the exception.", cause);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CannotInvokeHandlerAddedAsTheIEventExecutorRejectedIt(this IInternalLogger logger, IEventExecutor executor, IChannelHandlerContext ctx, Exception e)
        {
            logger.Warn("Can't invoke HandlerAdded() as the IEventExecutor {} rejected it, removing handler {}.",
                executor, ctx.Name, e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CannotInvokeHandlerRemovedAsTheIEventExecutorRejectedIt(this IInternalLogger logger, IEventExecutor executor, IChannelHandlerContext ctx, Exception e)
        {
            logger.Warn("Can't invoke HandlerRemoved() as the IEventExecutor {} rejected it, removing handler {}.",
                executor, ctx.Name, e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseAFileStream(this IInternalLogger logger, Exception exception)
        {
            logger.Warn("Failed to close a file stream.", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToMarkAPromiseAsSuccess(this IInternalLogger logger, IPromise promise)
        {
            var err = promise.Task.Exception?.InnerException;
            if (err is null)
            {
                logger.Warn("Failed to mark a promise as success because it has succeeded already: {}", promise);
            }
            else
            {
                logger.Warn("Failed to mark a promise as success because it has failed already: {}, unnotified cause:", promise, err);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToMarkAPromiseAsFailure(this IInternalLogger logger, IPromise promise, Exception cause)
        {
            var err = promise.Task.Exception?.InnerException;
            if (err is null)
            {
                logger.Warn("Failed to mark a promise as failure because it has succeeded already: {}", promise, cause);
            }
            else
            {
                logger.Warn("Failed to mark a promise as failure because it has failed already: {}, unnotified cause {}", promise, err.ToString(), cause);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowableBeingSuppressedBecauseIsAlreadyPending(this IInternalLogger logger, Exception pending, Exception err)
        {
            logger.Info($"Throwable being suppressed because Throwable {pending.ToString()} is already pending", err);
        }
    }
}
