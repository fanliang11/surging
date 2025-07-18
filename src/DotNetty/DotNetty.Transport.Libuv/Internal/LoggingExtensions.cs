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
using System.Runtime.CompilerServices;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Libuv.Native;

namespace DotNetty.Transport.Libuv
{
    internal static class LibuvLoggingExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopWalkingHandles(this IInternalLogger logger, IntPtr handle, int count)
        {
            logger.Debug($"Loop {handle} walking handles, count = {count}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopRunningDefaultToCallCloseCallbacks(this IInternalLogger logger, IntPtr handle, int count)
        {
            logger.Debug($"Loop {handle} running default to call close callbacks, count = {count}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopCloseResult(this IInternalLogger logger, IntPtr handle, int result, int count)
        {
            logger.Debug($"Loop {handle} close result = {result}, count = {count}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopCloseAllHandlesLimit20TimesExceeded(this IInternalLogger logger, IntPtr handle)
        {
            logger.Warn($"Loop {handle} close all handles limit 20 times exceeded.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopClosed(this IInternalLogger logger, IntPtr handle, int count)
        {
            logger.Info($"Loop {handle} closed, count = {count}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopGCHandleReleased(this IInternalLogger logger, IntPtr handle)
        {
            logger.Info($"Loop {handle} GCHandle released.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopMemoryReleased(this IInternalLogger logger, IntPtr handle)
        {
            logger.Info($"Loop {handle} memory released.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopWalkCallbackDisposed(this IInternalLogger logger, IntPtr handle, IntPtr loopHandle, IDisposable target)
        {
            logger.Debug($"Loop {loopHandle} walk callback disposed {handle} {target?.GetType()}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopWalkCallbackAttemptToCloseHandleFailed(this IInternalLogger logger, IntPtr handle, IntPtr loopHandle, Exception exception)
        {
            logger.Warn($"Loop {loopHandle} Walk callback attempt to close handle {handle} failed. {exception}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCloseChannelCleanly(this IInternalLogger logger, object channelObject, Exception ex)
        {
            logger.Debug($"Failed to close channel {channelObject} cleanly.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopAllocated(this IInternalLogger logger, IntPtr handle)
        {
            logger.Info($"Loop {handle} allocated.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ListeningOnPipe(this IInternalLogger logger, int loopThreadId, string pipeName)
        {
            logger.Info("{} ({}) listening on pipe {}.", nameof(DispatcherEventLoop), loopThreadId, pipeName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DispatcherPipeConnected(this IInternalLogger logger, int loopThreadId, string pipeName)
        {
            logger.Info($"{nameof(WorkerEventLoop)} ({loopThreadId}) dispatcher pipe {pipeName} connected.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopThreadFinished(this IInternalLogger logger, XThread thread, IntPtr handle)
        {
            logger.Info("Loop {}:{} thread finished.", thread.Name, handle);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopRunDefaultError(this IInternalLogger logger, XThread thread, IntPtr handle, Exception ex)
        {
            logger.Error("Loop {}:{} run default error.", thread.Name, handle, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ShuttingDownLoopError(this IInternalLogger logger, Exception ex)
        {
            logger.Error("{}: shutting down loop error", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopDisposed(this IInternalLogger logger, XThread thread, IntPtr handle)
        {
            logger.Info("{}:{} disposed.", thread.Name, handle);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopDisposing(this IInternalLogger logger, IDisposable handle)
        {
            logger.Debug("Disposing {}", handle.GetType());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AcceptClientConnectionFailed(this IInternalLogger logger, Exception ex)
        {
            logger.Info("Accept client connection failed.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToDisposeAClientConnection(this IInternalLogger logger, Exception ex)
        {
            logger.Warn("Failed to dispose a client connection.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToConnectToDispatcher(this IInternalLogger logger, int retryCount, OperationException error)
        {
            logger.Info($"{nameof(WorkerEventLoop)} failed to connect to dispatcher, Retry count = {retryCount}", error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IPCPipeReadError(this IInternalLogger logger, OperationException error)
        {
            logger.Warn("IPC Pipe read error", error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToWriteServerHandleToClient(this IInternalLogger logger, OperationException error)
        {
            logger.Warn($"{nameof(PipeListener)} failed to write server handle to client", error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToSendServerHandleToClient(this IInternalLogger logger, Exception ex)
        {
            logger.Warn($"{nameof(PipeListener)} failed to send server handle to client", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReadError(this IInternalLogger logger, OperationException error)
        {
            logger.Warn($"{nameof(PipeListener)} read error", error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TcpHandleReadCallbcakError(this IInternalLogger logger, IntPtr handle, Exception exception)
        {
            logger.Warn($"Tcp {handle} read callbcak error.", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TerminatedWithNonEmptyTaskQueue(this IInternalLogger logger, int count)
        {
            logger.Warn($"{nameof(LoopExecutor)} terminated with non-empty task queue ({count})");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopReleaseError(this IInternalLogger logger, XThread thread, IntPtr handle, Exception ex)
        {
            logger.Warn("{}:{} release error {}", thread.Name, handle, ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoopDisposeError(this IInternalLogger logger, IDisposable handle, Exception ex)
        {
            logger.Warn("{} dispose error {}", handle.GetType(), ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToCreateConnectRequestToDispatcher(this IInternalLogger logger, Exception exception)
        {
            logger.Warn($"{nameof(WorkerEventLoop)} failed to create connect request to dispatcher", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FailedToConnectToDispatcher(this IInternalLogger logger, ConnectRequest request)
        {
            logger.Warn($"{nameof(WorkerEventLoop)} failed to connect to dispatcher", request.Error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CallbackRrror(this IInternalLogger logger, uv_handle_type handleType, IntPtr handle, Exception exception)
        {
            logger.Error($"{handleType} {handle} callback error.", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ErrorWhilstClosingHandle(this IInternalLogger logger, uv_req_type requestType, IntPtr handle, Exception exception)
        {
            logger.Error($"{requestType} {handle} error whilst closing handle.", exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ErrorWhilstClosingHandle(this IInternalLogger logger, IntPtr handle, Exception exception)
        {
            logger.Error($"{nameof(NativeHandle)} {handle} error whilst closing handle.", exception);
        }
    }
}
