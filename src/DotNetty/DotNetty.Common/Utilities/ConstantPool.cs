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
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     A pool of <see cref="IConstant" />s.
    /// </summary>
    public abstract class ConstantPool
    {
        readonly Dictionary<string, IConstant> constants = new Dictionary<string, IConstant>(StringComparer.Ordinal);

        int nextId = 1;

        /// <summary>Shortcut of <c>this.ValueOf(firstNameComponent.Name + "#" + secondNameComponent)</c>.</summary>
        public IConstant ValueOf<T>(Type firstNameComponent, string secondNameComponent)
        {
            if (firstNameComponent is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.firstNameComponent); }
            if (secondNameComponent is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.secondNameComponent); }

            return this.ValueOf<T>(firstNameComponent.Name + '#' + secondNameComponent);
        }

        /// <summary>
        ///     Returns the <see cref="IConstant" /> which is assigned to the specified <c>name</c>.
        ///     If there's no such <see cref="IConstant" />, a new one will be created and returned.
        ///     Once created, the subsequent calls with the same <c>name</c> will always return the previously created one
        ///     (i.e. singleton.)
        /// </summary>
        /// <param name="name">the name of the <see cref="IConstant" /></param>
        public IConstant ValueOf<T>(string name)
        {
            IConstant c;

            lock (this.constants)
            {
                if (this.constants.TryGetValue(name, out c))
                {
                    return c;
                }
                else
                {
                    c = this.NewInstance0<T>(name);
                }
            }

            return c;
        }

        /// <summary>Returns <c>true</c> if a <see cref="AttributeKey{T}" /> exists for the given <c>name</c>.</summary>
        public bool Exists(string name)
        {
            if (string.IsNullOrEmpty(name)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
            lock (this.constants)
            {
                return this.constants.ContainsKey(name);
            }
        }

        /// <summary>
        ///     Creates a new <see cref="IConstant" /> for the given <c>name</c> or fail with an
        ///     <see cref="ArgumentException" /> if a <see cref="IConstant" /> for the given <c>name</c> exists.
        /// </summary>
        public IConstant NewInstance<T>(string name)
        {
            if (this.Exists(name))
            {
                ThrowHelper.ThrowArgumentException(name);
            }

            IConstant c = this.NewInstance0<T>(name);

            return c;
        }

        // Be careful that this dose not check whether the argument is null or empty.
        IConstant NewInstance0<T>(string name)
        {
            lock (this.constants)
            {
                IConstant c = this.NewConstant<T>(this.nextId, name);
                this.constants[name] = c;
                this.nextId++;
                return c;
            }
        }

        //static void CheckNotNullAndNotEmpty(string name) => Contract.Requires(!string.IsNullOrEmpty(name));

        protected abstract IConstant NewConstant<T>(int id, string name);

        [Obsolete]
        public int NextId()
        {
            lock (this.constants)
            {
                int id = this.nextId;
                this.nextId++;
                return id;
            }
        }
    }
}