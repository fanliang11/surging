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

namespace DotNetty.Transport.Libuv
{
    using System;
    using System.Diagnostics;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Libuv.Native;

    public sealed class DispatcherEventLoop : LoopExecutor
    {
        private PipeListener _pipeListener;
        private IServerNativeUnsafe _nativeUnsafe;

        internal DispatcherEventLoop(IEventLoopGroup parent)
            : this(parent, DefaultThreadFactory<DispatcherEventLoop>.Instance)
        {
        }

        internal DispatcherEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory)
            : base(parent, threadFactory, RejectedExecutionHandlers.Reject(), DefaultBreakoutInterval)
        {
            if (parent is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.parent); }

            string pipeName = "DotNetty_" + Guid.NewGuid().ToString("n");
            PipeName = (PlatformApi.IsWindows
                ? @"\\.\pipe\"
                : "/tmp/") + pipeName;
            Start();
        }

        internal string PipeName { get; }

        internal void Register(IServerNativeUnsafe serverChannel)
        {
            Debug.Assert(serverChannel is object);
            _nativeUnsafe = serverChannel;
        }

        protected override void Initialize()
        {
            _pipeListener = new PipeListener(UnsafeLoop, false);
            _pipeListener.Listen(PipeName);

            if (Logger.InfoEnabled)
            {
                Logger.ListeningOnPipe(LoopThreadId, PipeName);
            }
        }

        protected override void Release() => _pipeListener.Shutdown();

        internal void Dispatch(NativeHandle handle)
        {
            try
            {
                _pipeListener.DispatchHandle(handle);
            }
            catch
            {
                handle.CloseHandle();
                throw;
            }
        }

        internal void Accept(NativeHandle handle) => _nativeUnsafe.Accept(handle);
    }
}
