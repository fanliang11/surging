// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;

namespace DotNetty.Common
{
    internal static class CommonLoggingExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ATaskRaisedAnException(this IInternalLogger logger, IRunnable task, Exception ex)
        {
            logger.Warn("A task raised an exception. Task: {}", task, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ShutdownHookRaisedAnException(this IInternalLogger logger, Exception ex)
        {
            logger.Warn("Shutdown hook raised an exception.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AnEventExecutorTerminatedWithNonEmptyTaskQueue(this IInternalLogger logger, int numUserTasks)
        {
            logger.Warn($"An event executor terminated with non-empty task queue ({numUserTasks})");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToRetrieveTheListOfAvailableNetworkInterfaces(this IInternalLogger logger, Exception e)
        {
            logger.Warn("Failed to retrieve the list of available network interfaces", e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AnExcWasThrownWhileProcessingACancellationTask(this IInternalLogger logger, Exception ex)
        {
            logger.Warn("An exception was thrown while processing a cancellation task", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AnExceptionWasThrownBy(this IInternalLogger logger, ITimerTask task, Exception t)
        {
            logger.Warn($"An exception was thrown by {task.GetType().Name}.", t);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToReleaseAMessage(this IInternalLogger logger, object msg, Exception ex)
        {
            logger.Warn("Failed to release a message: {}", msg, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToReleaseAMessage(this IInternalLogger logger, object msg, int decrement, Exception ex)
        {
            logger.Warn("Failed to release a message: {} (decrement: {})", msg, decrement, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToReleaseAObject(this IInternalLogger logger, IReferenceCounted referenceCounted, Exception ex)
        {
            logger.Warn("Failed to release an object: {}", referenceCounted, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void NonZeroRefCnt(this IInternalLogger logger, IReferenceCounted referenceCounted, int decrement)
        {
            logger.Warn("Non-zero refCnt: {}", ReferenceCountUtil.FormatReleaseString(referenceCounted, decrement));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReleasedObject(this IInternalLogger logger, IReferenceCounted referenceCounted, int decrement)
        {
            logger.Debug("Released: {}", ReferenceCountUtil.FormatReleaseString(referenceCounted, decrement));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThreadDeathWatcherTaskRaisedAnException(this IInternalLogger logger, Exception t)
        {
            logger.Warn("Thread death watcher task raised an exception:", t);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToMarkAPromiseAsCancel(this IInternalLogger logger, IPromise promise)
        {
            var err = promise.Execption();
            if (err is null)
            {
                logger.Warn("Failed to cancel promise because it has succeeded already: {}", promise);
            }
            else
            {
                logger.Warn("Failed to cancel promise because it has failed already: {}, unnotified cause:", promise, err);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToMarkAPromiseAsSuccess(this IInternalLogger logger, IPromise promise)
        {
            var err = promise.Execption();
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
            var err = promise.Execption();
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
        public static void UnexpectedExceptionFromAnEventExecutor(this IInternalLogger logger, Exception ex)
        {
            logger.Error("Unexpected exception from an event executor: ", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ExecutionLoopFailed(this IInternalLogger logger, XThread thread, Exception ex)
        {
            logger.Error("{}: execution loop failed", thread.Name, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TimeoutProcessingFailed(this IInternalLogger logger, Exception ex)
        {
            logger.Error("Timeout processing failed.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void BuggyImplementation(this IInternalLogger logger)
        {
            logger.Error(
                $"Buggy {typeof(IEventExecutor).Name} implementation; {typeof(SingleThreadEventExecutor).Name}.ConfirmShutdown() must be called "
                + "before run() implementation terminates.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void BuggyImplementation(this IInternalLogger logger, SingleThreadEventExecutor eventExecutor)
        {
            var type = eventExecutor.GetType();
            logger.Error(
                $"Buggy {type.Name} implementation; {type.Name}.ConfirmShutdown() must be called "
                + "before run() implementation terminates.");
        }
    }
}
