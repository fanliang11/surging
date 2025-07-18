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


namespace DotNetty.Common.Utilities
{
    using System.Threading;

    public sealed class AtomicInteger
    {
        int value;

        public AtomicInteger() { }
        public AtomicInteger(int value) => this.value = value;

        public int Value
        {
            get => Volatile.Read(ref this.value);
            set => Interlocked.Exchange(ref this.value, value);
        }

        public int Increment()
        {
            return Interlocked.Increment(ref this.value);
        }

        public int GetAndIncrement()
        {
            var v = Volatile.Read(ref this.value);
            _ = Interlocked.Increment(ref this.value);
            return v;
        }

        public int Decrement()
        {
            return Interlocked.Decrement(ref this.value);
        }

        public int AddAndGet(int v)
        {
            return Interlocked.Add(ref this.value, v);
        }

        public bool CompareAndSet(int current, int next)
        {
            return Interlocked.CompareExchange(ref this.value, next, current) == current;
        }

        public static implicit operator int(AtomicInteger aInt) => aInt.Value;

        public static implicit operator AtomicInteger(int newValue) => new AtomicInteger(newValue);
    }
}
