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

namespace DotNetty.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using Thread = DotNetty.Common.Concurrency.XThread;

    public static class ThreadDeathWatcher
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance(typeof(ThreadDeathWatcher));

        static readonly IQueue<Entry> PendingEntries = PlatformDependent.NewMpscQueue<Entry>();
        static readonly Watcher watcher = new Watcher();
        static int started;
        static Thread watcherThread;

        static ThreadDeathWatcher()
        {
            string poolName = "threadDeathWatcher";
            string serviceThreadPrefix = SystemPropertyUtil.Get("io.netty.serviceThreadPrefix");
            if (!string.IsNullOrEmpty(serviceThreadPrefix))
            {
                poolName = serviceThreadPrefix + poolName;
            }
        }

        /// <summary>
        /// Schedules the specified <see cref="Action"/> to run when the specified <see cref="Thread"/> dies.
        /// </summary>
        public static void Watch(Thread thread, Action task)
        {
            if (thread is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.thread); }
            if (task is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.task); }
            //if (!thread.IsAlive) { ThrowHelper.ThrowArgumentException(); }
            
            Schedule(thread, task, true);
        }

        /// <summary>
        /// Cancels the task scheduled via <see cref="Watch"/>.
        /// </summary>
        public static void Unwatch(Thread thread, Action task)
        {
            if (thread is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.thread); }
            if (task is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.task); }

            Schedule(thread, task, false);
        }

        static void Schedule(Thread thread, Action task, bool isWatch)
        {
            _ = PendingEntries.TryEnqueue(new Entry(thread, task, isWatch));

            if (SharedConstants.False >= (uint)Interlocked.CompareExchange(ref started, SharedConstants.True, SharedConstants.False))
            {
                var watcherThread = new Thread(s => ((IRunnable)s).Run());
                watcherThread.Start(watcher);
                _ = Interlocked.Exchange(ref ThreadDeathWatcher.watcherThread, watcherThread);
            }
        }

        /// <summary>
        /// Waits until the thread of this watcher has no threads to watch and terminates itself.
        /// Because a new watcher thread will be started again on <see cref="Watch"/>,
        /// this operation is only useful when you want to ensure that the watcher thread is terminated
        /// <strong>after</strong> your application is shut down and there's no chance of calling <see cref="Watch"/>
        /// afterwards.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns><c>true</c> if and only if the watcher thread has been terminated.</returns>
        public static bool AwaitInactivity(TimeSpan timeout)
        {
            Thread watcherThread = Volatile.Read(ref ThreadDeathWatcher.watcherThread);
            if (watcherThread is object)
            {
                _ = watcherThread.Join(timeout);
                return !watcherThread.IsAlive;
            }
            else
            {
                return true;
            }
        }

        sealed class Watcher : IRunnable
        {
            readonly List<Entry> watchees = new List<Entry>();

            public void Run()
            {
                while(true)
                {
                    this.FetchWatchees();
                    this.NotifyWatchees();

                    // Try once again just in case notifyWatchees() triggered watch() or unwatch().
                    this.FetchWatchees();
                    this.NotifyWatchees();

                    Thread.Sleep(1000);

                    if (0u >= (uint)this.watchees.Count && PendingEntries.IsEmpty)
                    {
                        // Mark the current worker thread as stopped.
                        // The following CAS must always success and must be uncontended,
                        // because only one watcher thread should be running at the same time.
                        bool stopped = SharedConstants.False < (uint)Interlocked.CompareExchange(ref started, SharedConstants.False, SharedConstants.True);
                        Debug.Assert(stopped);

                        // Check if there are pending entries added by watch() while we do CAS above.
                        if (PendingEntries.IsEmpty)
                        {
                            // A) watch() was not invoked and thus there's nothing to handle
                            //    -> safe to terminate because there's nothing left to do
                            // B) a new watcher thread started and handled them all
                            //    -> safe to terminate the new watcher thread will take care the rest
                            break;
                        }

                        // There are pending entries again, added by watch()
                        if (SharedConstants.False < (uint)Interlocked.CompareExchange(ref started, SharedConstants.True, SharedConstants.False))
                        {
                            // watch() started a new watcher thread and set 'started' to true.
                            // -> terminate this thread so that the new watcher reads from pendingEntries exclusively.
                            break;
                        }

                        // watch() added an entry, but this worker was faster to set 'started' to true.
                        // i.e. a new watcher thread was not started
                        // -> keep this thread alive to handle the newly added entries.
                    }
                }
            }

            void FetchWatchees()
            {
                while(true)
                {
                    Entry e;
                    if (!PendingEntries.TryDequeue(out e))
                    {
                        break;
                    }

                    if (e.IsWatch)
                    {
                        this.watchees.Add(e);
                    }
                    else
                    {
                        _ = this.watchees.Remove(e);
                    }
                }
            }

            void NotifyWatchees()
            {
                List<Entry> watchees = this.watchees;
                for (int i = 0; i < watchees.Count;)
                {
                    Entry e = watchees[i];
                    if (!e.Thread.IsAlive)
                    {
                        watchees.RemoveAt(i);
                        try
                        {
                            e.Task();
                        }
                        catch (Exception t)
                        {
                            Logger.ThreadDeathWatcherTaskRaisedAnException(t);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        sealed class Entry
        {
            internal readonly Thread Thread;
            internal readonly Action Task;
            internal readonly bool IsWatch;

            public Entry(Thread thread, Action task, bool isWatch)
            {
                this.Thread = thread;
                this.Task = task;
                this.IsWatch = isWatch;
            }

            public override int GetHashCode() => this.Thread.GetHashCode() ^ this.Task.GetHashCode();

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                if (!(obj is Entry))
                {
                    return false;
                }

                var that = (Entry)obj;
                return this.Thread == that.Thread && this.Task == that.Task;
            }
        }
    }
}