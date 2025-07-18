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

namespace DotNetty.Transport.Channels
{
    using System;
    using DotNetty.Common.Concurrency;

    /// <summary>
    /// <see cref="MultithreadEventLoopGroup{T1, T2}"/> which must be used for the local transport.
    /// </summary>
    public class DefaultEventLoopGroup : MultithreadEventLoopGroup<DefaultEventLoopGroup, DefaultEventLoop>
    {
        private static readonly Func<DefaultEventLoopGroup, DefaultEventLoop> DefaultEventLoopFactory;

        static DefaultEventLoopGroup()
        {
            DefaultEventLoopFactory = group => new DefaultEventLoop(group);
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup()
            : base(0, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads)
            : base(nThreads, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IRejectedExecutionHandler rejectedHandler)
            : this(0, rejectedHandler)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IEventLoopTaskQueueFactory queueFactory)
            : this(0, queueFactory)
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, group => new DefaultEventLoop(group, rejectedHandler))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventLoop(group, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventLoop(group, queueFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventLoop(group, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventLoop(group, rejectedHandler, queueFactory))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IThreadFactory threadFactory)
            : this(0, threadFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, queueFactory: null))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, queueFactory: null))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, queueFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory,
            IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, queueFactory))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory)
            : base(0, chooserFactory, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory)
            : base(nThreads, chooserFactory, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, rejectedHandler, queueFactory: null))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory, int maxPendingTasks)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, queueFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, rejectedHandler, queueFactory))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, maxPendingTasks))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, queueFactory))
        {
        }
    }
}