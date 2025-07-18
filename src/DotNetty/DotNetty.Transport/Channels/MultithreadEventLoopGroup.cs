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
    /// TBD
    /// </summary>
    public class MultithreadEventLoopGroup : MultithreadEventLoopGroup<MultithreadEventLoopGroup, SingleThreadEventLoop>
    {
        private static readonly Func<MultithreadEventLoopGroup, SingleThreadEventLoop> DefaultEventLoopFactory;

        static MultithreadEventLoopGroup()
        {
            DefaultEventLoopFactory = group => new SingleThreadEventLoop(group);
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup()
            : base(0, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads)
            : base(nThreads, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(TimeSpan breakoutInterval)
            : this(0, breakoutInterval)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(IRejectedExecutionHandler rejectedHandler)
            : this(0, rejectedHandler)
        {
        }


        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, group => new SingleThreadEventLoop(group, rejectedHandler))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, TimeSpan breakoutInterval)
            : base(nThreads, group => new SingleThreadEventLoop(group, breakoutInterval))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : base(nThreads, group => new SingleThreadEventLoop(group, rejectedHandler, breakoutInterval))
        {
        }


        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(IThreadFactory threadFactory)
            : this(0, threadFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IThreadFactory threadFactory)
            : base(nThreads, group => new SingleThreadEventLoop(group, threadFactory))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IThreadFactory threadFactory, TimeSpan breakoutInterval)
            : base(nThreads, group => new SingleThreadEventLoop(group, threadFactory, breakoutInterval))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, group => new SingleThreadEventLoop(group, threadFactory, rejectedHandler))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : base(nThreads, group => new SingleThreadEventLoop(group, threadFactory, rejectedHandler, breakoutInterval))
        {
        }


        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(IEventExecutorChooserFactory<SingleThreadEventLoop> chooserFactory)
            : base(0, chooserFactory, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IEventExecutorChooserFactory<SingleThreadEventLoop> chooserFactory)
            : base(nThreads, chooserFactory, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IEventExecutorChooserFactory<SingleThreadEventLoop> chooserFactory, TimeSpan breakoutInterval)
            : base(nThreads, chooserFactory, group => new SingleThreadEventLoop(group, breakoutInterval))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IEventExecutorChooserFactory<SingleThreadEventLoop> chooserFactory, IRejectedExecutionHandler rejectedHandler)
            : base(nThreads, chooserFactory, group => new SingleThreadEventLoop(group, rejectedHandler))
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IEventExecutorChooserFactory<SingleThreadEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : base(nThreads, chooserFactory, group => new SingleThreadEventLoop(group, rejectedHandler, breakoutInterval))
        {
        }


        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup"/>.</summary>
        public MultithreadEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<SingleThreadEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : base(nThreads, chooserFactory, group => new SingleThreadEventLoop(group, threadFactory, rejectedHandler, breakoutInterval))
        {
        }
    }
}