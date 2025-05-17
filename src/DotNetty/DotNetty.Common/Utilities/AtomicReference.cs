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

    /// <summary>
    ///     Implementation of the java.concurrent.util AtomicReference type.
    /// </summary>
    public sealed class AtomicReference<T>
        where T : class
    {
        T atomicValue;

        /// <summary>
        ///     Sets the initial value of this <see cref="AtomicReference{T}" /> to <paramref name="originalValue"/>.
        /// </summary>
        public AtomicReference(T originalValue)
        {
            this.atomicValue = originalValue;
        }

        /// <summary>
        ///     Default constructor
        /// </summary>
        public AtomicReference()
        {
            this.atomicValue = default;
        }

        /// <summary>
        ///     The current value of this <see cref="AtomicReference{T}" />
        /// </summary>
        public T Value
        {
            get => Volatile.Read(ref this.atomicValue);
            set => Interlocked.Exchange(ref this.atomicValue, value);
        }

        /// <summary>
        ///     If <see cref="Value" /> equals <paramref name="expected"/>, then set the Value to
        ///     <paramref name="newValue"/>
        ///     Returns true if  <paramref name="newValue"/> was set, false otherwise.
        /// </summary>
        public bool CompareAndSet(T expected, T newValue) => Interlocked.CompareExchange(ref this.atomicValue, newValue, expected) == expected;

        #region Conversion operators

        /// <summary>
        ///     Implicit conversion operator = automatically casts the <see cref="AtomicReference{T}" /> to an instance of
        /// </summary>
        public static implicit operator T(AtomicReference<T> aRef) => aRef.Value;

        /// <summary>
        ///     Implicit conversion operator = allows us to cast any type directly into a <see cref="AtomicReference{T}" />
        ///     instance.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static implicit operator AtomicReference<T>(T newValue) => new AtomicReference<T>(newValue);

        #endregion
    }
}