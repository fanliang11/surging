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

namespace DotNetty.Common.Utilities
{
    using System.Threading;

    public sealed class AtomicLong
    {
        long _value;

        public AtomicLong() { }
        public AtomicLong(long value) => _value = value;

        public long Value
        {
            get => Volatile.Read(ref _value);
            set => Interlocked.Exchange(ref _value, value);
        }

        public long Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        public long GetAndIncrement()
        {
            var v = Volatile.Read(ref _value);
            _ = Interlocked.Increment(ref _value);
            return v;
        }

        public long Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }

        public long AddAndGet(long v)
        {
            return Interlocked.Add(ref _value, v);
        }

        public bool CompareAndSet(long current, long next)
        {
            return Interlocked.CompareExchange(ref _value, next, current) == current;
        }

        public static implicit operator long(AtomicLong aInt) => aInt.Value;

        public static implicit operator AtomicLong(long newValue) => new AtomicLong(newValue);
    }
}
