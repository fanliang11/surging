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
    using System.Threading;

    /// <summary>
    /// Default implementation which uses simple round-robin to choose next <see cref="IEventExecutor"/>.
    /// </summary>
    public sealed class DefaultEventExecutorChooserFactory<TEventExecutor> : IEventExecutorChooserFactory<TEventExecutor>
        where TEventExecutor : class, IEventExecutor
    {
        public static readonly DefaultEventExecutorChooserFactory<TEventExecutor> Instance = new DefaultEventExecutorChooserFactory<TEventExecutor>();

        private DefaultEventExecutorChooserFactory() { }

        /// <summary>
        /// Returns a new <see cref="IEventExecutorChooser{TEventExecutor}"/>.
        /// </summary>
        /// <param name="executors"></param>
        /// <returns></returns>
        public IEventExecutorChooser<TEventExecutor> NewChooser(TEventExecutor[] executors)
        {
            if (IsPowerOfTwo(executors.Length))
            {
                return new PowerOfTwoEventExecutorChooser(executors);
            }
            else
            {
                return new GenericEventExecutorChooser(executors);
            }
        }

        private static bool IsPowerOfTwo(int val)
        {
            return (val & -val) == val;
        }

        sealed class PowerOfTwoEventExecutorChooser : IEventExecutorChooser<TEventExecutor>
        {
            private readonly TEventExecutor[] _executors;
            private readonly int _amount;
            private readonly bool _isSingle;
            private int _idx;

            public PowerOfTwoEventExecutorChooser(TEventExecutor[] executors)
            {
                _executors = executors;
                _amount = executors.Length - 1;
                _isSingle = 0u >= (uint)_amount;
            }

            public TEventExecutor GetNext()
            {
                if (_isSingle) { return _executors[0]; }
                return _executors[Interlocked.Increment(ref _idx) & _amount];
            }
        }

        sealed class GenericEventExecutorChooser : IEventExecutorChooser<TEventExecutor>
        {
            private readonly TEventExecutor[] _executors;
            private readonly int _amount;
            //private readonly bool _isSingle;
            private int _idx;

            public GenericEventExecutorChooser(TEventExecutor[] executors)
            {
                _executors = executors;
                _amount = executors.Length;
                //_isSingle = 1u >= (uint)_amount; // 最小值为 1
            }

            public TEventExecutor GetNext()
            {
                //if (_isSingle) { return _executors[0]; }
                return _executors[Math.Abs(Interlocked.Increment(ref _idx) % _amount)];
            }
        }
    }
}