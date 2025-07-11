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

namespace DotNetty.Common.Concurrency
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void XParameterizedThreadStart(object obj);

    [DebuggerDisplay("ID={threadId}, Name={Name}, IsExplicit={isExplicit}")]
    public sealed class XThread
    {
        private static int s_maxThreadId;

        [ThreadStatic]
        private static XThread s_currentThread;

        private readonly int _threadId;
#pragma warning disable CS0414
        private readonly bool _isExplicit; // For debugging only
#pragma warning restore CS0414
        private Task _task;
        private readonly EventWaitHandle _completed = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle _readyToStart = new EventWaitHandle(false, EventResetMode.AutoReset);
        private object _startupParameter;

        static int GetNewThreadId() => Interlocked.Increment(ref s_maxThreadId);

        XThread()
        {
            _threadId = GetNewThreadId();
            _isExplicit = false;
            IsAlive = false;
        }

        public XThread(Action action)
        {
            _threadId = GetNewThreadId();
            _isExplicit = true;
            IsAlive = false;
            CreateLongRunningTask(x => action());
        }

        public XThread(XParameterizedThreadStart threadStartFunc)
        {
            _threadId = GetNewThreadId();
            _isExplicit = true;
            IsAlive = false;
            CreateLongRunningTask(threadStartFunc);
        }

        public void Start()
        {
            _ = _readyToStart.Set();
            IsAlive = true;
        }

        void CreateLongRunningTask(XParameterizedThreadStart threadStartFunc)
        {
            using (ExecutionContext.IsFlowSuppressed() ? default(AsyncFlowControl?) : ExecutionContext.SuppressFlow())
            {
                _task = Task.Factory.StartNew(
                    () =>
                    {
                        // We start the task running, then unleash it by signaling the readyToStart event.
                        // This is needed to avoid thread reuse for tasks (see below)
                        _ = _readyToStart.WaitOne();
                        // This is the first time we're using this thread, therefore the TLS slot must be empty
                        if (s_currentThread is object)
                        {
                            Debug.WriteLine("warning: currentThread already created; OS thread reused");
                            Debug.Assert(false);
                        }
                        s_currentThread = this;
                        threadStartFunc(_startupParameter);
                        _ = _completed.Set();
                    },
                    CancellationToken.None,
                    // .NET always creates a brand new thread for LongRunning tasks
                    // This is not documented but unlikely to ever change:
                    // https://github.com/dotnet/corefx/issues/2576#issuecomment-126693306
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        public void Start(object parameter)
        {
            _startupParameter = parameter;
            Start();
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        /// <exception cref="T:System.OperationCanceledException"><paramref name="cancellationToken" /> was canceled.</exception>
        public static void Sleep(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            using (var ev = new ManualResetEventSlim())
            {
                ev.Wait(millisecondsTimeout, cancellationToken);
            }
        }

        public int Id => _threadId;

        public string Name { get; set; }

        public bool IsAlive { get; private set; }

        public static XThread CurrentThread
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => s_currentThread ?? EnsureThreadCreated();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static XThread EnsureThreadCreated()
        {
            return s_currentThread = new XThread();
        }

        public bool Join(TimeSpan timeout) => _completed.WaitOne(timeout);

        public bool Join(int millisecondsTimeout) => _completed.WaitOne(millisecondsTimeout);
    }
}