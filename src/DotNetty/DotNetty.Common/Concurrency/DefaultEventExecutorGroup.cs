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

    /// <summary>
    /// Default implementation of <see cref="MultithreadEventExecutorGroup{T1,T2}"/> which will use <see cref="DefaultEventExecutor"/> instances
    /// to handle the tasks.
    /// </summary>
    public class DefaultEventExecutorGroup : MultithreadEventExecutorGroup<DefaultEventExecutorGroup, DefaultEventExecutor>
    {
        private static readonly Func<DefaultEventExecutorGroup, DefaultEventExecutor> DefaultEventExecutorFactory;

        static DefaultEventExecutorGroup()
        {
            DefaultEventExecutorFactory = group => new DefaultEventExecutor(group);
        }



        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup()
            : base(0, DefaultEventExecutorFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads)
            : base(nThreads, DefaultEventExecutorFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(IRejectedExecutionHandler rejectedHandler)
            : this(0, rejectedHandler)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(IEventExecutorTaskQueueFactory queueFactory)
            : this(0, queueFactory)
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, group => new DefaultEventExecutor(group, rejectedHandler))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventExecutor(group, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventExecutor(group, queueFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventExecutor(group, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventExecutor(group, rejectedHandler, queueFactory))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(IThreadFactory threadFactory)
            : this(0, threadFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory)
            : base(nThreads, group => new DefaultEventExecutor(group, threadFactory, queueFactory: null))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, group => new DefaultEventExecutor(group, threadFactory, rejectedHandler, queueFactory: null))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventExecutor(group, threadFactory, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventExecutor(group, threadFactory, queueFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventExecutor(group, threadFactory, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory,
            IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventExecutor(group, threadFactory, rejectedHandler, queueFactory))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory)
            : base(0, chooserFactory, DefaultEventExecutorFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory)
            : base(nThreads, chooserFactory, DefaultEventExecutorFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, rejectedHandler, queueFactory: null))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory, int maxPendingTasks)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, queueFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, rejectedHandler, queueFactory))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, threadFactory, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventExecutorGroup"/>.</summary>
        public DefaultEventExecutorGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<DefaultEventExecutor> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : base(nThreads, chooserFactory, group => new DefaultEventExecutor(group, threadFactory, rejectedHandler, queueFactory))
        {
        }
    }
}