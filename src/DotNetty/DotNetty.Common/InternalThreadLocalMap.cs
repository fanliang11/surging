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

namespace DotNetty.Common
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// The internal data structure that stores the thread-local variables for DotNetty and all
    /// <see cref="FastThreadLocal"/>s. Note that this class is for internal use only and is subject to change at any
    /// time. Use <see cref="FastThreadLocal"/> unless you know what you are doing.
    /// </summary>
    public sealed class InternalThreadLocalMap
    {
        const int DefaultArrayListInitialCapacity = 8;

        public static readonly object Unset = new object();
        [ThreadStatic]
        private static InternalThreadLocalMap s_slowThreadLocalMap;

        private static int s_nextIndex;

        /// <summary>
        /// Used by <see cref="FastThreadLocal"/>.
        /// </summary>
        private object[] _indexedVariables;

        // Core thread-locals
        int _futureListenerStackDepth;
        int _localChannelReaderStackDepth;

        // String-related thread-locals
        StringBuilder _stringBuilder;

        // ArrayList-related thread-locals
        List<ICharSequence> _charSequences;
        List<AsciiString> _asciiStrings;

        internal static int NextVariableIndex()
        {
            int index = Interlocked.Increment(ref s_nextIndex);
            if (index < 0)
            {
                _ = Interlocked.Decrement(ref s_nextIndex);
                ThrowHelper.ThrowInvalidOperationException_TooMany();
            }
            return index;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static InternalThreadLocalMap GetIfSet() => s_slowThreadLocalMap;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static InternalThreadLocalMap Get()
        {
            InternalThreadLocalMap ret = s_slowThreadLocalMap;
            if (ret is null)
            {
                ret = new InternalThreadLocalMap();
                s_slowThreadLocalMap = ret;
            }
            return ret;
        }

        public static void Remove() => s_slowThreadLocalMap = null;

        public static void Destroy() => s_slowThreadLocalMap = null;

        // Cache line padding (must be public)
        // With CompressedOops enabled, an instance of this class should occupy at least 128 bytes.
        // ReSharper disable InconsistentNaming
        public long rp1, rp2, rp3, rp4, rp5, rp6, rp7, rp8, rp9;
        // ReSharper restore InconsistentNaming

        InternalThreadLocalMap()
        {
            _indexedVariables = CreateIndexedVariableTable();
        }

        static object[] CreateIndexedVariableTable()
        {
            var array = new object[32];

            array.Fill(Unset);
            return array;
        }

        public int Count
        {
            get
            {
                int count = 0;

                if (_futureListenerStackDepth != 0)
                {
                    count++;
                }
                if (_localChannelReaderStackDepth != 0)
                {
                    count++;
                }
                if (_stringBuilder is object)
                {
                    count++;
                }
                foreach (object o in _indexedVariables)
                {
                    if (o != Unset)
                    {
                        count++;
                    }
                }

                // We should subtract 1 from the count because the first element in 'indexedVariables' is reserved
                // by 'FastThreadLocal' to keep the list of 'FastThreadLocal's to remove on 'FastThreadLocal.RemoveAll()'.
                return count - 1;
            }
        }

        public StringBuilder StringBuilder
        {
            get
            {
                StringBuilder builder = _stringBuilder;
                if (builder is null)
                {
                    _stringBuilder = builder = new StringBuilder(512);
                }
                else
                {
                    builder.Length = 0;
                }
                return builder;
            }
        }

        public List<ICharSequence> CharSequenceList(int minCapacity = DefaultArrayListInitialCapacity)
        {
            List<ICharSequence> localList = _charSequences;
            if (localList is null)
            {
                _charSequences = new List<ICharSequence>(minCapacity);
                return _charSequences;
            }

            localList.Clear();
            // ensureCapacity
            localList.Capacity = minCapacity;
            return localList;
        }

        public List<AsciiString> AsciiStringList(int minCapacity = DefaultArrayListInitialCapacity)
        {
            List<AsciiString> localList = _asciiStrings;
            if (localList is null)
            {
                _asciiStrings = new List<AsciiString>(minCapacity);
                return _asciiStrings;
            }

            localList.Clear();
            // ensureCapacity
            localList.Capacity = minCapacity;
            return localList;
        }

        public int FutureListenerStackDepth
        {
            get => _futureListenerStackDepth;
            set => _futureListenerStackDepth = value;
        }

        public int LocalChannelReaderStackDepth
        {
            get => _localChannelReaderStackDepth;
            set => _localChannelReaderStackDepth = value;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public object GetIndexedVariable(int index)
        {
            object[] lookup = _indexedVariables;
            return (uint)index < (uint)lookup.Length ? lookup[index] : Unset;
        }

        /// <summary>
        /// Sets a value at the given index in this <see cref="InternalThreadLocalMap"/>.
        /// </summary>
        /// <param name="index">The desired index at which a value should be set.</param>
        /// <param name="value">The value to set at the given index.</param>
        /// <returns><c>true</c> if and only if a new thread-local variable has been created.</returns>
        public bool SetIndexedVariable(int index, object value)
        {
            object[] lookup = _indexedVariables;
            if ((uint)index < (uint)lookup.Length)
            {
                object oldValue = lookup[index];
                lookup[index] = value;
                return oldValue == Unset;
            }
            else
            {
                ExpandIndexedVariableTableAndSet(index, value);
                return true;
            }
        }

        void ExpandIndexedVariableTableAndSet(int index, object value)
        {
            object[] oldArray = _indexedVariables;
            int oldCapacity = oldArray.Length;
            int newCapacity = index;
            newCapacity |= newCapacity.RightUShift(1);
            newCapacity |= newCapacity.RightUShift(2);
            newCapacity |= newCapacity.RightUShift(4);
            newCapacity |= newCapacity.RightUShift(8);
            newCapacity |= newCapacity.RightUShift(16);
            newCapacity++;

            var newArray = new object[newCapacity];
            oldArray.CopyTo(newArray, 0);
            newArray.Fill(oldCapacity, newArray.Length - oldCapacity, Unset);
            newArray[index] = value;
            _indexedVariables = newArray;
        }

        public object RemoveIndexedVariable(int index)
        {
            object[] lookup = _indexedVariables;
            if ((uint)index < (uint)lookup.Length)
            {
                object v = lookup[index];
                lookup[index] = Unset;
                return v;
            }
            else
            {
                return Unset;
            }
        }

        public bool IsIndexedVariableSet(int index)
        {
            object[] lookup = _indexedVariables;
            return (uint)index < (uint)lookup.Length && lookup[index] != Unset;
        }
    }
}