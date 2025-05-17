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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DotNetty.Common.Utilities;

    public class EmptyHeaders<TKey, TValue> : IHeaders<TKey, TValue>
        where TKey : class
    {
        static readonly NotSupportedException ReadOnlyException = new NotSupportedException("read only");

        public int Size => 0;

        public bool IsEmpty => true;

        public IHeaders<TKey, TValue> Add(TKey name, TValue value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> Add(TKey name, IEnumerable<TValue> values)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> Add(IHeaders<TKey, TValue> headers)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddBoolean(TKey name, bool value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddByte(TKey name, byte value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddChar(TKey name, char value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddDouble(TKey name, double value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddFloat(TKey name, float value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddInt(TKey name, int value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddLong(TKey name, long value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddObject(TKey name, object value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddObject(TKey name, IEnumerable<object> values)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddShort(TKey name, short value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> AddTimeMillis(TKey name, long value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> Clear()
        {
            return this;
        }

        public bool Contains(TKey name)
        {
            return false;
        }

        public bool Contains(TKey name, TValue value)
        {
            return false;
        }

        public bool ContainsBoolean(TKey name, bool value)
        {
            return false;
        }

        public bool ContainsByte(TKey name, byte value)
        {
            return false;
        }

        public bool ContainsChar(TKey name, char value)
        {
            return false;
        }

        public bool ContainsDouble(TKey name, double value)
        {
            return false;
        }

        public bool ContainsFloat(TKey name, float value)
        {
            return false;
        }

        public bool ContainsInt(TKey name, int value)
        {
            return false;
        }

        public bool ContainsLong(TKey name, long value)
        {
            return false;
        }

        public bool ContainsObject(TKey name, object value)
        {
            return false;
        }

        public bool ContainsShort(TKey name, short value)
        {
            return false;
        }

        public bool ContainsTimeMillis(TKey name, long value)
        {
            return false;
        }

        public TValue Get(TKey name, TValue defaultValue)
        {
            return default;
        }

        public IList<TValue> GetAll(TKey name)
        {
            return new List<TValue>();
        }

        public IList<TValue> GetAllAndRemove(TKey name)
        {
            return new List<TValue>();
        }

        public TValue GetAndRemove(TKey name, TValue defaultValue)
        {
            return default;
        }

        public bool GetBoolean(TKey name, bool defaultValue)
        {
            return default;
        }

        public bool GetBooleanAndRemove(TKey name, bool defaultValue)
        {
            return default;
        }

        public byte GetByte(TKey name, byte defaultValue)
        {
            return default;
        }

        public byte GetByteAndRemove(TKey name, byte defaultValue)
        {
            return default;
        }

        public char GetChar(TKey name, char defaultValue)
        {
            return default;
        }

        public char GetCharAndRemove(TKey name, char defaultValue)
        {
            return default;
        }

        public double GetDouble(TKey name, double defaultValue)
        {
            return default;
        }

        public double GetDoubleAndRemove(TKey name, double defaultValue)
        {
            return default;
        }

        public float GetFloat(TKey name, float defaultValue)
        {
            return default;
        }

        public float GetFloatAndRemove(TKey name, float defaultValue)
        {
            return default;
        }

        public int GetInt(TKey name, int defaultValue)
        {
            return default;
        }

        public int GetIntAndRemove(TKey name, int defaultValue)
        {
            return default;
        }

        public long GetLong(TKey name, long defaultValue)
        {
            return default;
        }

        public long GetLongAndRemove(TKey name, long defaultValue)
        {
            return default;
        }

        public short GetShort(TKey name, short defaultValue)
        {
            return default;
        }

        public short GetShortAndRemove(TKey name, short defaultValue)
        {
            return default;
        }

        public long GetTimeMillis(TKey name, long defaultValue)
        {
            return default;
        }

        public long GetTimeMillisAndRemove(TKey name, long defaultValue)
        {
            return default;
        }

        public ISet<TKey> Names()
        {
            return new HashSet<TKey>();
        }

        public bool Remove(TKey name)
        {
            return false;
        }

        public IHeaders<TKey, TValue> Set(TKey name, TValue value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> Set(TKey name, IEnumerable<TValue> values)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> Set(IHeaders<TKey, TValue> headers)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetAll(IHeaders<TKey, TValue> headers)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetBoolean(TKey name, bool value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetByte(TKey name, byte value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetChar(TKey name, char value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetDouble(TKey name, double value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetFloat(TKey name, float value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetInt(TKey name, int value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetLong(TKey name, long value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetObject(TKey name, object value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetObject(TKey name, IEnumerable<object> values)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetShort(TKey name, short value)
        {
            throw ReadOnlyException;
        }

        public IHeaders<TKey, TValue> SetTimeMillis(TKey name, long value)
        {
            throw ReadOnlyException;
        }

        public bool TryGet(TKey name, out TValue value)
        {
            value = default; return false;
        }

        public bool TryGetAndRemove(TKey name, out TValue value)
        {
            value = default; return false;
        }

        public bool TryGetBoolean(TKey name, out bool value)
        {
            value = default; return false;
        }

        public bool TryGetBooleanAndRemove(TKey name, out bool value)
        {
            value = default; return false;
        }

        public bool TryGetByte(TKey name, out byte value)
        {
            value = default; return false;
        }

        public bool TryGetByteAndRemove(TKey name, out byte value)
        {
            value = default; return false;
        }

        public bool TryGetChar(TKey name, out char value)
        {
            value = default; return false;
        }

        public bool TryGetCharAndRemove(TKey name, out char value)
        {
            value = default; return false;
        }

        public bool TryGetDouble(TKey name, out double value)
        {
            value = default; return false;
        }

        public bool TryGetDoubleAndRemove(TKey name, out double value)
        {
            value = default; return false;
        }

        public bool TryGetFloat(TKey name, out float value)
        {
            value = default; return false;
        }

        public bool TryGetFloatAndRemove(TKey name, out float value)
        {
            value = default; return false;
        }

        public bool TryGetInt(TKey name, out int value)
        {
            value = default; return false;
        }

        public bool TryGetIntAndRemove(TKey name, out int value)
        {
            value = default; return false;
        }

        public bool TryGetLong(TKey name, out long value)
        {
            value = default; return false;
        }

        public bool TryGetLongAndRemove(TKey name, out long value)
        {
            value = default; return false;
        }

        public bool TryGetShort(TKey name, out short value)
        {
            value = default; return false;
        }

        public bool TryGetShortAndRemove(TKey name, out short value)
        {
            value = default; return false;
        }

        public bool TryGetTimeMillis(TKey name, out long value)
        {
            value = default; return false;
        }

        public bool TryGetTimeMillisAndRemove(TKey name, out long value)
        {
            value = default; return false;
        }

        public IEnumerator<HeaderEntry<TKey, TValue>> GetEnumerator()
        {
            return System.Linq.Enumerable.Empty<HeaderEntry<TKey, TValue>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) { return true; }

            return obj is IHeaders<TKey, TValue> headers && headers.IsEmpty;
        }

        public override int GetHashCode()
        {
            return DefaultHeaders<TKey, TValue>.HashCodeSeed;
        }

        public override string ToString()
        {
            return $"{StringUtil.SimpleClassName(this.GetType())}[]";
        }
    }
}
