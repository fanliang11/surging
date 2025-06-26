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

namespace DotNetty.Transport.Libuv
{
    using System;
    using System.Threading;
    using DotNetty.Common.Concurrency;

    internal sealed class EventLoopChooserFactory<TEventLoop> : IEventExecutorChooserFactory<TEventLoop>
        where TEventLoop : LoopExecutor
    {
        public static readonly EventLoopChooserFactory<TEventLoop> Instance = new EventLoopChooserFactory<TEventLoop>();

        private EventLoopChooserFactory() { }

        public IEventExecutorChooser<TEventLoop> NewChooser(TEventLoop[] eventLoops)
        {
            if (IsPowerOfTwo(eventLoops.Length))
            {
                return new PowerOfTwoEventExecutorChooser(eventLoops);
            }
            else
            {
                return new GenericEventExecutorChooser(eventLoops);
            }
        }

        private static bool IsPowerOfTwo(int val)
        {
            return (val & -val) == val;
        }

        sealed class PowerOfTwoEventExecutorChooser : IEventExecutorChooser<TEventLoop>
        {
            private readonly TEventLoop[] _eventLoops;
            private readonly int _amount;
            private readonly bool _isSingle;
            private int _idx;

            public PowerOfTwoEventExecutorChooser(TEventLoop[] eventLoops)
            {
                _eventLoops = eventLoops;
                _amount = eventLoops.Length - 1;
                _isSingle = 0u >= (uint)_amount;
            }

            public TEventLoop GetNext()
            {
                if (_isSingle) { return _eventLoops[0]; }

                // Attempt to select event loop based on thread first
                int threadId = XThread.CurrentThread.Id;
                int i;
                for (i = 0; i < _eventLoops.Length; i++)
                {
                    var eventLoop = _eventLoops[i];
                    if (0u >= (uint)(eventLoop.LoopThreadId - threadId))
                    {
                        return eventLoop;
                    }
                }

                return _eventLoops[Interlocked.Increment(ref _idx) & _amount];
            }
        }

        sealed class GenericEventExecutorChooser : IEventExecutorChooser<TEventLoop>
        {
            private readonly TEventLoop[] _eventLoops;
            private readonly int _amount;
            //private readonly bool _isSingle;
            private int _idx;

            public GenericEventExecutorChooser(TEventLoop[] eventLoops)
            {
                _eventLoops = eventLoops;
                _amount = eventLoops.Length;
                //_isSingle = 1u >= (uint)_amount; // 最小值为 1
            }

            public TEventLoop GetNext()
            {
                //if (_isSingle) { return _eventLoops[0]; }

                // Attempt to select event loop based on thread first
                int threadId = XThread.CurrentThread.Id;
                int i;
                for (i = 0; i < _amount; i++)
                {
                    var eventLoop = _eventLoops[i];
                    if (0u >= (uint)(eventLoop.LoopThreadId - threadId))
                    {
                        return eventLoop;
                    }
                }

                return _eventLoops[Math.Abs(Interlocked.Increment(ref _idx) % _amount)];
            }
        }
    }
}