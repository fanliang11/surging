﻿/*
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

namespace DotNetty.Common
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public abstract class FastThreadLocal
    {
        static readonly int VariablesToRemoveIndex = InternalThreadLocalMap.NextVariableIndex();

        /// <summary>
        ///     Removes all <see cref="FastThreadLocal"/> variables bound to the current thread.  This operation is useful when you
        ///     are in a container environment, and you don't want to leave the thread local variables in the threads you do not
        ///     manage.
        /// </summary>
        public static void RemoveAll()
        {
            InternalThreadLocalMap threadLocalMap = InternalThreadLocalMap.GetIfSet();
            if (threadLocalMap is null)
            {
                return;
            }

            try
            {
                object v = threadLocalMap.GetIndexedVariable(VariablesToRemoveIndex);
                if (v is object && v != InternalThreadLocalMap.Unset)
                {
                    var variablesToRemove = (HashSet<FastThreadLocal>)v;
                    foreach (FastThreadLocal tlv in variablesToRemove) // todo: do we need to make a snapshot?
                    {
                        tlv.Remove(threadLocalMap);
                    }
                }
            }
            finally
            {
                InternalThreadLocalMap.Remove();
            }
        }

        /// <summary>
        /// Destroys the data structure that keeps all <see cref="FastThreadLocal"/> variables accessed from
        /// non-<see cref="FastThreadLocal"/>s.  This operation is useful when you are in a container environment, and
        /// you do not want to leave the thread local variables in the threads you do not manage.  Call this method when
        /// your application is being unloaded from the container.
        /// </summary>
        public static void Destroy() => InternalThreadLocalMap.Destroy();

        protected static void AddToVariablesToRemove(InternalThreadLocalMap threadLocalMap, FastThreadLocal variable)
        {
            object v = threadLocalMap.GetIndexedVariable(VariablesToRemoveIndex);
            HashSet<FastThreadLocal> variablesToRemove;
            if (v == InternalThreadLocalMap.Unset || v is null)
            {
                variablesToRemove = new HashSet<FastThreadLocal>(); // Collections.newSetFromMap(new IdentityHashMap<FastThreadLocal<?>, Boolean>());
                _ = threadLocalMap.SetIndexedVariable(VariablesToRemoveIndex, variablesToRemove);
            }
            else
            {
                variablesToRemove = (HashSet<FastThreadLocal>)v;
            }

            _ = variablesToRemove.Add(variable);
        }

        protected static void RemoveFromVariablesToRemove(InternalThreadLocalMap threadLocalMap, FastThreadLocal variable)
        {
            object v = threadLocalMap.GetIndexedVariable(VariablesToRemoveIndex);

            if (v == InternalThreadLocalMap.Unset || v is null)
            {
                return;
            }

            var variablesToRemove = (HashSet<FastThreadLocal>)v;
            _ = variablesToRemove.Remove(variable);
        }

        /// <summary>
        ///     Sets the value to uninitialized; a proceeding call to get() will trigger a call to GetInitialValue().
        /// </summary>
        /// <param name="threadLocalMap"></param>
        public abstract void Remove(InternalThreadLocalMap threadLocalMap);
    }

    public class FastThreadLocal<T> : FastThreadLocal
        where T : class
    {
        private readonly int _index;
        /// <summary>
        ///     Returns the number of thread local variables bound to the current thread.
        /// </summary>
        public static int Count => InternalThreadLocalMap.GetIfSet()?.Count ?? 0;

        public FastThreadLocal()
        {
            _index = InternalThreadLocalMap.NextVariableIndex();
        }

        /// <summary>
        ///     Gets or sets current value for the current thread.
        /// </summary>
        public T Value
        {
            get { return Get(InternalThreadLocalMap.Get()); }
            set { Set(InternalThreadLocalMap.Get(), value); }
        }

        /// <summary>
        ///     Returns the current value for the specified thread local map.
        ///     The specified thread local map must be for the current thread.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public T Get(InternalThreadLocalMap threadLocalMap)
        {
            object v = threadLocalMap.GetIndexedVariable(_index);
            if (v != InternalThreadLocalMap.Unset)
            {
                return (T)v;
            }

            return Initialize(threadLocalMap);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        T Initialize(InternalThreadLocalMap threadLocalMap)
        {
            T v = GetInitialValue();

            _ = threadLocalMap.SetIndexedVariable(_index, v);
            AddToVariablesToRemove(threadLocalMap, this);
            return v;
        }

        /// <summary>
        /// Set the value for the specified thread local map. The specified thread local map must be for the current thread.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public void Set(InternalThreadLocalMap threadLocalMap, T value)
        {
            if (threadLocalMap.SetIndexedVariable(_index, value))
            {
                AddToVariablesToRemove(threadLocalMap, this);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if and only if this thread-local variable is set.
        /// </summary>
        public bool IsSet() => IsSet(InternalThreadLocalMap.GetIfSet());

        /// <summary>
        /// Returns <c>true</c> if and only if this thread-local variable is set.
        /// The specified thread local map must be for the current thread.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public bool IsSet(InternalThreadLocalMap threadLocalMap) => threadLocalMap is object && threadLocalMap.IsIndexedVariableSet(_index);

        /// <summary>
        /// Returns the initial value for this thread-local variable.
        /// </summary>
        protected virtual T GetInitialValue() => null;

        public void Remove() => Remove(InternalThreadLocalMap.GetIfSet());

        /// <summary>
        /// Sets the value to uninitialized for the specified thread local map;
        /// a proceeding call to <see cref="Get"/> will trigger a call to <see cref="GetInitialValue"/>.
        /// The specified thread local map must be for the current thread.
        /// </summary>
        /// <param name="threadLocalMap">
        /// The <see cref="InternalThreadLocalMap"/> from which this <see cref="FastThreadLocal"/> should be removed.
        /// </param>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public sealed override void Remove(InternalThreadLocalMap threadLocalMap)
        {
            if (threadLocalMap is null)
            {
                return;
            }

            object v = threadLocalMap.RemoveIndexedVariable(_index);
            RemoveFromVariablesToRemove(threadLocalMap, this);

            if (v != InternalThreadLocalMap.Unset)
            {
                OnRemoval((T)v);
            }
        }

        /// <summary>
        /// Invoked when this thread local variable is removed by <see cref="Remove()"/>.
        /// </summary>
        protected virtual void OnRemoval(T value)
        {
        }
    }
}