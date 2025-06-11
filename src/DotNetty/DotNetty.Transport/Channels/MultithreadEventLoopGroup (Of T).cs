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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;

    /// <summary>
    /// Abstract base class for <see cref="IEventLoopGroup"/> implementations that handles their tasks with multiple threads at
    /// the same time.
    /// </summary>
    public abstract class MultithreadEventLoopGroup<TLoopGroup, TEventLoop> : MultithreadEventExecutorGroup<TLoopGroup, TEventLoop>, IEventLoopGroup
        where TLoopGroup : MultithreadEventLoopGroup<TLoopGroup, TEventLoop>
        where TEventLoop : class, IEventLoop
    {
        private static readonly int DefaultEventLoopThreadCount = Environment.ProcessorCount * 2;

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup{TGroup, TExecutor}"/>.</summary>
        protected MultithreadEventLoopGroup(int nThreads, Func<TLoopGroup, TEventLoop> eventLoopFactory)
            : this(0u >= (uint)nThreads ? DefaultEventLoopThreadCount : nThreads, DefaultEventExecutorChooserFactory<TEventLoop>.Instance, eventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="MultithreadEventLoopGroup{TGroup, TExecutor}"/>.</summary>
        protected MultithreadEventLoopGroup(int nThreads,
            IEventExecutorChooserFactory<TEventLoop> chooserFactory,
            Func<TLoopGroup, TEventLoop> eventLoopFactory)
            : base(0u >= (uint)nThreads ? DefaultEventLoopThreadCount : nThreads, chooserFactory, eventLoopFactory)
        {
        }

        IEnumerable<IEventLoop> IEventLoopGroup.Items => GetItems();

        IEventLoop IEventLoopGroup.GetNext() => GetNext();

        public virtual Task RegisterAsync(IChannel channel) => GetNext().RegisterAsync(channel);
    }
}