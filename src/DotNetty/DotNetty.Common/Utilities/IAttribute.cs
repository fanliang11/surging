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
    /// <summary>An attribute which allows to store a value reference. It may be updated atomically and so is thread-safe.</summary>
    /// <typeparam name="T">the type of the value it holds.</typeparam>
    public interface IAttribute<T>
    {
        /// <summary>
        ///     Returns the key of this attribute.
        /// </summary>
        AttributeKey<T> Key { get; }

        /// <summary>
        ///     Returns the current value, which may be <c>null</c>
        /// </summary>
        T Get();

        /// <summary>
        ///     Sets the value
        /// </summary>
        void Set(T value);

        /// <summary>
        ///     Atomically sets to the given value and returns the old value which may be <c>null</c> if non was set before.
        /// </summary>
        T GetAndSet(T value);

        /// <summary>
        ///     Atomically sets to the given value if this <see cref="IAttribute{T}" />'s value is <c>null</c>.
        ///     If it was not possible to set the value as it contains a value it will just return the current value.
        /// </summary>
        T SetIfAbsent(T value);

        /// <summary>
        ///     Removes this attribute from the <see cref="IAttributeMap" /> and returns the old value. Subsequent
        ///     <see cref="Get" />
        ///     calls will return <c>null</c>.
        ///     If you only want to return the old value and clear the <see cref="IAttribute{T}" /> while still keep it in
        ///     <see cref="IAttributeMap" /> use <see cref="GetAndSet" /> with a value of <c>null</c>.
        /// </summary>
        T GetAndRemove();

        /// <summary>
        ///     Atomically sets the value to the given updated value if the current value == the expected value.
        ///     If it the set was successful it returns <c>true</c> otherwise <c>false</c>.
        /// </summary>
        bool CompareAndSet(T oldValue, T newValue);

        /// <summary>
        ///     Removes this attribute from the <see cref="IAttributeMap" />. Subsequent <see cref="Get" /> calls will return
        ///     <c>null</c>.
        ///     If you only want to remove the value and clear the <see cref="IAttribute{T}" /> while still keep it in
        ///     <see cref="IAttributeMap" /> use <see cref="Set" /> with a value of <c>null</c>.
        /// </summary>
        void Remove();
    }
}