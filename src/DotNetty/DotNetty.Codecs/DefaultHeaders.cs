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
    using System.Collections.Immutable;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Utilities;

    using static Common.Internal.MathUtil;
    using static HeadersUtils;

    public class DefaultHeaders<TKey, TValue> : IHeaders<TKey, TValue>
        where TKey : class
    {
        internal const int HashCodeSeed = unchecked((int)0xc2b2ae35);

        private static readonly DefaultHashingStrategy<TValue> DefaultValueHashingStrategy = new DefaultHashingStrategy<TValue>();
        private static readonly DefaultHashingStrategy<TKey> DefaultKeyHashingStragety = new DefaultHashingStrategy<TKey>();
        private static readonly NullNameValidator<TKey> DefaultKeyNameValidator = NullNameValidator<TKey>.Instance;

        private readonly HeaderEntry<TKey, TValue>[] _entries;
        protected readonly HeaderEntry<TKey, TValue> _head;

        private readonly byte _hashMask;
        protected readonly IValueConverter<TValue> ValueConverter;
        private readonly INameValidator<TKey> _nameValidator;
        private readonly IHashingStrategy<TKey> _hashingStrategy;
        private int _size;

        public DefaultHeaders(IValueConverter<TValue> valueConverter)
            : this(DefaultKeyHashingStragety, valueConverter, DefaultKeyNameValidator, 16)
        {
        }

        public DefaultHeaders(IValueConverter<TValue> valueConverter, INameValidator<TKey> nameValidator)
            : this(DefaultKeyHashingStragety, valueConverter, nameValidator, 16)
        {
        }

        public DefaultHeaders(IHashingStrategy<TKey> nameHashingStrategy, IValueConverter<TValue> valueConverter, INameValidator<TKey> nameValidator)
            : this(nameHashingStrategy, valueConverter, nameValidator, 16)
        {
        }

        public DefaultHeaders(IHashingStrategy<TKey> nameHashingStrategy,
            IValueConverter<TValue> valueConverter, INameValidator<TKey> nameValidator, int arraySizeHint)
        {
            if (nameHashingStrategy is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.nameHashingStrategy);
            if (valueConverter is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.valueConverter);
            if (nameValidator is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.nameValidator);

            _hashingStrategy = nameHashingStrategy;
            ValueConverter = valueConverter;
            _nameValidator = nameValidator;

            // Enforce a bound of [2, 128] because hashMask is a byte. The max possible value of hashMask is one less
            // than the length of this array, and we want the mask to be > 0.
            _entries = new HeaderEntry<TKey, TValue>[FindNextPositivePowerOfTwo(Math.Max(2, Math.Min(arraySizeHint, 128)))];
            _hashMask = (byte)(_entries.Length - 1);
            _head = new HeaderEntry<TKey, TValue>();
        }

        public bool TryGet(TKey name, out TValue value)
        {
            if (name is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.name);

            bool found = false;
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);
            HeaderEntry<TKey, TValue> e = _entries[i];
            value = default;
            // loop until the first header was found
            while (e is object)
            {
                if (e.Hash == h && _hashingStrategy.Equals(name, e._key))
                {
                    value = e._value;
                    found = true;
                }

                e = e.Next;
            }
            return found;
        }

        public TValue Get(TKey name, TValue defaultValue) => TryGet(name, out TValue value) ? value : defaultValue;

        public bool TryGetAndRemove(TKey name, out TValue value)
        {
            if (name is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.name);

            int h = _hashingStrategy.HashCode(name);
            return TryRemove0(h, Index(h), name, out value);
        }

        public TValue GetAndRemove(TKey name, TValue defaultValue) => TryGetAndRemove(name, out TValue value) ? value : defaultValue;

        public virtual IList<TValue> GetAll(TKey name)
        {
            if (name is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.name);

            var values = new List<TValue>();
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);
            HeaderEntry<TKey, TValue> e = _entries[i];
            while (e is object)
            {
                if (e.Hash == h && _hashingStrategy.Equals(name, e._key))
                {
                    values.Insert(0, e._value);
                }

                e = e.Next;
            }
            return values;
        }

        public virtual IEnumerable<TValue> ValueIterator(TKey name) => new ValueEnumerator(this, name);

        public IList<TValue> GetAllAndRemove(TKey name)
        {
            IList<TValue> all = GetAll(name);
            _ = Remove(name);
            return all;
        }

        public bool Contains(TKey name) => TryGet(name, out _);

        public bool ContainsObject(TKey name, object value)
        {
            if (value is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.value);

            return Contains(name, ValueConverter.ConvertObject(value));
        }

        public bool ContainsBoolean(TKey name, bool value) => Contains(name, ValueConverter.ConvertBoolean(value));

        public bool ContainsByte(TKey name, byte value) => Contains(name, ValueConverter.ConvertByte(value));

        public bool ContainsChar(TKey name, char value) => Contains(name, ValueConverter.ConvertChar(value));

        public bool ContainsShort(TKey name, short value) => Contains(name, ValueConverter.ConvertShort(value));

        public bool ContainsInt(TKey name, int value) => Contains(name, ValueConverter.ConvertInt(value));

        public bool ContainsLong(TKey name, long value) => Contains(name, ValueConverter.ConvertLong(value));

        public bool ContainsFloat(TKey name, float value) => Contains(name, ValueConverter.ConvertFloat(value));

        public bool ContainsDouble(TKey name, double value) => Contains(name, ValueConverter.ConvertDouble(value));

        public bool ContainsTimeMillis(TKey name, long value) => Contains(name, ValueConverter.ConvertTimeMillis(value));

        public bool Contains(TKey name, TValue value) => Contains(name, value, DefaultValueHashingStrategy);

        public bool Contains(TKey name, TValue value, IHashingStrategy<TValue> valueHashingStrategy)
        {
            if (name is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.name);

            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);
            HeaderEntry<TKey, TValue> e = _entries[i];
            while (e is object)
            {
                if (e.Hash == h && _hashingStrategy.Equals(name, e._key)
                    && valueHashingStrategy.Equals(value, e._value))
                {
                    return true;
                }
                e = e.Next;
            }
            return false;
        }

        public int Size => _size;

        public bool IsEmpty => _head == _head.After;

        public ISet<TKey> Names()
        {
            if (IsEmpty)
            {
                return ImmutableHashSet<TKey>.Empty;
            }

            var names = new HashSet<TKey>(_hashingStrategy);
            HeaderEntry<TKey, TValue> e = _head.After;
            while (e != _head)
            {
                _ = names.Add(e._key);
                e = e.After;
            }
            return names;
        }

        public virtual IHeaders<TKey, TValue> Add(TKey name, TValue value)
        {
            if (value == null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.value);

            _nameValidator.ValidateName(name);
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);
            Add0(h, i, name, value);
            return this;
        }

        public virtual IHeaders<TKey, TValue> Add(TKey name, IEnumerable<TValue> values)
        {
            _nameValidator.ValidateName(name);
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);
            foreach (TValue v in values)
            {
                Add0(h, i, name, v);
            }
            return this;
        }

        public virtual IHeaders<TKey, TValue> AddObject(TKey name, object value)
        {
            if (value is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.value);

            return Add(name, ValueConverter.ConvertObject(value));
        }

        public virtual IHeaders<TKey, TValue> AddObject(TKey name, IEnumerable<object> values)
        {
            foreach (object value in values)
            {
                _ = AddObject(name, value);
            }
            return this;
        }

        public virtual IHeaders<TKey, TValue> AddObject(TKey name, params object[] values)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Avoid enumerator allocations
            for (int i = 0; i < values.Length; i++)
            {
                _ = AddObject(name, values[i]);
            }

            return this;
        }

        public IHeaders<TKey, TValue> AddInt(TKey name, int value) => Add(name, ValueConverter.ConvertInt(value));

        public IHeaders<TKey, TValue> AddLong(TKey name, long value) => Add(name, ValueConverter.ConvertLong(value));

        public IHeaders<TKey, TValue> AddDouble(TKey name, double value) => Add(name, ValueConverter.ConvertDouble(value));

        public IHeaders<TKey, TValue> AddTimeMillis(TKey name, long value) => Add(name, ValueConverter.ConvertTimeMillis(value));

        public IHeaders<TKey, TValue> AddChar(TKey name, char value) => Add(name, ValueConverter.ConvertChar(value));

        public IHeaders<TKey, TValue> AddBoolean(TKey name, bool value) => Add(name, ValueConverter.ConvertBoolean(value));

        public IHeaders<TKey, TValue> AddFloat(TKey name, float value) => Add(name, ValueConverter.ConvertFloat(value));

        public IHeaders<TKey, TValue> AddByte(TKey name, byte value) => Add(name, ValueConverter.ConvertByte(value));

        public IHeaders<TKey, TValue> AddShort(TKey name, short value) => Add(name, ValueConverter.ConvertShort(value));

        public virtual IHeaders<TKey, TValue> Add(IHeaders<TKey, TValue> headers)
        {
            if (ReferenceEquals(headers, this))
            {
                CThrowHelper.ThrowArgumentException_CannotAddToItSelf();
            }
            AddImpl(headers);
            return this;
        }

        protected void AddImpl(IHeaders<TKey, TValue> headers)
        {
            if (headers is DefaultHeaders<TKey, TValue> defaultHeaders)
            {
                HeaderEntry<TKey, TValue> e = defaultHeaders._head.After;

                if (defaultHeaders._hashingStrategy == _hashingStrategy
                    && defaultHeaders._nameValidator == _nameValidator)
                {
                    // Fastest copy
                    while (e != defaultHeaders._head)
                    {
                        Add0(e.Hash, Index(e.Hash), e._key, e._value);
                        e = e.After;
                    }
                }
                else
                {
                    // Fast copy
                    while (e != defaultHeaders._head)
                    {
                        _ = Add(e._key, e._value);
                        e = e.After;
                    }
                }
            }
            else
            {
                // Slow copy
                foreach (HeaderEntry<TKey, TValue> header in headers)
                {
                    _ = Add(header._key, header._value);
                }
            }
        }

        public IHeaders<TKey, TValue> Set(TKey name, TValue value)
        {
            if (value == null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.value);

            _nameValidator.ValidateName(name);
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);
            _ = TryRemove0(h, i, name, out _);
            Add0(h, i, name, value);
            return this;
        }

        public virtual IHeaders<TKey, TValue> Set(TKey name, IEnumerable<TValue> values)
        {
            if (values is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.values);

            _nameValidator.ValidateName(name);
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);

            _ = TryRemove0(h, i, name, out _);
            // ReSharper disable once PossibleNullReferenceException
            foreach (TValue v in values)
            {
                if (v == null)
                {
                    break;
                }
                Add0(h, i, name, v);
            }

            return this;
        }

        public virtual IHeaders<TKey, TValue> SetObject(TKey name, object value)
        {
            if (value is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.value);

            TValue convertedValue = ValueConverter.ConvertObject(value);
            return Set(name, convertedValue);
        }

        public virtual IHeaders<TKey, TValue> SetObject(TKey name, IEnumerable<object> values)
        {
            if (values is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.values);

            _nameValidator.ValidateName(name);
            int h = _hashingStrategy.HashCode(name);
            int i = Index(h);

            _ = TryRemove0(h, i, name, out _);
            // ReSharper disable once PossibleNullReferenceException
            foreach (object v in values)
            {
                if (v is null)
                {
                    break;
                }
                Add0(h, i, name, ValueConverter.ConvertObject(v));
            }

            return this;
        }

        public IHeaders<TKey, TValue> SetInt(TKey name, int value) => Set(name, ValueConverter.ConvertInt(value));

        public IHeaders<TKey, TValue> SetLong(TKey name, long value) => Set(name, ValueConverter.ConvertLong(value));

        public IHeaders<TKey, TValue> SetDouble(TKey name, double value) => Set(name, ValueConverter.ConvertDouble(value));

        public IHeaders<TKey, TValue> SetTimeMillis(TKey name, long value) => Set(name, ValueConverter.ConvertTimeMillis(value));

        public IHeaders<TKey, TValue> SetFloat(TKey name, float value) => Set(name, ValueConverter.ConvertFloat(value));

        public IHeaders<TKey, TValue> SetChar(TKey name, char value) => Set(name, ValueConverter.ConvertChar(value));

        public IHeaders<TKey, TValue> SetBoolean(TKey name, bool value) => Set(name, ValueConverter.ConvertBoolean(value));

        public IHeaders<TKey, TValue> SetByte(TKey name, byte value) => Set(name, ValueConverter.ConvertByte(value));

        public IHeaders<TKey, TValue> SetShort(TKey name, short value) => Set(name, ValueConverter.ConvertShort(value));

        public virtual IHeaders<TKey, TValue> Set(IHeaders<TKey, TValue> headers)
        {
            if (!ReferenceEquals(headers, this))
            {
                _ = Clear();
                AddImpl(headers);
            }
            return this;
        }

        public virtual IHeaders<TKey, TValue> SetAll(IHeaders<TKey, TValue> headers)
        {
            if (!ReferenceEquals(headers, this))
            {
                foreach (TKey key in headers.Names())
                {
                    _ = Remove(key);
                }
                AddImpl(headers);
            }
            return this;
        }

        public bool Remove(TKey name) => TryGetAndRemove(name, out _);

        public virtual IHeaders<TKey, TValue> Clear()
        {
            Array.Clear(_entries, 0, _entries.Length);
            _head.Before = _head.After = _head;
            _size = 0;
            return this;
        }

        public IEnumerator<HeaderEntry<TKey, TValue>> GetEnumerator() => new HeaderEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetBoolean(TKey name, out bool value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToBoolean(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public bool GetBoolean(TKey name, bool defaultValue) => TryGetBoolean(name, out bool value) ? value : defaultValue;

        public bool TryGetByte(TKey name, out byte value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToByte(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public byte GetByte(TKey name, byte defaultValue) => TryGetByte(name, out byte value) ? value : defaultValue;

        public bool TryGetChar(TKey name, out char value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToChar(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public char GetChar(TKey name, char defaultValue) => TryGetChar(name, out char value) ? value : defaultValue;

        public bool TryGetShort(TKey name, out short value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToShort(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public short GetShort(TKey name, short defaultValue) => TryGetShort(name, out short value) ? value : defaultValue;

        public bool TryGetInt(TKey name, out int value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToInt(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public int GetInt(TKey name, int defaultValue) => TryGetInt(name, out int value) ? value : defaultValue;

        public bool TryGetLong(TKey name, out long value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToLong(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public long GetLong(TKey name, long defaultValue) => TryGetLong(name, out long value) ? value : defaultValue;

        public bool TryGetFloat(TKey name, out float value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToFloat(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public float GetFloat(TKey name, float defaultValue) => TryGetFloat(name, out float value) ? value : defaultValue;

        public bool TryGetDouble(TKey name, out double value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToDouble(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public double GetDouble(TKey name, double defaultValue) => TryGetDouble(name, out double value) ? value : defaultValue;

        public bool TryGetTimeMillis(TKey name, out long value)
        {
            if (TryGet(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToTimeMillis(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public long GetTimeMillis(TKey name, long defaultValue) => TryGetTimeMillis(name, out long value) ? value : defaultValue;

        public bool TryGetBooleanAndRemove(TKey name, out bool value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToBoolean(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public bool GetBooleanAndRemove(TKey name, bool defaultValue) => TryGetBooleanAndRemove(name, out bool value) ? value : defaultValue;

        public bool TryGetByteAndRemove(TKey name, out byte value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToByte(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
            value = default;
            return false;
        }

        public byte GetByteAndRemove(TKey name, byte defaultValue) => TryGetByteAndRemove(name, out byte value) ? value : defaultValue;

        public bool TryGetCharAndRemove(TKey name, out char value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToChar(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public char GetCharAndRemove(TKey name, char defaultValue) => TryGetCharAndRemove(name, out char value) ? value : defaultValue;

        public bool TryGetShortAndRemove(TKey name, out short value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToShort(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public short GetShortAndRemove(TKey name, short defaultValue) => TryGetShortAndRemove(name, out short value) ? value : defaultValue;

        public bool TryGetIntAndRemove(TKey name, out int value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToInt(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public int GetIntAndRemove(TKey name, int defaultValue) => TryGetIntAndRemove(name, out int value) ? value : defaultValue;

        public bool TryGetLongAndRemove(TKey name, out long value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToLong(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public long GetLongAndRemove(TKey name, long defaultValue) => TryGetLongAndRemove(name, out long value) ? value : defaultValue;

        public bool TryGetFloatAndRemove(TKey name, out float value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToFloat(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public float GetFloatAndRemove(TKey name, float defaultValue) => TryGetFloatAndRemove(name, out float value) ? value : defaultValue;

        public bool TryGetDoubleAndRemove(TKey name, out double value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToDouble(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public double GetDoubleAndRemove(TKey name, double defaultValue) => TryGetDoubleAndRemove(name, out double value) ? value : defaultValue;

        public bool TryGetTimeMillisAndRemove(TKey name, out long value)
        {
            if (TryGetAndRemove(name, out TValue v))
            {
                try
                {
                    value = ValueConverter.ConvertToTimeMillis(v);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            value = default;
            return false;
        }

        public long GetTimeMillisAndRemove(TKey name, long defaultValue) => TryGetTimeMillisAndRemove(name, out long value) ? value : defaultValue;

        public override bool Equals(object obj) => obj is IHeaders<TKey, TValue> headers && Equals(headers, DefaultValueHashingStrategy);

        public override int GetHashCode() => HashCode(DefaultValueHashingStrategy);

        public bool Equals(IHeaders<TKey, TValue> h2, IHashingStrategy<TValue> valueHashingStrategy)
        {
            if (h2.Size != _size)
            {
                return false;
            }

            if (ReferenceEquals(this, h2))
            {
                return true;
            }

            foreach (TKey name in Names())
            {
                IList<TValue> otherValues = h2.GetAll(name);
                IList<TValue> values = GetAll(name);
                if (otherValues.Count != values.Count)
                {
                    return false;
                }
                for (int i = 0; i < otherValues.Count; i++)
                {
                    if (!valueHashingStrategy.Equals(otherValues[i], values[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int HashCode(IHashingStrategy<TValue> valueHashingStrategy)
        {
            int result = HashCodeSeed;
            foreach (TKey name in Names())
            {
                result = 31 * result + _hashingStrategy.HashCode(name);
                IList<TValue> values = GetAll(name);
                for (int i = 0; i < values.Count; ++i)
                {
                    result = 31 * result + valueHashingStrategy.HashCode(values[i]);
                }
            }
            return result;
        }

        public override string ToString() => HeadersUtils.ToString(this, _size);

        protected virtual HeaderEntry<TKey, TValue> NewHeaderEntry(int h, TKey name, TValue value, HeaderEntry<TKey, TValue> next) =>
            new HeaderEntry<TKey, TValue>(h, name, value, next, _head);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        int Index(int hash) => hash & _hashMask;

        void Add0(int h, int i, TKey name, TValue value)
        {
            // Update the hash table.
            _entries[i] = NewHeaderEntry(h, name, value, _entries[i]);
            ++_size;
        }

        bool TryRemove0(int h, int i, TKey name, out TValue value)
        {
            value = default;

            HeaderEntry<TKey, TValue> e = _entries[i];
            if (e is null)
            {
                return false;
            }

            bool result = false;

            HeaderEntry<TKey, TValue> next = e.Next;
            while (next is object)
            {
                if (next.Hash == h && _hashingStrategy.Equals(name, next._key))
                {
                    value = next._value;
                    e.Next = next.Next;
                    next.Remove();
                    --_size;
                    result = true;
                }
                else
                {
                    e = next;
                }

                next = e.Next;
            }

            e = _entries[i];
            if (e.Hash == h && _hashingStrategy.Equals(name, e._key))
            {
                if (!result)
                {
                    value = e._value;
                    result = true;
                }
                _entries[i] = e.Next;
                e.Remove();
                --_size;
            }

            return result;
        }

        public DefaultHeaders<TKey, TValue> Copy()
        {
            var copy = new DefaultHeaders<TKey, TValue>(_hashingStrategy, ValueConverter, _nameValidator, _entries.Length);
            copy.AddImpl(this);
            return copy;
        }

        struct ValueEnumerator : IEnumerator<TValue>, IEnumerable<TValue>
        {
            private readonly IHashingStrategy<TKey> _hashingStrategy;
            private readonly int _hash;
            private readonly TKey _name;
            private readonly HeaderEntry<TKey, TValue> _head;
            private HeaderEntry<TKey, TValue> _node;
            private TValue _current;

            public ValueEnumerator(DefaultHeaders<TKey, TValue> headers, TKey name)
            {
                if (name is null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.name);

                _hashingStrategy = headers._hashingStrategy;
                _hash = _hashingStrategy.HashCode(name);
                _name = name;
                _node = _head = headers._entries[headers.Index(_hash)];
                _current = default;
            }

            bool IEnumerator.MoveNext()
            {
                if (_node is null)
                {
                    return false;
                }

                _current = _node._value;
                CalculateNext(_node.Next);
                return true;
            }

            void CalculateNext(HeaderEntry<TKey, TValue> entry)
            {
                while (entry is object)
                {
                    if (entry.Hash == _hash && _hashingStrategy.Equals(_name, entry._key))
                    {
                        _node = entry;
                        return;
                    }
                    entry = entry.Next;
                }
                _node = null;
            }

            TValue IEnumerator<TValue>.Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset()
            {
                _node = _head;
                _current = default;
            }

            void IDisposable.Dispose()
            {
                _node = null;
                _current = default;
            }

            public IEnumerator<TValue> GetEnumerator() => this;

            IEnumerator IEnumerable.GetEnumerator() => this;
        }

        struct HeaderEnumerator : IEnumerator<HeaderEntry<TKey, TValue>>
        {
            private readonly HeaderEntry<TKey, TValue> _head;
            private readonly int _size;

            private HeaderEntry<TKey, TValue> _node;
            private int _index;

            public HeaderEnumerator(DefaultHeaders<TKey, TValue> headers)
            {
                _head = headers._head;
                _size = headers._size;
                _node = _head;
                _index = 0;
            }

            public HeaderEntry<TKey, TValue> Current => _node;

            object IEnumerator.Current
            {
                [MethodImpl(InlineMethod.AggressiveInlining)]
                get
                {
                    if (0u >= (uint)_index || _index == _size + 1)
                    {
                        CThrowHelper.ThrowInvalidOperationException_EnumeratorNotInitOrCompleted();
                    }
                    return _node;
                }
            }

            public bool MoveNext()
            {
                if (_node is null)
                {
                    _index = _size + 1;
                    return false;
                }

                _index++;
                _node = _node.After;
                if (_node == _head)
                {
                    _node = null;
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                _node = _head.After;
                _index = 0;
            }

            public void Dispose()
            {
                _node = null;
                _index = 0;
            }
        }
    }

    public class HeaderEntry<TKey, TValue>
        where TKey : class
    {
        internal readonly int Hash;
        // ReSharper disable InconsistentNaming
        internal readonly TKey _key;
        internal protected TValue _value;
        // ReSharper restore InconsistentNaming
        private readonly bool _isReadonly;

        public HeaderEntry(int hash, TKey key)
        {
            Hash = hash;
            _key = key;
        }

        public HeaderEntry(TKey key, TValue value, bool isReadonly)
        {
            _key = key;
            _value = value;
            _isReadonly = isReadonly;
        }

        internal HeaderEntry()
        {
            Hash = -1;
            _key = default;
            Before = this;
            After = this;
        }

        internal HeaderEntry(int hash, TKey key, TValue value,
            HeaderEntry<TKey, TValue> next, HeaderEntry<TKey, TValue> head)
        {
            Hash = hash;
            _key = key;
            _value = value;
            Next = next;

            After = head;
            Before = head.Before;
            // PointNeighborsToThis
            Before.After = this;
            After.Before = this;
        }

        public virtual void Remove()
        {
            if (_isReadonly) { CThrowHelper.ThrowNotSupportedException_Readonly(); }
            Before.After = After;
            After.Before = Before;
        }

        public HeaderEntry<TKey, TValue> After { get; protected internal set; }
        public HeaderEntry<TKey, TValue> Before { get; protected internal set; }
        public HeaderEntry<TKey, TValue> Next { get; protected internal set; }

        public TKey Key => _key;

        public TValue Value => _value;

        public TValue SetValue(TValue newValue)
        {
            if (_isReadonly) { CThrowHelper.ThrowNotSupportedException_Readonly(); }
            if (newValue == null) CThrowHelper.ThrowArgumentNullException(CExceptionArgument.newValue);

            TValue oldValue = _value;
            _value = newValue;
            return oldValue;
        }

        protected void PointNeighborsToThis()
        {
            Before.After = this;
            After.Before = this;
        }

        public override string ToString() => $"{_key}={_value}";

        // ReSharper disable once MergeConditionalExpression
        public override bool Equals(object obj) => obj is HeaderEntry<TKey, TValue> other
            && (_key is null ? other._key is null : _key.Equals(other._key))
            && (_value == null ? other._value == null : _value.Equals(other._value));

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => (_key is null ? 0 : _key.GetHashCode())
                ^ (_value == null ? 0 : _value.GetHashCode());
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
